namespace PortalAle.Application.Clientes.Listar;

/// <summary>
/// Item de resposta da listagem paginada de clientes.
/// </summary>
public record ListarClientesResponse(
    int Id,
    string Nome,
    string Cnpj,
    string Endereco,
    bool Ativo,
    DateTimeOffset DataCriacao);
