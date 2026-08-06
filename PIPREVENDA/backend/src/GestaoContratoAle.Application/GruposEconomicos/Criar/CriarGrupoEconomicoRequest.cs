using PortalAle.Application.Base;

namespace PortalAle.Application.GruposEconomicos.Criar;

/// <summary>
/// Request de escrita para criar um novo grupo econômico.
/// </summary>
public record CriarGrupoEconomicoRequest(
    string Codigo,
    string Nome) : IRequest<Result<CriarGrupoEconomicoResponse>>;
