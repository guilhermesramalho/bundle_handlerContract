using PortalAle.Application.Base;

namespace PortalAle.Application.Clientes.Consultar;

public class ConsultarClienteQueryHandler(IClienteQueries clienteQueries)
    : IQueryHandler<ConsultarClienteRequest, ConsultarClienteResponse>
{
    public async Task<Result<ConsultarClienteResponse>> ExecuteAsync(
        ConsultarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        ConsultarClienteResponse? resultado = await clienteQueries.ConsultarClientePorIdAsync(request.Id, cancellationToken);

        if (resultado is null)
            return Result<ConsultarClienteResponse>.Failure($"Cliente com Id {request.Id} não foi encontrado");

        return Result<ConsultarClienteResponse>.Success(resultado);
    }
}
