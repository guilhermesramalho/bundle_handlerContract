namespace PortalAle.Application.Clientes.Atualizar;

/// <summary>
/// Response da operação de atualização de cliente.
/// </summary>
public record AtualizarClienteResponse(
    int Id,
    string Nome,
    string Cnpj,
    string Endereco,
    bool Ativo,
    DateTimeOffset? DataAlteracao);
