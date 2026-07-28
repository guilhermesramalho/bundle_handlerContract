namespace PortalAle.Application.Base;

/// <summary>
/// Handler de leitura (Query). Injeta apenas contratos I{Entidade}Queries,
/// nunca DbContext. Ver DT-019.
/// </summary>
public interface IQueryHandler<in TRequest, TResponse>
    where TRequest : IRequest<Result<TResponse>>
{
    Task<Result<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
