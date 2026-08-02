namespace PortalAle.Application.Base;

/// <summary>
/// Handler de escrita (Command) com payload de retorno. Ver DT-020.
/// </summary>
public interface ICommandHandler<in TRequest, TResponse>
    where TRequest : IRequest<Result<TResponse>>
{
    Task<Result<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler de escrita (Command) sem payload de retorno.
/// </summary>
public interface ICommandHandler<in TRequest>
    where TRequest : IRequest<Result>
{
    Task<Result> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
