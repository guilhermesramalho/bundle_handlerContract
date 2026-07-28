using System.Diagnostics;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortalAle.Api.Middleware;
using PortalAle.IoC;
using Serilog;
using Serilog.Formatting.Json;

const string ErrorTypeBaseUrl = "https://api.portalale.com.br/errors";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Logs estruturados com Serilog. Ver DT-007.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("Aplicacao", "PortalAle-Backend")
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.File(
            new JsonFormatter(),
            "logs/portalale-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    // Registro agregado de Application + Data.SqlServer via composition root. Ver PortalAle.IoC.
    builder.Services.AddPortalAleServices(builder.Configuration);

    builder.Services.AddOpenApi();

    // Esquema de autenticação (JWT/Azure AD, etc.) ainda não definido para o projeto
    // — AddAuthorization() habilita apenas UseAuthorization()/.RequireAuthorization()
    // nos endpoints; UseAuthentication() deve ser adicionado quando o provedor for escolhido.
    builder.Services.AddAuthorization();

    // Versionamento de API por segmento de URL (/api/v1/...). Endpoints devem
    // declarar .HasApiVersion(new ApiVersion(1.0)) ao serem criados.
    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    // ProblemDetails (RFC 7807) para todos os erros HTTP. Ver DT-013.
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions["traceId"] =
                Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            context.ProblemDetails.Instance = context.HttpContext.Request.Path;
            context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        };
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<CorrelationIdMiddleware>();

    // Middleware global de exceções não tratadas -> ProblemDetails. Ver DT-013.
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            context.RequestServices
                .GetRequiredService<ILogger<Program>>()
                .LogError(exception, "Erro não tratado capturado pelo middleware.");

            var problemDetails = exception switch
            {
                ArgumentException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Argumento Inválido",
                    Detail = app.Environment.IsDevelopment()
                        ? exception.Message
                        : "Um ou mais argumentos fornecidos são inválidos",
                    Type = $"{ErrorTypeBaseUrl}/validation-error",
                },
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Acesso Negado",
                    Detail = "Você não tem permissão para acessar este recurso",
                    Type = $"{ErrorTypeBaseUrl}/forbidden",
                },
                KeyNotFoundException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso Não Encontrado",
                    Detail = app.Environment.IsDevelopment()
                        ? exception.Message
                        : "O recurso solicitado não foi encontrado",
                    Type = $"{ErrorTypeBaseUrl}/not-found",
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro Interno do Servidor",
                    Detail = app.Environment.IsDevelopment()
                        ? exception?.Message
                        : "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
                    Type = $"{ErrorTypeBaseUrl}/internal-error",
                },
            };

            problemDetails.Instance = context.Request.Path;
            problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            if (app.Environment.IsDevelopment() && exception is not null)
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            }

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    });

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseHttpsRedirection();
    app.UseAuthorization();

    // Endpoints de negócio são registrados aqui conforme forem criados, ex.:
    // app.MapContratosEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente durante a inicialização.");
}
finally
{
    Log.CloseAndFlush();
}
