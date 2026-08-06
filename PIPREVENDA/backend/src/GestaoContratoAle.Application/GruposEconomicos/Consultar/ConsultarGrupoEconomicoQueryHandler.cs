using PortalAle.Application.Base;

namespace PortalAle.Application.GruposEconomicos.Consultar;

public class ConsultarGrupoEconomicoQueryHandler(IGrupoEconomicoQueries grupoEconomicoQueries)
    : IQueryHandler<ConsultarGrupoEconomicoRequest, ConsultarGrupoEconomicoResponse>
{
    public async Task<Result<ConsultarGrupoEconomicoResponse>> ExecuteAsync(
        ConsultarGrupoEconomicoRequest request,
        CancellationToken cancellationToken = default)
    {
        ConsultarGrupoEconomicoResponse? resultado = await grupoEconomicoQueries.ConsultarGrupoEconomicoPorIdAsync(request.Id, cancellationToken);

        if (resultado is null)
            return Result<ConsultarGrupoEconomicoResponse>.Failure($"Grupo econômico com Id {request.Id} não foi encontrado");

        return Result<ConsultarGrupoEconomicoResponse>.Success(resultado);
    }
}
