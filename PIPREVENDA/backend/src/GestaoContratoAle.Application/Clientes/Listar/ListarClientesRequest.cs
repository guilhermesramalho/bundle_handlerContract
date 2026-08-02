using PortalAle.Application.Base;

namespace PortalAle.Application.Clientes.Listar;

/// <summary>
/// Request de listagem paginada e ordenável de clientes, com filtros opcionais.
/// </summary>
public record ListarClientesRequest
    : PaginationRequest, ISortableRequest, IRequest<Result<PaginationResponse<ListarClientesResponse>>>
{
    public string? FiltroNome { get; init; }

    public string? FiltroCnpj { get; init; }

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
