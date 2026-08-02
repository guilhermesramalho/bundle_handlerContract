using Microsoft.EntityFrameworkCore;
using PortalAle.Domain.Base;

namespace PortalAle.Data;

/// <summary>
/// Implementação genérica de IRepository&lt;T&gt;, reutilizável por repositórios
/// específicos de Aggregate Root via herança (ex.: ContratoRepository : Repository&lt;Contrato&gt;).
/// Agnóstica de provider EF Core (SQL Server, etc.). Ver DT-018.
/// </summary>
public class Repository<T>(DbContext context) : IRepository<T>
    where T : Entity
{
    protected readonly DbContext Context = context;

    public virtual async Task<T?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Set<T>().FindAsync([id], cancellationToken);

    public virtual async Task<List<T>> ListarTodosAsync(CancellationToken cancellationToken = default)
        => await Context.Set<T>().ToListAsync(cancellationToken);

    public virtual async Task<int> AdicionarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entidade, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return entidade.Id;
    }

    public virtual async Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Update(entidade);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var entidade = await Context.Set<T>().FindAsync([id], cancellationToken);

        if (entidade is null)
            return;

        Context.Set<T>().Remove(entidade);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AnyAsync(entidade => entidade.Id == id, cancellationToken);
}
