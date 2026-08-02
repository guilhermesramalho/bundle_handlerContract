using PortalAle.Application.Base;

namespace PortalAle.Application.Clientes.Consultar;

/// <summary>
/// Request de leitura para consultar um cliente por Id.
/// </summary>
public record ConsultarClienteRequest(int Id) : IRequest<Result<ConsultarClienteResponse>>;
