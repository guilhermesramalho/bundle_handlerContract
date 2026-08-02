using PortalAle.Domain.Base;

namespace PortalAle.Domain.Clientes;

/// <summary>
/// Repositório de Cliente. Ver DT-018.
/// </summary>
public interface IClienteRepository : IRepository<Cliente>
{
    /// <summary>
    /// Verifica se já existe cliente com o CNPJ informado, opcionalmente ignorando
    /// um Id (uso em atualização, para não conflitar com o próprio registro).
    /// </summary>
    Task<bool> ExistePorCnpjAsync(string cnpj, int? idParaIgnorar = null, CancellationToken cancellationToken = default);
}
