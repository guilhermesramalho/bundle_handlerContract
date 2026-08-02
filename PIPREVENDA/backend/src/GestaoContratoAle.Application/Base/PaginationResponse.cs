namespace PortalAle.Application.Base;

/// <summary>
/// Resposta padronizada de listagem paginada. Ver DT-019.
/// </summary>
public record PaginationResponse<T>(
    IReadOnlyList<T> Items,
    int PageIndex,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
