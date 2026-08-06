using PortalAle.Application.Base;

namespace PortalAle.Application.Contratos.AtualizarGalonagemFaturada;

/// <summary>
/// Uso interno/futuro pela integração SAP/PCR (Fase 5, bloqueada). Criado agora sem
/// endpoint público — quando o Worker existir, chama este Command diretamente sem
/// precisar tocar em Contrato de novo. Ver PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 5.1.
/// </summary>
public record AtualizarGalonagemFaturadaRequest(
    int ContratoId,
    decimal NovaGalonagemFaturada) : IRequest<Result<AtualizarGalonagemFaturadaResponse>>;
