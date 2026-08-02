using PortalAle.Application.Base;
using PortalAle.Domain.Clientes;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Application.Clientes.Criar;

public class CriarClienteCommandHandler(IClienteRepository clienteRepository)
    : ICommandHandler<CriarClienteRequest, CriarClienteResponse>
{
    public async Task<Result<CriarClienteResponse>> ExecuteAsync(
        CriarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await clienteRepository.ExistePorCnpjAsync(request.Cnpj, cancellationToken: cancellationToken))
            return Result<CriarClienteResponse>.Failure("Cnpj: Já existe um cliente cadastrado com este CNPJ");

        Cliente cliente;

        try
        {
            cliente = new Cliente(request.Nome, request.Cnpj, request.Endereco);
        }
        catch (DomainException ex)
        {
            return Result<CriarClienteResponse>.Failure(ex.Errors);
        }

        await clienteRepository.AdicionarAsync(cliente, cancellationToken);

        return Result<CriarClienteResponse>.Success(new CriarClienteResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Cnpj,
            cliente.Endereco,
            cliente.Ativo,
            cliente.DataCriacao));
    }
}
