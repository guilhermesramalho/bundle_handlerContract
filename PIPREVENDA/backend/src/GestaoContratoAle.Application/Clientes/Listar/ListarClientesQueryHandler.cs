using PortalAle.Application.Base;

namespace PortalAle.Application.Clientes.Listar;

public class ListarClientesQueryHandler(IClienteQueries clienteQueries)
    : IQueryHandler<ListarClientesRequest, PaginationResponse<ListarClientesResponse>>
{
    public async Task<Result<PaginationResponse<ListarClientesResponse>>> ExecuteAsync(
        ListarClientesRequest request,
        CancellationToken cancellationToken = default)
    {
        PaginationResponse<ListarClientesResponse> resultado = await clienteQueries.ListarClientesAsync(request, cancellationToken);

        return Result<PaginationResponse<ListarClientesResponse>>.Success(resultado);
    }
}
