using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PortalAle.Domain.Base;

namespace PortalAle.Data;

/// <summary>
/// Implementação de IUnitOfWork usando EF Core. Agnóstica de provider — a escolha
/// do provider (SQL Server) ocorre no registro do DbContext concreto. Ver DT-021.
/// </summary>
public class UnitOfWork(DbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public bool HasActiveTransaction => _transaction is not null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            return;

        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        logger.LogDebug("Transação iniciada.");
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var alteracoes = await context.SaveChangesAsync(cancellationToken);

            if (_transaction is not null)
                await _transaction.CommitAsync(cancellationToken);

            logger.LogDebug("Transação confirmada (commit). {Alteracoes} alteração(ões) persistida(s).", alteracoes);

            return alteracoes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao confirmar transação. Revertendo (rollback).");
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            logger.LogWarning("Transação revertida (rollback).");
        }

        await DisposeTransactionAsync();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is null)
            return;

        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
