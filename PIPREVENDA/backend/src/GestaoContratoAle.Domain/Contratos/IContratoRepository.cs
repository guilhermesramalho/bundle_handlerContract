using PortalAle.Domain.Base;

namespace PortalAle.Domain.Contratos;

/// <summary>
/// Repositório de Contrato. Ver DT-018.
/// </summary>
public interface IContratoRepository : IRepository<Contrato>
{
    Task<bool> ExistePorPcrAsync(string pcr, int? idParaIgnorar = null, CancellationToken cancellationToken = default);

    /// <summary>Carrega o contrato com os eventos (Include) — necessário para RegistrarEvento não perder o histórico já carregado.</summary>
    Task<Contrato?> ObterComEventosAsync(int id, CancellationToken cancellationToken = default);
}
