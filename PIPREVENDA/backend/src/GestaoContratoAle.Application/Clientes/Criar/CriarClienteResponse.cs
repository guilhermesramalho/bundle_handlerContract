namespace PortalAle.Application.Clientes.Criar;

/// <summary>
/// Response da operação de criação de cliente.
/// </summary>
public record CriarClienteResponse(
    int Id,
    string Nome,
    string Cnpj,
    string Endereco,
    bool Ativo,
    DateTimeOffset DataCriacao);
