using Microsoft.EntityFrameworkCore;
using PortalAle.Application.Base;
using PortalAle.Application.Contratos;
using PortalAle.Application.Contratos.Consultar;
using PortalAle.Application.Contratos.Listar;
using PortalAle.Data.Extensions;
using PortalAle.Domain.Contratos;

namespace PortalAle.Data.SqlServer.Persistencia.Queries;

/// <summary>
/// Implementação de leitura (read-only, otimizada) para Contrato. Ver DT-019 e
/// PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 5.2.
///
/// Galonagem/situação da PCR são calculadas aqui reaproveitando a MESMA fórmula de
/// <see cref="GalonagemCalculo"/>/<see cref="Contrato.CalcularGalonagem"/> — não é
/// possível chamar o método de domínio dentro de uma query LINQ-to-Entities (não
/// traduzível para SQL), então a fórmula é reescrita como expressão. A decomposição em
/// múltiplos `Select` encadeados existe porque expression trees não permitem variáveis
/// locais (`var x = ...;`) dentro do corpo — cada estágio expõe os campos computados do
/// anterior como propriedades para o próximo poder referenciá-los.
///
/// Ordenação NÃO usa o padrão genérico <c>ApplySorting&lt;T&gt;</c> (Expression&lt;Func&lt;T,
/// object&gt;&gt; com boxing) usado em ClienteQueries — descoberto rodando a query de verdade
/// que o EF Core não traduz um `OrderBy` boxado a `object` quando T é um record já projetado
/// com membros calculados por condicionais aninhadas ("could not be translated"). Em vez
/// disso, a ordenação é aplicada ANTES da projeção final (sobre o estágio intermediário, com
/// membros simples) via switch com `OrderBy`/`OrderByDescending` fortemente tipados por caso.
/// </summary>
public class ContratoQueries(ApplicationDbContext context) : IContratoQueries
{

    public async Task<ConsultarContratoResponse?> ConsultarContratoPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        DateOnly hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        Contrato? contrato = await context.Contratos
            .AsNoTracking()
            .Include(c => c.Cliente)
            .ThenInclude(cliente => cliente!.GrupoEconomico)
            .Include(c => c.Eventos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (contrato is null)
            return null;

        var (situacao, galonagemContratada, saldo, percentual) = contrato.CalcularGalonagem(hoje);
        bool isB2B = EhSegmentoB2B(contrato.Segmento);

        return new ConsultarContratoResponse(
            contrato.Id,
            (isB2B ? "PCF " : "PCR ") + contrato.Pcr,
            contrato.ClienteId,
            contrato.Cliente!.Nome,
            contrato.Cliente.Cnpj,
            contrato.Cliente.NrSap,
            contrato.Cliente.Uf,
            contrato.Cliente.GrupoEconomicoId,
            contrato.Cliente.GrupoEconomico?.Nome,
            contrato.Pcr,
            contrato.Segmento,
            contrato.Tipo,
            contrato.SituacaoMes,
            contrato.Bandeira,
            contrato.RegistradoAle,
            contrato.DataRegistroAnp,
            contrato.InicioVigencia,
            contrato.FimVigencia,
            contrato.VolumeMensalContratado,
            galonagemContratada,
            contrato.GalonagemFaturada,
            saldo,
            percentual,
            situacao,
            contrato.MargemBase,
            contrato.TirContratada,
            contrato.Greenfield,
            contrato.Denuncia,
            contrato.Garantia,
            contrato.Sublocado,
            contrato.Encerrado,
            contrato.PapelGuardaChuva,
            contrato.CodigoGuardaChuva,
            contrato.Diretoria,
            contrato.RegionalVendas,
            contrato.PontoVenda,
            contrato.Consultor,
            contrato.Observacao,
            contrato.Clausula,
            contrato.CnpjSucedido,
            contrato.RazaoSucedido,
            contrato.DataCriacao,
            contrato.DataAlteracao,
            [.. contrato.Eventos
                .OrderByDescending(e => e.Data)
                .Select(e => new EventoContratoItem(e.Id, e.Tipo, e.Data, e.Descricao))]);
    }

    public async Task<PaginationResponse<ListarContratosResponse>> ListarContratosAsync(
        ListarContratosRequest request,
        CancellationToken cancellationToken = default)
    {
        DateOnly hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        IQueryable<Contrato> query = context.Contratos.AsNoTracking();

        if (request.Segmentos is { Length: > 0 })
            query = query.Where(c => request.Segmentos.Contains(c.Segmento));

        if (request.Tipos is { Length: > 0 })
            query = query.Where(c => request.Tipos.Contains(c.Tipo));

        if (request.EmDenuncia.HasValue)
            query = query.Where(c => c.Denuncia == request.EmDenuncia.Value);

        if (!string.IsNullOrWhiteSpace(request.Bandeira))
            query = query.Where(c => c.Bandeira == request.Bandeira);

        if (!string.IsNullOrWhiteSpace(request.Diretoria))
            query = query.Where(c => c.Diretoria == request.Diretoria);

        if (!string.IsNullOrWhiteSpace(request.RegionalVendas))
            query = query.Where(c => c.RegionalVendas == request.RegionalVendas);

        if (!string.IsNullOrWhiteSpace(request.PontoVenda))
            query = query.Where(c => c.PontoVenda == request.PontoVenda);

        if (!string.IsNullOrWhiteSpace(request.Consultor))
            query = query.Where(c => c.Consultor == request.Consultor);

        if (request.Ufs is { Length: > 0 })
            query = query.Where(c => c.Cliente != null && c.Cliente.Uf != null && request.Ufs.Contains(c.Cliente.Uf));

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            string busca = request.Busca.Trim();
            query = query.Where(c =>
                (c.Cliente != null && c.Cliente.Nome.Contains(busca)) ||
                (c.Cliente != null && c.Cliente.Cnpj.Contains(busca)) ||
                (c.Cliente != null && c.Cliente.NrSap != null && c.Cliente.NrSap.Contains(busca)) ||
                (c.Cliente != null && c.Cliente.GrupoEconomico != null && c.Cliente.GrupoEconomico.Nome.Contains(busca)) ||
                c.Pcr.Contains(busca));
        }

        // Estágio 1: campos crus + meses de vigência (mínimo 1 mês, mesma regra de Contrato.CalcularGalonagem).
        var estagio1 = query.Select(c => new
        {
            c.Id,
            c.Pcr,
            c.Segmento,
            c.Tipo,
            c.FimVigencia,
            c.Denuncia,
            c.Bandeira,
            c.Diretoria,
            c.RegionalVendas,
            c.PontoVenda,
            c.Consultor,
            c.PapelGuardaChuva,
            c.VolumeMensalContratado,
            c.GalonagemFaturada,
            ClienteNome = c.Cliente!.Nome,
            ClienteCnpj = c.Cliente.Cnpj,
            ClienteNrSap = c.Cliente.NrSap,
            ClienteUf = c.Cliente.Uf,
            GrupoEconomicoNome = c.Cliente.GrupoEconomico != null ? c.Cliente.GrupoEconomico.Nome : null,
            MesesVigencia = EF.Functions.DateDiffMonth(c.InicioVigencia, c.FimVigencia) < 1
                ? 1
                : EF.Functions.DateDiffMonth(c.InicioVigencia, c.FimVigencia),
        });

        // Estágio 2: galonagem contratada (VolumeMensalContratado * MesesVigencia).
        var estagio2 = estagio1.Select(x => new
        {
            x.Id,
            x.Pcr,
            x.Segmento,
            x.Tipo,
            x.FimVigencia,
            x.Denuncia,
            x.Bandeira,
            x.Diretoria,
            x.RegionalVendas,
            x.PontoVenda,
            x.Consultor,
            x.PapelGuardaChuva,
            x.ClienteNome,
            x.ClienteCnpj,
            x.ClienteNrSap,
            x.ClienteUf,
            x.GrupoEconomicoNome,
            x.GalonagemFaturada,
            GalonagemContratada = x.VolumeMensalContratado * x.MesesVigencia,
        });

        // Estágio 3: percentual cumprido + saldo.
        var estagio3 = estagio2.Select(y => new
        {
            y.Id,
            y.Pcr,
            y.Segmento,
            y.Tipo,
            y.FimVigencia,
            y.Denuncia,
            y.Bandeira,
            y.Diretoria,
            y.RegionalVendas,
            y.PontoVenda,
            y.Consultor,
            y.PapelGuardaChuva,
            y.ClienteNome,
            y.ClienteCnpj,
            y.ClienteNrSap,
            y.ClienteUf,
            y.GrupoEconomicoNome,
            y.GalonagemContratada,
            y.GalonagemFaturada,
            GalonagemPercentual = y.GalonagemContratada == 0 ? 0 : y.GalonagemFaturada / y.GalonagemContratada,
            GalonagemSaldo = (y.GalonagemContratada - y.GalonagemFaturada) < 0 ? 0 : (y.GalonagemContratada - y.GalonagemFaturada),
        });

        // Filtro por situação da PCR (calculada): precisa acontecer sobre estagio3 (campos
        // ainda simples: GalonagemPercentual/GalonagemSaldo/FimVigencia), não sobre o record
        // final já projetado — filtrar em cima de um record com um membro `enum` calculado por
        // uma expressão condicional grande não é traduzível pelo EF Core ("could not be
        // translated", descoberto rodando a query de verdade). Os bools abaixo são capturados
        // do lado do cliente (não fazem parte da query), then combinados com as mesmas
        // comparações usadas no cálculo de SituacaoPcr no Select final.
        if (request.SituacoesPcr is { Length: > 0 })
        {
            bool quiserVigente = request.SituacoesPcr.Contains(SituacaoPcr.Vigente);
            bool quiserVencidoGalonagem = request.SituacoesPcr.Contains(SituacaoPcr.VencidoPorGalonagem);
            bool quiserVencidoData = request.SituacoesPcr.Contains(SituacaoPcr.VencidoPorData);

            estagio3 = estagio3.Where(y =>
                (quiserVencidoGalonagem && (y.GalonagemPercentual >= 1 || y.GalonagemSaldo <= 0)) ||
                (quiserVencidoData && !(y.GalonagemPercentual >= 1 || y.GalonagemSaldo <= 0) && y.FimVigencia < hoje) ||
                (quiserVigente && !(y.GalonagemPercentual >= 1 || y.GalonagemSaldo <= 0) && !(y.FimVigencia < hoje)));
        }

        // Ordenação sobre estagio3 (membros simples), ANTES da projeção final — ver nota na
        // documentação da classe sobre por que não ordenar depois do Select final.
        bool desc = request.IsSortDescending();
        var estagio3Ordenado = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "razao" => desc ? estagio3.OrderByDescending(y => y.ClienteNome) : estagio3.OrderBy(y => y.ClienteNome),
            "grupo" => desc ? estagio3.OrderByDescending(y => y.GrupoEconomicoNome) : estagio3.OrderBy(y => y.GrupoEconomicoNome),
            "segmento" => desc ? estagio3.OrderByDescending(y => y.Segmento) : estagio3.OrderBy(y => y.Segmento),
            "galpct" => desc ? estagio3.OrderByDescending(y => y.GalonagemPercentual) : estagio3.OrderBy(y => y.GalonagemPercentual),
            "fim" => desc ? estagio3.OrderByDescending(y => y.FimVigencia) : estagio3.OrderBy(y => y.FimVigencia),
            // Default/"contratoId": ordena por Pcr (mesma ordenação textual do protótipo).
            _ => desc ? estagio3.OrderByDescending(y => y.Pcr) : estagio3.OrderBy(y => y.Pcr),
        };

        // Estágio final: situação da PCR (mesma regra de GalonagemCalculo.CalcularSituacao) + prefixo PCR/PCF.
        IQueryable<ListarContratosResponse> projecaoFinal = estagio3Ordenado.Select(z => new ListarContratosResponse(
            z.Id,
            (z.Segmento == Segmento.Cofa || z.Segmento == Segmento.Cofd || z.Segmento == Segmento.Cofar
                || z.Segmento == Segmento.Spot || z.Segmento == Segmento.Outros ? "PCF " : "PCR ") + z.Pcr,
            z.Pcr,
            z.ClienteNome,
            z.ClienteCnpj,
            z.ClienteNrSap,
            z.ClienteUf,
            z.GrupoEconomicoNome,
            z.Segmento,
            z.Tipo,
            z.GalonagemContratada,
            z.GalonagemFaturada,
            z.GalonagemPercentual,
            (z.GalonagemPercentual >= 1 || z.GalonagemSaldo <= 0)
                ? SituacaoPcr.VencidoPorGalonagem
                : (z.FimVigencia < hoje ? SituacaoPcr.VencidoPorData : SituacaoPcr.Vigente),
            z.FimVigencia,
            z.Denuncia,
            z.Bandeira,
            z.Diretoria,
            z.RegionalVendas,
            z.PontoVenda,
            z.Consultor,
            z.PapelGuardaChuva));

        return await projecaoFinal
            .ToPagedResponseAsync(request.NormalizedPageIndex, request.NormalizedPageSize, cancellationToken);
    }

    private static bool EhSegmentoB2B(Segmento segmento)
        => segmento is Segmento.Cofa or Segmento.Cofd or Segmento.Cofar or Segmento.Spot or Segmento.Outros;
}
