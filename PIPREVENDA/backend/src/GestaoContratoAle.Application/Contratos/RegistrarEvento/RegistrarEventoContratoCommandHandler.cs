using PortalAle.Application.Base;
using PortalAle.Domain.Contratos;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Application.Contratos.RegistrarEvento;

public class RegistrarEventoContratoCommandHandler(IContratoRepository contratoRepository)
    : ICommandHandler<RegistrarEventoContratoRequest, RegistrarEventoContratoResponse>
{
    public async Task<Result<RegistrarEventoContratoResponse>> ExecuteAsync(
        RegistrarEventoContratoRequest request,
        CancellationToken cancellationToken = default)
    {
        Contrato? contrato = await contratoRepository.ObterComEventosAsync(request.ContratoId, cancellationToken);

        if (contrato is null)
            return Result<RegistrarEventoContratoResponse>.Failure($"ContratoId: Contrato com Id {request.ContratoId} não foi encontrado");

        try
        {
            contrato.RegistrarEvento(request.Tipo, request.Data, request.Descricao);
        }
        catch (DomainException ex)
        {
            return Result<RegistrarEventoContratoResponse>.Failure(ex.Errors);
        }

        await contratoRepository.AtualizarAsync(contrato, cancellationToken);

        return Result<RegistrarEventoContratoResponse>.Success(new RegistrarEventoContratoResponse(
            contrato.Id,
            request.Tipo,
            request.Data,
            request.Descricao,
            contrato.Encerrado,
            contrato.Denuncia));
    }
}
