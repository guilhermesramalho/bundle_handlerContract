using PortalAle.Application.Base;

namespace PortalAle.Application.Clientes.Excluir;

/// <summary>
/// Request de escrita para excluir (logicamente) um cliente.
/// </summary>
public record ExcluirClienteRequest(int Id) : IRequest<Result>;
