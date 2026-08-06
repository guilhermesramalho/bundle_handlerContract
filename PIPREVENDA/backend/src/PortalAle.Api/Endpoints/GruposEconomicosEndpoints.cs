using Asp.Versioning;
using Asp.Versioning.Builder;
using PortalAle.Api.Extensions;
using PortalAle.Application.Base;
using PortalAle.Application.GruposEconomicos.Consultar;
using PortalAle.Application.GruposEconomicos.Criar;
using PortalAle.Application.GruposEconomicos.Listar;

namespace PortalAle.Api.Endpoints;

/// <summary>
/// Endpoints REST de GrupoEconomico. Ver DT-006. Cadastro manual — placeholder
/// consciente até a integração SAP (Fase 5), ver PLANO-IMPLEMENTACAO-API-CONTRATO.md
/// seção 1. Só Criar/Consultar/Listar — sem Atualizar/Excluir por ora.
/// </summary>
public static class GruposEconomicosEndpoints
{
    public static void MapGruposEconomicosEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1.0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/grupos-economicos")
            .WithApiVersionSet(versionSet)
            .WithTags("GruposEconomicos");

        group.MapGet("/", ListarGruposEconomicos)
            .WithName("ListarGruposEconomicos")
            .WithSummary("Lista grupos econômicos de forma paginada")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<PaginationResponse<ListarGruposEconomicosResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", ConsultarGrupoEconomico)
            .WithName("ConsultarGrupoEconomico")
            .WithSummary("Consulta grupo econômico por Id")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<ConsultarGrupoEconomicoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CriarGrupoEconomico)
            .WithName("CriarGrupoEconomico")
            .WithSummary("Cria um novo grupo econômico")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<CriarGrupoEconomicoResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> ListarGruposEconomicos(
        [AsParameters] ListarGruposEconomicosRequest request,
        IQueryHandler<ListarGruposEconomicosRequest, PaginationResponse<ListarGruposEconomicosResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<PaginationResponse<ListarGruposEconomicosResponse>> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> ConsultarGrupoEconomico(
        int id,
        IQueryHandler<ConsultarGrupoEconomicoRequest, ConsultarGrupoEconomicoResponse> handler,
        CancellationToken cancellationToken)
    {
        ConsultarGrupoEconomicoRequest request = new(id);
        Result<ConsultarGrupoEconomicoResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails(StatusCodes.Status404NotFound, "not-found");
    }

    private static async Task<IResult> CriarGrupoEconomico(
        CriarGrupoEconomicoRequest request,
        ICommandHandler<CriarGrupoEconomicoRequest, CriarGrupoEconomicoResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<CriarGrupoEconomicoResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/v1/grupos-economicos/{result.Value!.Id}", result.Value)
            : result.ToValidationProblem();
    }
}
