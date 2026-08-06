namespace PortalAle.Application.Base;

/// <summary>
/// Base para requests de listagem paginada. Ver DT-019.
/// </summary>
public abstract record PaginationRequest : IPaginationRequest
{
    private const int TamanhoPaginaPadrao = 20;
    private const int TamanhoPaginaMaximo = 100;

    public int? PageIndex { get; init; }

    public int? PageSize { get; init; }

    public int NormalizedPageIndex => PageIndex is null or < 1 ? 1 : PageIndex.Value;

    public int NormalizedPageSize => PageSize is null or < 1 or > TamanhoPaginaMaximo
        ? TamanhoPaginaPadrao
        : PageSize.Value;
}
