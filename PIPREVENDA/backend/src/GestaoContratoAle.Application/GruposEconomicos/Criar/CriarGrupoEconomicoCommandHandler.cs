using PortalAle.Application.Base;
using PortalAle.Domain.Exceptions;
using PortalAle.Domain.GruposEconomicos;

namespace PortalAle.Application.GruposEconomicos.Criar;

public class CriarGrupoEconomicoCommandHandler(IGrupoEconomicoRepository grupoEconomicoRepository)
    : ICommandHandler<CriarGrupoEconomicoRequest, CriarGrupoEconomicoResponse>
{
    public async Task<Result<CriarGrupoEconomicoResponse>> ExecuteAsync(
        CriarGrupoEconomicoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await grupoEconomicoRepository.ExistePorCodigoAsync(request.Codigo, cancellationToken: cancellationToken))
            return Result<CriarGrupoEconomicoResponse>.Failure("Codigo: Já existe um grupo econômico cadastrado com este código");

        GrupoEconomico grupoEconomico;

        try
        {
            grupoEconomico = new GrupoEconomico(request.Codigo, request.Nome);
        }
        catch (DomainException ex)
        {
            return Result<CriarGrupoEconomicoResponse>.Failure(ex.Errors);
        }

        await grupoEconomicoRepository.AdicionarAsync(grupoEconomico, cancellationToken);

        return Result<CriarGrupoEconomicoResponse>.Success(new CriarGrupoEconomicoResponse(
            grupoEconomico.Id,
            grupoEconomico.Codigo,
            grupoEconomico.Nome,
            grupoEconomico.DataCriacao));
    }
}
