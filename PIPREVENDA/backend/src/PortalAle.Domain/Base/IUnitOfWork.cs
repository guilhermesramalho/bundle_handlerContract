namespace PortalAle.Domain.Base;

/// <summary>
/// Contrato agnóstico de infraestrutura para controle transacional.
/// Ver DT-021: Controle Transacional Automático para CommandHandlers.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
