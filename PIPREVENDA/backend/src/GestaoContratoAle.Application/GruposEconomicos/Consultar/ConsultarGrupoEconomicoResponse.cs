namespace PortalAle.Application.GruposEconomicos.Consultar;

public record ConsultarGrupoEconomicoResponse(
    int Id,
    string Codigo,
    string Nome,
    DateTimeOffset DataCriacao,
    DateTimeOffset? DataAlteracao);
