using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PortalAle.Application.Base;
using PortalAle.Application.Clientes;
using PortalAle.Application.Clientes.Consultar;
using PortalAle.Application.Clientes.Listar;
using PortalAle.Data.Extensions;
using PortalAle.Domain.Clientes;

namespace PortalAle.Data.SqlServer.Persistencia.Queries;

/// <summary>
/// Implementação de leitura (read-only, otimizada) para Cliente. Ver DT-019.
/// </summary>
public class ClienteQueries(ApplicationDbContext context) : IClienteQueries
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Cliente, object>>> s_sortMap =
        new Dictionary<string, Expression<Func<Cliente, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["nome"] = cliente => cliente.Nome,
            ["cnpj"] = cliente => cliente.Cnpj,
            ["dataCriacao"] = cliente => cliente.DataCriacao,
        };

    public async Task<ConsultarClienteResponse?> ConsultarClientePorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await context.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Id == id)
            .Select(cliente => new ConsultarClienteResponse(
                cliente.Id,
                cliente.Nome,
                cliente.Cnpj,
                cliente.Endereco,
                cliente.Ativo,
                cliente.DataCriacao,
                cliente.DataAlteracao))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginationResponse<ListarClientesResponse>> ListarClientesAsync(
        ListarClientesRequest request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Cliente> query = context.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.FiltroNome))
            query = query.Where(cliente => cliente.Nome.Contains(request.FiltroNome));

        if (!string.IsNullOrWhiteSpace(request.FiltroCnpj))
            query = query.Where(cliente => cliente.Cnpj.Contains(request.FiltroCnpj));

        return await query
            .ApplySorting(request.SortBy, request.IsSortDescending(), s_sortMap)
            .Select(cliente => new ListarClientesResponse(
                cliente.Id,
                cliente.Nome,
                cliente.Cnpj,
                cliente.Endereco,
                cliente.Ativo,
                cliente.DataCriacao))
            .ToPagedResponseAsync(request.NormalizedPageIndex, request.NormalizedPageSize, cancellationToken);
    }
}
