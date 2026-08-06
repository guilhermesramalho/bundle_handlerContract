namespace PortalAle.Application.GruposEconomicos.Criar;

/// <summary>
/// Response da operação de criação de grupo econômico.
/// </summary>
public record CriarGrupoEconomicoResponse(
    int Id,
    string Codigo,
    string Nome,
    DateTimeOffset DataCriacao);
