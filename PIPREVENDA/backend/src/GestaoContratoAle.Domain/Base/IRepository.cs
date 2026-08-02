namespace PortalAle.Domain.Base;

/// <summary>
/// Contrato base de repositório para Aggregate Roots.
/// Interfaces específicas (ex.: IContratoRepository) devem herdar desta interface
/// e adicionar apenas métodos específicos, sem duplicar os básicos.
/// Ver DT-018: Padrão de Implementação de Repositórios.
/// </summary>
public interface IRepository<T>
    where T : Entity
{
    Task<T?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<T>> ListarTodosAsync(CancellationToken cancellationToken = default);

    Task<int> AdicionarAsync(T entidade, CancellationToken cancellationToken = default);

    Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default);

    Task RemoverAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);
}
