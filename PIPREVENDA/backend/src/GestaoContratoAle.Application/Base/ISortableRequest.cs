namespace PortalAle.Application.Base;

/// <summary>
/// Contrato de ordenação dinâmica para requests de listagem. Ver DT-019.
/// </summary>
public interface ISortableRequest
{
    string? SortBy { get; }

    string? SortDirection { get; }
}

public static class SortableRequestExtensions
{
    /// <summary>
    /// Indica se a ordenação solicitada é descendente ("desc", case-insensitive).
    /// Implementado como método de extensão (e não membro default da interface)
    /// porque membros default de interface não ficam visíveis através do tipo
    /// concreto que a implementa — apenas via referência à própria interface.
    /// </summary>
    public static bool IsSortDescending(this ISortableRequest request)
        => string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
