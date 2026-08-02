namespace PortalAle.Application.Clientes.Consultar;

/// <summary>
/// Response da consulta de um cliente por Id.
/// </summary>
public record ConsultarClienteResponse(
    int Id,
    string Nome,
    string Cnpj,
    string Endereco,
    bool Ativo,
    DateTimeOffset DataCriacao,
    DateTimeOffset? DataAlteracao);
