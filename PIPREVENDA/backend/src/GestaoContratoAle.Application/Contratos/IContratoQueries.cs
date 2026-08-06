using PortalAle.Application.Base;
using PortalAle.Application.Contratos.Consultar;
using PortalAle.Application.Contratos.Listar;

namespace PortalAle.Application.Contratos;

/// <summary>
/// Contrato de consultas (read-only) para Contrato. Implementação concreta na
/// camada de Infraestrutura. Ver DT-019.
/// </summary>
public interface IContratoQueries : IQuery
{
    Task<ConsultarContratoResponse?> ConsultarContratoPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginationResponse<ListarContratosResponse>> ListarContratosAsync(
        ListarContratosRequest request,
        CancellationToken cancellationToken = default);
}
