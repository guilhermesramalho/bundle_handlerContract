using PortalAle.Application.Base;
using PortalAle.Domain.Clientes;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Application.Clientes.Excluir;

public class ExcluirClienteCommandHandler(IClienteRepository clienteRepository)
    : ICommandHandler<ExcluirClienteRequest>
{
    public async Task<Result> ExecuteAsync(ExcluirClienteRequest request, CancellationToken cancellationToken = default)
    {
        Cliente? cliente = await clienteRepository.ObterPorIdAsync(request.Id, cancellationToken);

        if (cliente is null)
            return Result.Failure($"Cliente com Id {request.Id} não foi encontrado");

        try
        {
            cliente.ExcluirLogicamente();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Errors);
        }

        await clienteRepository.AtualizarAsync(cliente, cancellationToken);

        return Result.Success();
    }
}
