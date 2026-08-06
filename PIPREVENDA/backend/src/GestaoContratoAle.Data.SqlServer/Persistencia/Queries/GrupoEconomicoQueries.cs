using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PortalAle.Application.Base;
using PortalAle.Application.GruposEconomicos;
using PortalAle.Application.GruposEconomicos.Consultar;
using PortalAle.Application.GruposEconomicos.Listar;
using PortalAle.Data.Extensions;
using PortalAle.Domain.GruposEconomicos;

namespace PortalAle.Data.SqlServer.Persistencia.Queries;

/// <summary>
/// Implementação de leitura (read-only, otimizada) para GrupoEconomico. Ver DT-019.
/// </summary>
public class GrupoEconomicoQueries(ApplicationDbContext context) : IGrupoEconomicoQueries
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<GrupoEconomico, object>>> s_sortMap =
        new Dictionary<string, Expression<Func<GrupoEconomico, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["codigo"] = grupo => grupo.Codigo,
            ["nome"] = grupo => grupo.Nome,
        };

    public async Task<ConsultarGrupoEconomicoResponse?> ConsultarGrupoEconomicoPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await context.GruposEconomicos
            .AsNoTracking()
            .Where(grupo => grupo.Id == id)
            .Select(grupo => new ConsultarGrupoEconomicoResponse(
                grupo.Id,
                grupo.Codigo,
                grupo.Nome,
                grupo.DataCriacao,
                grupo.DataAlteracao))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginationResponse<ListarGruposEconomicosResponse>> ListarGruposEconomicosAsync(
        ListarGruposEconomicosRequest request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<GrupoEconomico> query = context.GruposEconomicos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.FiltroNome))
            query = query.Where(grupo => grupo.Nome.Contains(request.FiltroNome));

        return await query
            .ApplySorting(request.SortBy, request.IsSortDescending(), s_sortMap)
            .Select(grupo => new ListarGruposEconomicosResponse(
                grupo.Id,
                grupo.Codigo,
                grupo.Nome))
            .ToPagedResponseAsync(request.NormalizedPageIndex, request.NormalizedPageSize, cancellationToken);
    }
}
