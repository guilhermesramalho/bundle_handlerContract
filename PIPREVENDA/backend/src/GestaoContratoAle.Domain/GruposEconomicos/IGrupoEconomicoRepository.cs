using PortalAle.Domain.Base;

namespace PortalAle.Domain.GruposEconomicos;

/// <summary>
/// Repositório de GrupoEconomico. Ver DT-018.
/// </summary>
public interface IGrupoEconomicoRepository : IRepository<GrupoEconomico>
{
    Task<bool> ExistePorCodigoAsync(string codigo, int? idParaIgnorar = null, CancellationToken cancellationToken = default);
}
