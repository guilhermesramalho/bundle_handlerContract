using PortalAle.Domain.Contratos;

namespace PortalAle.Application.Contratos.Listar;

/// <summary>
/// Item de resposta da listagem paginada de contratos. Sem "Jurídico" (Elaw, fora de
/// escopo) nem "Venc. projetado" (precisa de série histórica mensal de faturamento,
/// que só existe com a integração SAP/PCR — Fase 5). Ver
/// PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 2.
/// </summary>
public record ListarContratosResponse(
    int Id,
    string ContratoId,
    string Pcr,
    string ClienteNome,
    string ClienteCnpj,
    string? ClienteNrSap,
    string? ClienteUf,
    string? GrupoEconomicoNome,
    Segmento Segmento,
    TipoContrato Tipo,
    decimal GalonagemContratada,
    decimal GalonagemFaturada,
    decimal GalonagemPercentual,
    SituacaoPcr SituacaoPcr,
    DateOnly FimVigencia,
    bool Denuncia,
    string Bandeira,
    string Diretoria,
    string RegionalVendas,
    string PontoVenda,
    string Consultor,
    PapelGuardaChuva? PapelGuardaChuva);
