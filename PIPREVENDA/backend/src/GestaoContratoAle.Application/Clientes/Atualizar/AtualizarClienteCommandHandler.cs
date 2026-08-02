using PortalAle.Application.Base;
using PortalAle.Domain.Clientes;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Application.Clientes.Atualizar;

public class AtualizarClienteCommandHandler(IClienteRepository clienteRepository)
    : ICommandHandler<AtualizarClienteRequest, AtualizarClienteResponse>
{
    public async Task<Result<AtualizarClienteResponse>> ExecuteAsync(
        AtualizarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        Cliente? cliente = await clienteRepository.ObterPorIdAsync(request.Id, cancellationToken);

        if (cliente is null)
            return Result<AtualizarClienteResponse>.Failure($"Cliente com Id {request.Id} não foi encontrado");

        if (await clienteRepository.ExistePorCnpjAsync(request.Cnpj, request.Id, cancellationToken))
            return Result<AtualizarClienteResponse>.Failure("Cnpj: Já existe outro cliente cadastrado com este CNPJ");

        try
        {
            cliente.Atualizar(request.Nome, request.Cnpj, request.Endereco, request.Ativo);
        }
        catch (DomainException ex)
        {
            return Result<AtualizarClienteResponse>.Failure(ex.Errors);
        }

        await clienteRepository.AtualizarAsync(cliente, cancellationToken);

        return Result<AtualizarClienteResponse>.Success(new AtualizarClienteResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Cnpj,
            cliente.Endereco,
            cliente.Ativo,
            cliente.DataAlteracao));
    }
}
