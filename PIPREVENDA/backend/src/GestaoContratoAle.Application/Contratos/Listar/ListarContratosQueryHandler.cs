using PortalAle.Application.Base;

namespace PortalAle.Application.Contratos.Listar;

public class ListarContratosQueryHandler(IContratoQueries contratoQueries)
    : IQueryHandler<ListarContratosRequest, PaginationResponse<ListarContratosResponse>>
{
    public async Task<Result<PaginationResponse<ListarContratosResponse>>> ExecuteAsync(
        ListarContratosRequest request,
        CancellationToken cancellationToken = default)
    {
        PaginationResponse<ListarContratosResponse> resultado = await contratoQueries.ListarContratosAsync(request, cancellationToken);

        return Result<PaginationResponse<ListarContratosResponse>>.Success(resultado);
    }
}
