namespace PortalAle.Application.Base;

/// <summary>
/// Base para requests de listagem paginada. Ver DT-019.
/// </summary>
public abstract record PaginationRequest : IPaginationRequest
{
    private const int TamanhoPaginaPadrao = 20;
    private const int TamanhoPaginaMaximo = 100;

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = TamanhoPaginaPadrao;

    public int NormalizedPageIndex => PageIndex < 1 ? 1 : PageIndex;

    public int NormalizedPageSize => PageSize is < 1 or > TamanhoPaginaMaximo
        ? TamanhoPaginaPadrao
        : PageSize;
}
