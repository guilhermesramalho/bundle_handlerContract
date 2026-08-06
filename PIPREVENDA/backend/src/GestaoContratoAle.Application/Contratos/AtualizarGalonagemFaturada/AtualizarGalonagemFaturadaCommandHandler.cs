using PortalAle.Application.Base;
using PortalAle.Domain.Contratos;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Application.Contratos.AtualizarGalonagemFaturada;

public class AtualizarGalonagemFaturadaCommandHandler(IContratoRepository contratoRepository)
    : ICommandHandler<AtualizarGalonagemFaturadaRequest, AtualizarGalonagemFaturadaResponse>
{
    public async Task<Result<AtualizarGalonagemFaturadaResponse>> ExecuteAsync(
        AtualizarGalonagemFaturadaRequest request,
        CancellationToken cancellationToken = default)
    {
        Contrato? contrato = await contratoRepository.ObterPorIdAsync(request.ContratoId, cancellationToken);

        if (contrato is null)
            return Result<AtualizarGalonagemFaturadaResponse>.Failure($"ContratoId: Contrato com Id {request.ContratoId} não foi encontrado");

        try
        {
            contrato.AtualizarGalonagemFaturada(request.NovaGalonagemFaturada);
        }
        catch (DomainException ex)
        {
            return Result<AtualizarGalonagemFaturadaResponse>.Failure(ex.Errors);
        }

        await contratoRepository.AtualizarAsync(contrato, cancellationToken);

        return Result<AtualizarGalonagemFaturadaResponse>.Success(new AtualizarGalonagemFaturadaResponse(
            contrato.Id,
            contrato.GalonagemFaturada));
    }
}
