using PortalAle.Application.Base;

namespace PortalAle.Application.Contratos.Consultar;

public record ConsultarContratoRequest(int Id) : IRequest<Result<ConsultarContratoResponse>>;
