using PortalAle.Application.Base;

namespace PortalAle.Application.Contratos.Consultar;

public class ConsultarContratoQueryHandler(IContratoQueries contratoQueries)
    : IQueryHandler<ConsultarContratoRequest, ConsultarContratoResponse>
{
    public async Task<Result<ConsultarContratoResponse>> ExecuteAsync(
        ConsultarContratoRequest request,
        CancellationToken cancellationToken = default)
    {
        ConsultarContratoResponse? resultado = await contratoQueries.ConsultarContratoPorIdAsync(request.Id, cancellationToken);

        if (resultado is null)
            return Result<ConsultarContratoResponse>.Failure($"Contrato com Id {request.Id} não foi encontrado");

        return Result<ConsultarContratoResponse>.Success(resultado);
    }
}
