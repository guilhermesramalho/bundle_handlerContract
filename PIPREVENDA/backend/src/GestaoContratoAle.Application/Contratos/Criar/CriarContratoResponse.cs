using PortalAle.Domain.Contratos;

namespace PortalAle.Application.Contratos.Criar;

/// <summary>
/// Response da operação de criação de contrato.
/// </summary>
public record CriarContratoResponse(
    int Id,
    int ClienteId,
    string Pcr,
    Segmento Segmento,
    TipoContrato Tipo,
    DateOnly InicioVigencia,
    DateOnly FimVigencia,
    DateTimeOffset DataCriacao);
