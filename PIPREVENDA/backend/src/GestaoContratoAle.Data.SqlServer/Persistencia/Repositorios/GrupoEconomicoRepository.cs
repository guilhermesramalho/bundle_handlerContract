using Microsoft.EntityFrameworkCore;
using PortalAle.Data;
using PortalAle.Domain.GruposEconomicos;

namespace PortalAle.Data.SqlServer.Persistencia.Repositorios;

/// <summary>
/// Repositório de GrupoEconomico. Ver DT-018.
/// </summary>
public class GrupoEconomicoRepository(ApplicationDbContext context) : Repository<GrupoEconomico>(context), IGrupoEconomicoRepository
{
    public async Task<bool> ExistePorCodigoAsync(
        string codigo,
        int? idParaIgnorar = null,
        CancellationToken cancellationToken = default)
    {
        return await context.GruposEconomicos
            .Where(grupo => grupo.Codigo == codigo)
            .Where(grupo => idParaIgnorar == null || grupo.Id != idParaIgnorar)
            .AnyAsync(cancellationToken);
    }
}
