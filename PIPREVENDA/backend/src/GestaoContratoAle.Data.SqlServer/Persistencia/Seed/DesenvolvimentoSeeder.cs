using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PortalAle.Domain.Clientes;
using PortalAle.Domain.Contratos;
using PortalAle.Domain.GruposEconomicos;

namespace PortalAle.Data.SqlServer.Persistencia.Seed;

/// <summary>
/// Seed de desenvolvimento — porta 1:1 os 14 contratos de
/// <c>frontend/src/mocks/contratos.ts</c> (por sua vez portado do protótipo,
/// <c>docs/modelo-frontend/_extraido/template.html</c>) para o banco real,
/// via os construtores de domínio (mesma validação de uma criação normal —
/// não é um `INSERT` cru). Idempotente: só roda se a tabela `Contrato`
/// estiver vazia. Chamado só em <c>ASPNETCORE_ENVIRONMENT=Development</c>
/// (ver <c>Program.cs</c>) — nunca em produção.
///
/// É dado fictício (mesmo aviso do mock do frontend, PLANO-IMPLEMENTACAO-FRONTEND.md
/// seção 4) — não usar para validar regra de negócio com o cliente como se
/// fosse dado real.
/// </summary>
public static class DesenvolvimentoSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Contratos.AnyAsync(cancellationToken))
            return;

        Dictionary<string, GrupoEconomico> grupos = CriarGruposEconomicos();
        context.GruposEconomicos.AddRange(grupos.Values);
        await context.SaveChangesAsync(cancellationToken);

        List<SeedContrato> dados = ObterDados();

        Dictionary<int, Cliente> clientes = [];
        foreach (SeedContrato d in dados)
        {
            var cliente = new Cliente(d.Razao, ComCnpjValido(d.CnpjBase12), $"{d.Rn.Replace("RN ", string.Empty)} - {CITY_UF[d.Rn]}");
            cliente.AtribuirDadosComplementares($"1000{d.Id:D6}", CITY_UF[d.Rn], grupos[d.GrupoNome].Id);
            clientes[d.Id] = cliente;
        }
        context.Clientes.AddRange(clientes.Values);
        await context.SaveChangesAsync(cancellationToken);

        List<Contrato> contratos = [];
        foreach (SeedContrato d in dados)
        {
            var contrato = new Contrato(
                clientes[d.Id].Id,
                d.Pcr,
                d.Segmento,
                d.Tipo,
                d.SituacaoMes,
                d.Bandeira,
                d.RegistradoAle,
                ParseDataBr(d.InicioBr),
                ParseDataBr(d.FimBr),
                d.VolMensal,
                d.MargemBase,
                d.Diretoria,
                d.Gr,
                d.Rn,
                d.Consultor,
                d.Greenfield);

            if (!string.IsNullOrEmpty(d.DataAnpBr))
                contrato.DefinirRegistroAnp(ParseDataBr(d.DataAnpBr));

            if (d.Guarda is { } guarda)
                contrato.DefinirGuardaChuva(guarda.Papel, guarda.Codigo);

            if (d.Obs is not null || d.Clausula is not null)
                contrato.DefinirObservacaoEClausula(d.Obs, d.Clausula);

            if (d.CnpjSucedido is not null && d.RazaoSucedido is not null)
                contrato.DefinirSucessao(d.CnpjSucedido, d.RazaoSucedido);

            // Eventos do mock (exceto "Novo negócio" — já registrado automaticamente
            // pelo construtor, ver Contrato.cs). Preserva a ordem original.
            foreach (var evento in d.Eventos.Where(e => e.Tipo != "Novo negócio"))
                contrato.RegistrarEvento(MapearTipoEvento(evento.Tipo), ParseDataBr(evento.DataBr), evento.Desc);

            contratos.Add(contrato);
        }
        context.Contratos.AddRange(contratos);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, GrupoEconomico> CriarGruposEconomicos()
    {
        (string Nome, string Codigo)[] grupos =
        [
            ("Grupo Solaris", "101"), ("Grupo Bandeirante", "102"), ("Grupo Vale Verde", "103"),
            ("Grupo LogBR", "104"), ("Grupo AgroCer", "105"), ("Grupo Litoral", "106"),
            ("Grupo Estrada Real", "107"), ("Grupo Frota Rápida", "108"), ("Grupo Serra Azul", "109"),
            ("Grupo Cidade Nova", "110"),
        ];

        return grupos.ToDictionary(g => g.Nome, g => new GrupoEconomico(g.Codigo, g.Nome));
    }

    private static TipoEventoContrato MapearTipoEvento(string tipoMock) => tipoMock switch
    {
        "Renovação" => TipoEventoContrato.Renovacao,
        "Readequação" => TipoEventoContrato.Readequacao,
        "Cessão" => TipoEventoContrato.Cessao,
        "Sucessão" => TipoEventoContrato.Sucessao,
        "Denúncia" => TipoEventoContrato.Denuncia,
        "Encerramento" => TipoEventoContrato.Encerramento,
        _ => throw new ArgumentOutOfRangeException(nameof(tipoMock), tipoMock, "Tipo de evento do seed sem mapeamento — conferir frontend/src/mocks/contratos.ts."),
    };

    private static DateOnly ParseDataBr(string dataBr)
        => DateOnly.ParseExact(dataBr, "dd/MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>Mesmo algoritmo de Cliente.EhCnpjValido — calcula os 2 dígitos verificadores
    /// para uma base de 12 dígitos (raiz + filial), garantindo CNPJ válido no seed.</summary>
    private static string ComCnpjValido(string base12)
    {
        int[] pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int digito1 = CalcularDigitoVerificador(base12, pesos1);
        string base13 = base12 + digito1;
        int digito2 = CalcularDigitoVerificador(base13, pesos2);
        return base13 + digito2;
    }

    private static int CalcularDigitoVerificador(string baseNumerica, int[] pesos)
    {
        int soma = 0;
        for (int i = 0; i < pesos.Length; i++)
            soma += (baseNumerica[i] - '0') * pesos[i];

        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    // Mesmo mapa cidade->UF do protótipo (frontend/src/mocks/contratos.ts CITY_UF).
    private static readonly Dictionary<string, string> CITY_UF = new()
    {
        ["RN Campinas"] = "SP", ["RN Curitiba"] = "PR", ["RN Niterói"] = "RJ", ["RN Goiânia"] = "GO",
        ["RN Rondonópolis"] = "MT", ["RN Recife"] = "PE", ["RN Belo Horizonte"] = "MG",
        ["RN Porto Alegre"] = "RS", ["RN Belém"] = "PA", ["RN Fortaleza"] = "CE",
    };

    private sealed record SeedContrato(
        int Id, string Pcr, string CnpjBase12, string Razao, string GrupoNome,
        Segmento Segmento, TipoContrato Tipo, SituacaoMes SituacaoMes, string Bandeira, bool RegistradoAle,
        string DataAnpBr, string Diretoria, string Gr, string Rn, string Consultor,
        string InicioBr, string FimBr, decimal VolMensal, decimal MargemBase, bool Greenfield,
        (PapelGuardaChuva Papel, string Codigo)? Guarda, string? Obs, string? Clausula,
        string? CnpjSucedido, string? RazaoSucedido,
        (string Tipo, string DataBr, string Desc)[] Eventos);

    /// <summary>Porte 1:1 de RAW_CONTRATOS em frontend/src/mocks/contratos.ts (14 itens).</summary>
    private static List<SeedContrato> ObterDados() =>
    [
        new(1, "700.123", "123456780001", "Auto Posto Aurora Ltda", "Grupo Solaris",
            Segmento.Rede, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira ALE", true, "12/05/2026",
            "Sudeste", "GR São Paulo Interior", "RN Campinas", "Marcos Vidal",
            "01/03/2024", "28/02/2029", 420m, 0.47m, true,
            (PapelGuardaChuva.Principal, "G1"), "Posto inaugurado em 2024, em curva de maturação.", "Compensação por metas trimestrais.",
            null, null,
            [("Novo negócio", "01/03/2024", "Assinatura do contrato PCVM — investimento em imagem e bombas."),
             ("Readequação", "10/09/2025", "Ajuste de volume mensal contratado de 380 para 420 m³.")]),

        new(2, "700.124", "123456780002", "Posto Aurora Norte", "Grupo Solaris",
            Segmento.Rede, TipoContrato.Imagem, SituacaoMes.Ativo, "Bandeira ALE", true, "12/05/2026",
            "Sudeste", "GR São Paulo Interior", "RN Campinas", "Marcos Vidal",
            "15/06/2023", "14/06/2028", 260m, 0.44m, false,
            (PapelGuardaChuva.Adicional, "G1"), "Adicional vinculado ao contrato principal Aurora.", "Sem cláusula específica.",
            null, null,
            [("Novo negócio", "15/06/2023", "Contrato adicional vinculado ao guarda-chuva Solaris.")]),

        new(3, "700.125", "123456780003", "Posto Aurora Sul (GRR)", "Grupo Solaris",
            Segmento.Grr, TipoContrato.Comodato, SituacaoMes.Ativo, "Bandeira ALE", true, "12/05/2026",
            "Sudeste", "GR São Paulo Interior", "RN Campinas", "Marcos Vidal",
            "01/02/2022", "31/12/2026", 180m, 0.41m, false,
            (PapelGuardaChuva.Adicional, "G1"), "Revenda GRR com comodato de equipamentos.", "Compensação por inadimplência de volume.",
            null, null,
            [("Novo negócio", "01/02/2022", "Início como GRR no guarda-chuva Solaris."),
             ("Renovação", "05/01/2024", "Renovação por 36 meses.")]),

        new(4, "700.210", "457782210001", "Transportes Bandeirante S.A.", "Grupo Bandeirante",
            Segmento.Cofa, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira Branca", false, "02/04/2026",
            "Sul", "GR Sul", "RN Curitiba", "Helena Rocha",
            "01/09/2023", "30/09/2026", 640m, 0.33m, false,
            (PapelGuardaChuva.Principal, "B2G"), "Cliente B2B frota — abastecimento dedicado. Guarda-chuva B2B com galonagem global do grupo.", "Compensação proporcional ao volume não cumprido.",
            null, null,
            [("Novo negócio", "01/09/2023", "Contrato COFA de fornecimento a frota.")]),

        new(5, "700.305", "339015540001", "Posto Vale Verde Ltda", "Grupo Vale Verde",
            Segmento.Rede, TipoContrato.Imagem, SituacaoMes.Ativo, "Bandeira ALE", true, "28/03/2026",
            "Sudeste", "GR Rio de Janeiro", "RN Niterói", "Paulo Tavares",
            "01/07/2021", "31/07/2026", 300m, 0.39m, false,
            null, "Performance abaixo do contratado nos últimos 6 meses.", "Cláusula de compensação acionada em 2026.",
            null, null,
            [("Novo negócio", "01/07/2021", "Contrato de imagem firmado."),
             ("Denúncia", "15/02/2026", "Denúncia contratual registrada por descumprimento de volume.")]),

        new(6, "700.412", "582201100001", "Log Cargas Brasil Ltda", "Grupo LogBR",
            Segmento.Cofd, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira Branca", false, "19/03/2026",
            "Centro-Oeste", "GR Centro-Oeste", "RN Goiânia", "Renata Lima",
            "01/04/2024", "31/03/2027", 520m, 0.31m, false,
            null, "Distribuidor com performance acima do contratado.", "Sem cláusula específica.",
            null, null,
            [("Novo negócio", "01/04/2024", "Contrato COFD de distribuição.")]),

        new(7, "700.455", "713348820001", "AgroDiesel Cerrado S.A.", "Grupo AgroCer",
            Segmento.Cofar, TipoContrato.Comodato, SituacaoMes.Ativo, "Bandeira ALE", true, "30/04/2026",
            "Centro-Oeste", "GR Centro-Oeste", "RN Rondonópolis", "Renata Lima",
            "01/06/2022", "31/05/2027", 880m, 0.29m, false,
            null, "Cliente agro com sazonalidade de safra.", "Compensação por sazonalidade prevista.",
            "88112443000155", "AgroCerrado Comércio Ltda",
            [("Sucessão", "01/06/2022", "Sucessão de AgroCerrado Comércio Ltda."),
             ("Renovação", "10/05/2025", "Renovação contratual por 24 meses.")]),

        new(8, "700.508", "905542210001", "Posto Litoral Azul Ltda", "Grupo Litoral",
            Segmento.Rede, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira ALE", true, "08/02/2026",
            "Nordeste", "GR Nordeste", "RN Recife", "Bruno Aragão",
            "01/01/2020", "31/10/2026", 240m, 0.43m, false,
            null, "Galonagem contratada já esgotada — renovação em estudo.", "Sem cláusula específica.",
            null, null,
            [("Novo negócio", "01/01/2020", "Contrato PCVM firmado."),
             ("Renovação", "15/11/2023", "Renovação por 36 meses.")]),

        new(9, "700.611", "217784430001", "Posto Estrada Real", "Grupo Estrada Real",
            Segmento.Rede, TipoContrato.Imagem, SituacaoMes.Ativo, "Bandeira ALE", true, "21/05/2026",
            "Sudeste", "GR Minas Gerais", "RN Belo Horizonte", "Camila Reis",
            "01/10/2025", "30/09/2030", 360m, 0.46m, true,
            null, "Greenfield recém-inaugurado, início da curva.", "Compensação por metas de maturação.",
            null, null,
            [("Novo negócio", "01/10/2025", "Inauguração de posto Greenfield.")]),

        new(10, "700.702", "661239980001", "Frota Rápida Spot Ltda", "Grupo Frota Rápida",
            Segmento.Spot, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira Branca", false, "14/01/2026",
            "Sul", "GR Sul", "RN Porto Alegre", "Helena Rocha",
            "01/02/2026", "31/07/2026", 300m, 0.28m, false,
            null, "Operação Spot de curta duração.", "Sem cláusula específica.",
            null, null,
            [("Novo negócio", "01/02/2026", "Contrato Spot de 6 meses.")]),

        new(11, "700.805", "489012230001", "Mineração Serra Azul S.A.", "Grupo Serra Azul",
            Segmento.Cofa, TipoContrato.Comodato, SituacaoMes.Ativo, "Bandeira ALE", true, "27/04/2026",
            "Norte", "GR Norte", "RN Belém", "Diego Fontes",
            "01/05/2023", "30/04/2027", 1200m, 0.30m, false,
            null, "Grande consumidor de diesel para mineração.", "Compensação por take-or-pay.",
            null, null,
            [("Novo negócio", "01/05/2023", "Contrato COFA take-or-pay."),
             ("Readequação", "20/01/2026", "Readequação de volume por expansão da mina.")]),

        new(12, "700.901", "156670010001", "Posto Cidade Nova", "Grupo Cidade Nova",
            Segmento.Rede, TipoContrato.Pcvm, SituacaoMes.Inativo, "Bandeira Branca", false, "05/12/2025",
            "Nordeste", "GR Nordeste", "RN Fortaleza", "Bruno Aragão",
            "01/01/2019", "31/01/2026", 200m, 0.40m, false,
            null, "Prazo contratual expirado sem cumprimento da galonagem — posto migrou para outra bandeira.", "Cláusula de compensação em discussão.",
            null, null,
            [("Novo negócio", "01/01/2019", "Contrato PCVM firmado."),
             ("Denúncia", "10/12/2025", "Denúncia por mudança de bandeira."),
             ("Encerramento", "31/01/2026", "Encerramento do contrato.")]),

        new(13, "700.211", "457782210002", "Bandeirante Log — Filial Paraná", "Grupo Bandeirante",
            Segmento.Cofa, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira Branca", false, "02/04/2026",
            "Sul", "GR Sul", "RN Curitiba", "Helena Rocha",
            "01/09/2023", "30/09/2026", 380m, 0.32m, false,
            (PapelGuardaChuva.Adicional, "B2G"), "Filial B2B do guarda-chuva Bandeirante — galonagem global do grupo.", "CNPJs solidários no cumprimento da galonagem global.",
            null, null,
            [("Novo negócio", "01/09/2023", "Adicional COFA vinculado ao guarda-chuva B2B.")]),

        new(14, "700.212", "457782210003", "Bandeirante Log — Filial Santa Catarina", "Grupo Bandeirante",
            Segmento.Cofd, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira Branca", false, "02/04/2026",
            "Sul", "GR Sul", "RN Curitiba", "Helena Rocha",
            "01/09/2023", "30/09/2026", 300m, 0.30m, false,
            (PapelGuardaChuva.Adicional, "B2G"), "Filial B2B do guarda-chuva Bandeirante — galonagem global do grupo.", "CNPJs solidários no cumprimento da galonagem global.",
            null, null,
            [("Novo negócio", "01/09/2023", "Adicional COFD vinculado ao guarda-chuva B2B.")]),
    ];
}
