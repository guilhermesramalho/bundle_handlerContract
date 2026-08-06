using PortalAle.Application.Base;

namespace PortalAle.Application.GruposEconomicos.Consultar;

public record ConsultarGrupoEconomicoRequest(int Id) : IRequest<Result<ConsultarGrupoEconomicoResponse>>;
