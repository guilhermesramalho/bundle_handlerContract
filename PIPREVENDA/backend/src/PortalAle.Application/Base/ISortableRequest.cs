namespace PortalAle.Application.Base;

/// <summary>
/// Contrato de ordenação dinâmica para requests de listagem. Ver DT-019.
/// </summary>
public interface ISortableRequest
{
    string? SortBy { get; }

    string? SortDirection { get; }

    bool IsSortDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
