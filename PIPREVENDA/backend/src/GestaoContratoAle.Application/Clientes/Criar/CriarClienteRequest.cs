using PortalAle.Application.Base;

namespace PortalAle.Application.Clientes.Criar;

/// <summary>
/// Request de escrita para criar um novo cliente.
/// </summary>
public record CriarClienteRequest(
    string Nome,
    string Cnpj,
    string Endereco) : IRequest<Result<CriarClienteResponse>>;
