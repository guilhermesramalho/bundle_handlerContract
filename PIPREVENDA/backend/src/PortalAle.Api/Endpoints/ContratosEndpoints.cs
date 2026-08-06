using Asp.Versioning;
using Asp.Versioning.Builder;
using PortalAle.Api.Extensions;
using PortalAle.Application.Base;
using PortalAle.Application.Contratos.Consultar;
using PortalAle.Application.Contratos.Criar;
using PortalAle.Application.Contratos.Listar;
using PortalAle.Application.Contratos.RegistrarEvento;
using PortalAle.Domain.Contratos;

namespace PortalAle.Api.Endpoints;

/// <summary>
/// Endpoints REST de Contrato. Ver DT-006 e PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 6.
/// Sem PUT/DELETE genéricos — contratos não são editados/excluídos por completo, mudam de
/// estado via eventos de negócio (POST /eventos).
/// </summary>
public static class ContratosEndpoints
{
    public static void MapContratosEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1.0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/contratos")
            .WithApiVersionSet(versionSet)
            .WithTags("Contratos");

        group.MapGet("/", ListarContratos)
            .WithName("ListarContratos")
            .WithSummary("Lista contratos de forma paginada, filtrável e ordenável")
            .WithDescription("Cobre os mesmos filtros da tela de Listagem de Contratos do frontend (busca, segmento, tipo, situação da PCR, denúncia, bandeira, UF, hierarquia comercial).")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<PaginationResponse<ListarContratosResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", ConsultarContrato)
            .WithName("ConsultarContrato")
            .WithSummary("Consulta contrato por Id")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<ConsultarContratoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CriarContrato)
            .WithName("CriarContrato")
            .WithSummary("Cria um novo contrato")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<CriarContratoResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/{id:int}/eventos", RegistrarEventoContrato)
            .WithName("RegistrarEventoContrato")
            .WithSummary("Registra um evento no ciclo de vida do contrato (Renovação, Readequação, Cessão, Sucessão, Denúncia ou Encerramento)")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<RegistrarEventoContratoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> ListarContratos(
        [AsParameters] ListarContratosRequest request,
        IQueryHandler<ListarContratosRequest, PaginationResponse<ListarContratosResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<PaginationResponse<ListarContratosResponse>> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> ConsultarContrato(
        int id,
        IQueryHandler<ConsultarContratoRequest, ConsultarContratoResponse> handler,
        CancellationToken cancellationToken)
    {
        ConsultarContratoRequest request = new(id);
        Result<ConsultarContratoResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails(StatusCodes.Status404NotFound, "not-found");
    }

    private static async Task<IResult> CriarContrato(
        CriarContratoRequest request,
        ICommandHandler<CriarContratoRequest, CriarContratoResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<CriarContratoResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/v1/contratos/{result.Value!.Id}", result.Value)
            : result.ToValidationProblem();
    }

    private static async Task<IResult> RegistrarEventoContrato(
        int id,
        RegistrarEventoContratoBody body,
        ICommandHandler<RegistrarEventoContratoRequest, RegistrarEventoContratoResponse> handler,
        CancellationToken cancellationToken)
    {
        RegistrarEventoContratoRequest request = new(id, body.Tipo, body.Data, body.Descricao);
        Result<RegistrarEventoContratoResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return result.Errors.Any(e => e.Contains("não foi encontrado"))
            ? result.ToProblemDetails(StatusCodes.Status404NotFound, "not-found")
            : result.ToValidationProblem();
    }
}

/// <summary>
/// Corpo da requisição de registro de evento — sem o ContratoId, que vem da rota.
/// </summary>
public record RegistrarEventoContratoBody(TipoEventoContrato Tipo, DateOnly Data, string Descricao);
