using PortalAle.Application.Base;
using PortalAle.Domain.Contratos;

namespace PortalAle.Application.Contratos.RegistrarEvento;

/// <summary>
/// Registra um evento no ciclo de vida do contrato (Renovação, Readequação, Cessão,
/// Sucessão, Denúncia ou Encerramento — NovoNegocio é automático na criação, ver
/// Contrato ctor). Um único Command cobre os 6 tipos restantes — não há necessidade
/// de um Command por tipo de evento.
/// </summary>
public record RegistrarEventoContratoRequest(
    int ContratoId,
    TipoEventoContrato Tipo,
    DateOnly Data,
    string Descricao) : IRequest<Result<RegistrarEventoContratoResponse>>;
