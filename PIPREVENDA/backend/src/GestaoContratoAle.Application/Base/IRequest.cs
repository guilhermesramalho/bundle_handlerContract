namespace PortalAle.Application.Base;

/// <summary>
/// Contrato base para requests de casos de uso (Commands e Queries) que retornam
/// um resultado tipado. Ver DT-016.
/// </summary>
public interface IRequest<out TResult>
{
}

/// <summary>
/// Contrato base para requests de casos de uso sem payload de retorno.
/// </summary>
public interface IRequest : IRequest<Result>
{
}
