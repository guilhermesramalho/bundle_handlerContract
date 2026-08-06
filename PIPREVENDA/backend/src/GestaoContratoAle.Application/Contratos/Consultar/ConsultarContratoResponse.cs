using PortalAle.Domain.Contratos;

namespace PortalAle.Application.Contratos.Consultar;

public record EventoContratoItem(int Id, TipoEventoContrato Tipo, DateOnly Data, string Descricao);

/// <summary>
/// Detalhe de um contrato. Campos de galonagem/situação são calculados na query
/// (mesma fórmula de Contrato.CalcularGalonagem/GalonagemCalculo), não persistidos.
/// Sem dado jurídico (Elaw) ou de vencimento projetado (depende de série histórica de
/// faturamento mensal, que só existirá com a integração SAP/PCR — Fase 5) — ver
/// PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 2.
/// </summary>
public record ConsultarContratoResponse(
    int Id,
    string ContratoId,
    int ClienteId,
    string ClienteNome,
    string ClienteCnpj,
    string? ClienteNrSap,
    string? ClienteUf,
    int? GrupoEconomicoId,
    string? GrupoEconomicoNome,
    string Pcr,
    Segmento Segmento,
    TipoContrato Tipo,
    SituacaoMes SituacaoMes,
    string Bandeira,
    bool RegistradoAle,
    DateOnly? DataRegistroAnp,
    DateOnly InicioVigencia,
    DateOnly FimVigencia,
    decimal VolumeMensalContratado,
    decimal GalonagemContratada,
    decimal GalonagemFaturada,
    decimal GalonagemSaldo,
    decimal GalonagemPercentual,
    SituacaoPcr SituacaoPcr,
    decimal MargemBase,
    decimal? TirContratada,
    bool Greenfield,
    bool Denuncia,
    bool Garantia,
    bool Sublocado,
    bool Encerrado,
    PapelGuardaChuva? PapelGuardaChuva,
    string? CodigoGuardaChuva,
    string Diretoria,
    string RegionalVendas,
    string PontoVenda,
    string Consultor,
    string? Observacao,
    string? Clausula,
    string? CnpjSucedido,
    string? RazaoSucedido,
    DateTimeOffset DataCriacao,
    DateTimeOffset? DataAlteracao,
    IReadOnlyList<EventoContratoItem> Eventos);
