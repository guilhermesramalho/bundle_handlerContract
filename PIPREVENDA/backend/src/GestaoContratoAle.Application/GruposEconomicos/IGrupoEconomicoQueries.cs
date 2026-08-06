using PortalAle.Application.Base;
using PortalAle.Application.GruposEconomicos.Consultar;
using PortalAle.Application.GruposEconomicos.Listar;

namespace PortalAle.Application.GruposEconomicos;

/// <summary>
/// Contrato de consultas (read-only) para GrupoEconomico. Implementação concreta na
/// camada de Infraestrutura. Ver DT-019.
/// </summary>
public interface IGrupoEconomicoQueries : IQuery
{
    Task<ConsultarGrupoEconomicoResponse?> ConsultarGrupoEconomicoPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginationResponse<ListarGruposEconomicosResponse>> ListarGruposEconomicosAsync(
        ListarGruposEconomicosRequest request,
        CancellationToken cancellationToken = default);
}
