using PortalAle.Application.Base;

namespace PortalAle.Application.GruposEconomicos.Listar;

/// <summary>
/// Request de listagem paginada e ordenável de grupos econômicos, com filtro opcional por nome.
/// </summary>
public record ListarGruposEconomicosRequest
    : PaginationRequest, ISortableRequest, IRequest<Result<PaginationResponse<ListarGruposEconomicosResponse>>>
{
    public string? FiltroNome { get; init; }

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
