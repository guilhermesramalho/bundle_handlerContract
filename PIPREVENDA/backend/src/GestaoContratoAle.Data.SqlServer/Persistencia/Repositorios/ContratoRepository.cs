using Microsoft.EntityFrameworkCore;
using PortalAle.Data;
using PortalAle.Domain.Contratos;

namespace PortalAle.Data.SqlServer.Persistencia.Repositorios;

/// <summary>
/// Repositório de Contrato. Ver DT-018.
/// </summary>
public class ContratoRepository(ApplicationDbContext context) : Repository<Contrato>(context), IContratoRepository
{
    public async Task<bool> ExistePorPcrAsync(
        string pcr,
        int? idParaIgnorar = null,
        CancellationToken cancellationToken = default)
    {
        return await context.Contratos
            .Where(contrato => contrato.Pcr == pcr)
            .Where(contrato => idParaIgnorar == null || contrato.Id != idParaIgnorar)
            .AnyAsync(cancellationToken);
    }

    public async Task<Contrato?> ObterComEventosAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Contratos
            .Include(contrato => contrato.Eventos)
            .FirstOrDefaultAsync(contrato => contrato.Id == id, cancellationToken);
    }
}
