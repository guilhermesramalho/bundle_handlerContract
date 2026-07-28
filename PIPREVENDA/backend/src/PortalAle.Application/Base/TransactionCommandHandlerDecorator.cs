using PortalAle.Domain.Base;

namespace PortalAle.Application.Base;

/// <summary>
/// Decorator que intercepta CommandHandlers automaticamente e gerencia transações
/// (Begin/Commit/Rollback) sem necessidade de código explícito nos handlers.
/// Ver DT-021: Controle Transacional Automático para CommandHandlers.
/// </summary>
public class TransactionCommandHandlerDecorator<TRequest, TResponse>(
    ICommandHandler<TRequest, TResponse> innerHandler,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TRequest, TResponse>
    where TRequest : IRequest<Result<TResponse>>
{
    public async Task<Result<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        // Suporta composição: se já existe transação ativa (handler chamado a partir de outro handler), não inicia nova.
        var iniciouTransacao = !unitOfWork.HasActiveTransaction;

        if (iniciouTransacao)
            await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await innerHandler.ExecuteAsync(request, cancellationToken);

            if (iniciouTransacao)
            {
                if (result.IsSuccess)
                    await unitOfWork.CommitAsync(cancellationToken);
                else
                    await unitOfWork.RollbackAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            if (iniciouTransacao)
                await unitOfWork.RollbackAsync(cancellationToken);

            throw;
        }
    }
}

/// <summary>
/// Variante do decorator transacional para CommandHandlers sem payload de retorno.
/// </summary>
public class TransactionCommandHandlerDecorator<TRequest>(
    ICommandHandler<TRequest> innerHandler,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TRequest>
    where TRequest : IRequest<Result>
{
    public async Task<Result> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var iniciouTransacao = !unitOfWork.HasActiveTransaction;

        if (iniciouTransacao)
            await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await innerHandler.ExecuteAsync(request, cancellationToken);

            if (iniciouTransacao)
            {
                if (result.IsSuccess)
                    await unitOfWork.CommitAsync(cancellationToken);
                else
                    await unitOfWork.RollbackAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            if (iniciouTransacao)
                await unitOfWork.RollbackAsync(cancellationToken);

            throw;
        }
    }
}
