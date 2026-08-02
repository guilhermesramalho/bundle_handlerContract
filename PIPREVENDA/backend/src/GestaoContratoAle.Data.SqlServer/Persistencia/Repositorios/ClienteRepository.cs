using Microsoft.EntityFrameworkCore;
using PortalAle.Data;
using PortalAle.Domain.Clientes;

namespace PortalAle.Data.SqlServer.Persistencia.Repositorios;

/// <summary>
/// Repositório de Cliente. Ver DT-018.
/// </summary>
public class ClienteRepository(ApplicationDbContext context) : Repository<Cliente>(context), IClienteRepository
{
    public async Task<bool> ExistePorCnpjAsync(
        string cnpj,
        int? idParaIgnorar = null,
        CancellationToken cancellationToken = default)
    {
        return await context.Clientes
            .Where(cliente => cliente.Cnpj == cnpj)
            .Where(cliente => idParaIgnorar == null || cliente.Id != idParaIgnorar)
            .AnyAsync(cancellationToken);
    }
}
