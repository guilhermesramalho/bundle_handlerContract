using PortalAle.Application.Base;
using PortalAle.Domain.Base;
using PortalAle.Domain.Clientes;
using PortalAle.Domain.Contratos;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Application.Contratos.Criar;

public class CriarContratoCommandHandler(
    IContratoRepository contratoRepository,
    IRepository<Cliente> clienteRepository)
    : ICommandHandler<CriarContratoRequest, CriarContratoResponse>
{
    public async Task<Result<CriarContratoResponse>> ExecuteAsync(
        CriarContratoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await clienteRepository.ExisteAsync(request.ClienteId, cancellationToken))
            return Result<CriarContratoResponse>.Failure($"ClienteId: Cliente com Id {request.ClienteId} não foi encontrado");

        if (await contratoRepository.ExistePorPcrAsync(request.Pcr, cancellationToken: cancellationToken))
            return Result<CriarContratoResponse>.Failure("Pcr: Já existe um contrato cadastrado com este PCR/PCF");

        Contrato contrato;

        try
        {
            contrato = new Contrato(
                request.ClienteId,
                request.Pcr,
                request.Segmento,
                request.Tipo,
                request.SituacaoMes,
                request.Bandeira,
                request.RegistradoAle,
                request.InicioVigencia,
                request.FimVigencia,
                request.VolumeMensalContratado,
                request.MargemBase,
                request.Diretoria,
                request.RegionalVendas,
                request.PontoVenda,
                request.Consultor,
                request.Greenfield);
        }
        catch (DomainException ex)
        {
            return Result<CriarContratoResponse>.Failure(ex.Errors);
        }

        await contratoRepository.AdicionarAsync(contrato, cancellationToken);

        return Result<CriarContratoResponse>.Success(new CriarContratoResponse(
            contrato.Id,
            contrato.ClienteId,
            contrato.Pcr,
            contrato.Segmento,
            contrato.Tipo,
            contrato.InicioVigencia,
            contrato.FimVigencia,
            contrato.DataCriacao));
    }
}
