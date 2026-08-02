using PortalAle.Application.Base;
using PortalAle.Application.Clientes.Consultar;
using PortalAle.Application.Clientes.Listar;

namespace PortalAle.Application.Clientes;

/// <summary>
/// Contrato de consultas (read-only) para Cliente. Implementação concreta na
/// camada de Infraestrutura. Ver DT-019.
/// </summary>
public interface IClienteQueries : IQuery
{
    Task<ConsultarClienteResponse?> ConsultarClientePorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginationResponse<ListarClientesResponse>> ListarClientesAsync(
        ListarClientesRequest request,
        CancellationToken cancellationToken = default);
}
