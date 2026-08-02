using PortalAle.Application.Base;

namespace PortalAle.Application.Clientes.Atualizar;

/// <summary>
/// Request de escrita para atualizar um cliente existente.
/// </summary>
public record AtualizarClienteRequest(
    int Id,
    string Nome,
    string Cnpj,
    string Endereco,
    bool Ativo) : IRequest<Result<AtualizarClienteResponse>>;
