using Microsoft.EntityFrameworkCore;
using PortalAle.Application.Base;

namespace PortalAle.Data.Extensions;

/// <summary>
/// Extensões de paginação para uso nas implementações de I{Entidade}Queries. Ver DT-019.
/// </summary>
public static class PaginationExtensions
{
    public static async Task<PaginationResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<T>(items, pageIndex, pageSize, totalCount);
    }
}
