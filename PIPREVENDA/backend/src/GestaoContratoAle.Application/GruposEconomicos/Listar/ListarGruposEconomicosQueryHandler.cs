using PortalAle.Application.Base;

namespace PortalAle.Application.GruposEconomicos.Listar;

public class ListarGruposEconomicosQueryHandler(IGrupoEconomicoQueries grupoEconomicoQueries)
    : IQueryHandler<ListarGruposEconomicosRequest, PaginationResponse<ListarGruposEconomicosResponse>>
{
    public async Task<Result<PaginationResponse<ListarGruposEconomicosResponse>>> ExecuteAsync(
        ListarGruposEconomicosRequest request,
        CancellationToken cancellationToken = default)
    {
        PaginationResponse<ListarGruposEconomicosResponse> resultado = await grupoEconomicoQueries.ListarGruposEconomicosAsync(request, cancellationToken);

        return Result<PaginationResponse<ListarGruposEconomicosResponse>>.Success(resultado);
    }
}
