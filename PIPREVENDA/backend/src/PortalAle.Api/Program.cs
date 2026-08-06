using System.Diagnostics;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PortalAle.Api.Endpoints;
using PortalAle.Api.Extensions;
using PortalAle.Api.Middleware;
using PortalAle.Data.SqlServer;
using PortalAle.Data.SqlServer.Persistencia.Seed;
using PortalAle.IoC;
using Serilog;
using Serilog.Formatting.Json;

const string errorTypeBaseUrl = "https://api.portalale.com.br/errors";

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

    // Enums de domínio (Segmento, TipoContrato, etc.) trafegam como string no JSON
    // ("Rede", não "0") — mais legível na API pública e alinhado ao que os filtros de
    // query string já aceitam via [AsParameters] (enum.TryParse por nome).
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddOpenApi();

    // CORS para o frontend Next.js rodando em origem separada (localhost:3000) — sem
    // isso o navegador bloqueia o fetch com "Failed to fetch" mesmo com a API no ar e
    // saudável (curl/servidor-a-servidor não é afetado, só o browser aplica CORS,
    // descoberto testando a integração de verdade no Chrome). Origens configuráveis via
    // appsettings ("Cors:AllowedOrigins") para não hardcodar a porta do frontend aqui.
    const string corsPolicyFrontend = "FrontendPolicy";
    string[] corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000"];

    builder.Services.AddCors(options =>
        options.AddPolicy(corsPolicyFrontend, policy => policy
            .WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

    // Healthcheck de conectividade com o SQL Server (banco mk_gestaoContrato).
    string sqlServerConnectionString = builder.Configuration.GetConnectionString("PortalAle")
        ?? throw new InvalidOperationException("ConnectionString 'PortalAle' não configurada.");

    builder.Services
        .AddHealthChecks()
        .AddSqlServer(sqlServerConnectionString, name: "sqlserver", tags: ["ready"]);

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

    // Aplica automaticamente as migrations pendentes do EF Core ao subir a aplicação
    // (cria o banco/tabelas se ainda não existirem). Ver PortalAle.Data.SqlServer/Persistencia/Migrations.
    using (IServiceScope migrationScope = app.Services.CreateScope())
    {
        ApplicationDbContext dbContext = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

        // Seed de desenvolvimento (dataset mock do frontend) — nunca em produção,
        // idempotente (só insere se a tabela Contrato estiver vazia). Ver
        // PortalAle.Data.SqlServer/Persistencia/Seed/DesenvolvimentoSeeder.cs.
        if (app.Environment.IsDevelopment())
            await DesenvolvimentoSeeder.SeedAsync(dbContext);
    }

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
                    Type = $"{errorTypeBaseUrl}/validation-error",
                },
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Acesso Negado",
                    Detail = "Você não tem permissão para acessar este recurso",
                    Type = $"{errorTypeBaseUrl}/forbidden",
                },
                KeyNotFoundException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso Não Encontrado",
                    Detail = app.Environment.IsDevelopment()
                        ? exception.Message
                        : "O recurso solicitado não foi encontrado",
                    Type = $"{errorTypeBaseUrl}/not-found",
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro Interno do Servidor",
                    Detail = app.Environment.IsDevelopment()
                        ? exception?.Message
                        : "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
                    Type = $"{errorTypeBaseUrl}/internal-error",
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
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "PortalAle API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // Só força HTTPS fora de Development. Em dev a API roda sem certificado
    // configurado para o frontend confiar, e o launch profile "https" expõe
    // as duas portas (http+https) — com o redirect incondicional, todo GET
    // do frontend em localhost:5013 (perfil "http" do launchSettings) vira
    // um 307 para https://localhost:7030, que quebra o fetch (troca de
    // origem no meio do caminho) mesmo com CORS liberado. Descoberto
    // testando a integração de verdade, não em nenhum teste automatizado.
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseCors(corsPolicyFrontend);
    app.UseAuthorization();

    // Healthcheck de infraestrutura — não versionado, não segue o padrão CQRS
    // (endpoint técnico, não caso de uso de negócio).
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    });

    // Endpoints de negócio são registrados aqui conforme forem criados.
    app.MapClientesEndpoints();
    app.MapGruposEconomicosEndpoints();
    app.MapContratosEndpoints();

    app.Run();
}
catch (HostAbortedException)
{
    // Lançada intencionalmente pelo host das ferramentas de design-time do EF Core
    // (ex.: "dotnet ef migrations add") após montar o DI container — não é uma falha real.
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente durante a inicialização.");
}
finally
{
    Log.CloseAndFlush();
}
