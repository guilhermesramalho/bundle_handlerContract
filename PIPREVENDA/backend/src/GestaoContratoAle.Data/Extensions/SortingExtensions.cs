using System.Linq.Expressions;

namespace PortalAle.Data.Extensions;

/// <summary>
/// Extensões de ordenação dinâmica para uso nas implementações de I{Entidade}Queries. Ver DT-019.
/// </summary>
public static class SortingExtensions
{
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool descending,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap)
    {
        if (string.IsNullOrWhiteSpace(sortBy) || !sortMap.TryGetValue(sortBy, out var sortExpression))
            return query;

        return descending
            ? query.OrderByDescending(sortExpression)
            : query.OrderBy(sortExpression);
    }
}
