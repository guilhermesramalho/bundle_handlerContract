using Asp.Versioning;
using Asp.Versioning.Builder;
using PortalAle.Api.Extensions;
using PortalAle.Application.Base;
using PortalAle.Application.Clientes.Atualizar;
using PortalAle.Application.Clientes.Consultar;
using PortalAle.Application.Clientes.Criar;
using PortalAle.Application.Clientes.Excluir;
using PortalAle.Application.Clientes.Listar;

namespace PortalAle.Api.Endpoints;

/// <summary>
/// Endpoints REST de Cliente. Ver DT-006.
/// </summary>
public static class ClientesEndpoints
{
    public static void MapClientesEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1.0))
            .ReportApiVersions()
            .Build();

        // .RequireAuthorization() será adicionado quando o esquema de autenticação
        // do projeto (JWT/Azure AD, etc.) for definido — ver comentário em Program.cs.
        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/clientes")
            .WithApiVersionSet(versionSet)
            .WithTags("Clientes");

        group.MapGet("/", ListarClientes)
            .WithName("ListarClientes")
            .WithSummary("Lista clientes de forma paginada")
            .WithDescription("Retorna uma página de clientes, com filtros opcionais por nome e CNPJ.")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<PaginationResponse<ListarClientesResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", ConsultarCliente)
            .WithName("ConsultarCliente")
            .WithSummary("Consulta cliente por Id")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<ConsultarClienteResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CriarCliente)
            .WithName("CriarCliente")
            .WithSummary("Cria um novo cliente")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<CriarClienteResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", AtualizarCliente)
            .WithName("AtualizarCliente")
            .WithSummary("Atualiza um cliente existente")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces<AtualizarClienteResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", ExcluirCliente)
            .WithName("ExcluirCliente")
            .WithSummary("Exclui (logicamente) um cliente")
            .WithOpenApi()
            .HasApiVersion(new ApiVersion(1.0))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ListarClientes(
        [AsParameters] ListarClientesRequest request,
        IQueryHandler<ListarClientesRequest, PaginationResponse<ListarClientesResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<PaginationResponse<ListarClientesResponse>> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> ConsultarCliente(
        int id,
        IQueryHandler<ConsultarClienteRequest, ConsultarClienteResponse> handler,
        CancellationToken cancellationToken)
    {
        ConsultarClienteRequest request = new(id);
        Result<ConsultarClienteResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails(StatusCodes.Status404NotFound, "not-found");
    }

    private static async Task<IResult> CriarCliente(
        CriarClienteRequest request,
        ICommandHandler<CriarClienteRequest, CriarClienteResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<CriarClienteResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/v1/clientes/{result.Value!.Id}", result.Value)
            : result.ToValidationProblem();
    }

    private static async Task<IResult> AtualizarCliente(
        int id,
        AtualizarClienteBody body,
        ICommandHandler<AtualizarClienteRequest, AtualizarClienteResponse> handler,
        CancellationToken cancellationToken)
    {
        AtualizarClienteRequest request = new(id, body.Nome, body.Cnpj, body.Endereco, body.Ativo);
        Result<AtualizarClienteResponse> result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToValidationProblem();
    }

    private static async Task<IResult> ExcluirCliente(
        int id,
        ICommandHandler<ExcluirClienteRequest> handler,
        CancellationToken cancellationToken)
    {
        ExcluirClienteRequest request = new(id);
        Result result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : result.ToProblemDetails(StatusCodes.Status404NotFound, "not-found");
    }
}

/// <summary>
/// Corpo da requisição de atualização de cliente — sem o Id, que vem da rota.
/// </summary>
public record AtualizarClienteBody(string Nome, string Cnpj, string Endereco, bool Ativo);
