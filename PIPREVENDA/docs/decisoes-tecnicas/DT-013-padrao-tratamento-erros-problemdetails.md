# DT-013: Padrão de Tratamento de Erros com ProblemDetails (RFC 7807)

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Adotar ProblemDetails (RFC 7807) como padrão universal para erros HTTP, incluindo traceId, validações e middleware global
>
> **Quando usar:** Ao retornar erros em endpoints REST, implementar middleware de tratamento de exceções ou padronizar respostas de erro
>
> **Palavras-chave:** `problemdetails` `rfc7807` `error-handling` `middleware` `validation` `traceid`

## Contexto

A API PortalAle necessita de um padrão consistente para:

1. **Retorno de Erros Estruturados**: Fornecer informações detalhadas sobre erros de forma padronizada
2. **Rastreabilidade**: Incluir identificadores únicos (traceId) para correlação de logs
3. **Frontend Friendly**: Erros em formato facilmente consumível pelo frontend React/Next.js
4. **Padrões Internacionais**: Seguir RFC 7807 (ProblemDetails for HTTP APIs)
5. **Erros de Validação**: Agrupar erros por campo para facilitar exibição em formulários
6. **Exceções Não Tratadas**: Middleware global para capturar e converter exceções em ProblemDetails

**Problema**: Sem padronização, erros são retornados de forma inconsistente:

- Alguns endpoints retornam objetos anônimos `{ Errors: [...] }`
- Outros retornam apenas mensagens de texto
- Exceções não tratadas expõem stack traces e detalhes técnicos
- Front end precisa implementar lógica diferente para cada tipo de erro

---

## Decisão

Adotamos **ProblemDetails (RFC 7807)** como padrão universal para todos os erros da API, com as seguintes especificações:

### 1. Estrutura ProblemDetails (RFC 7807)

**Campos Obrigatórios:**

```json
{
  "type": "https://api.projsub.petrobras.com.br/errors/[tipo-erro]",
  "title": "Título do Erro",
  "status": 400,
  "detail": "Descrição detalhada do erro",
  "instance": "/api/recurso/123"
}
```

**Campos Opcionais (Extensions):**

```json
{
  "type": "...",
  "title": "...",
  "status": 400,
  "detail": "...",
  "instance": "...",
  "errors": ["Erro 1", "Erro 2"],
  "traceId": "00-abc123-def456-00",
  "timestamp": "2026-02-16T10:30:00Z"
}
```

### 2. Tipos de Erro Padronizados

| Tipo                      | URL                                                                   | Status HTTP | Quando Usar                     |
| ------------------------- | --------------------------------------------------------------------- | ----------- | ------------------------------- |
| `validation-error`        | `https://api.projsub.petrobras.com.br/errors/validation-error`        | 400         | Erros de validação de entrada   |
| `business-rule-violation` | `https://api.projsub.petrobras.com.br/errors/business-rule-violation` | 400         | Violação de regras de negócio   |
| `not-found`               | `https://api.projsub.petrobras.com.br/errors/not-found`               | 404         | Recurso não encontrado          |
| `conflict`                | `https://api.projsub.petrobras.com.br/errors/conflict`                | 409         | Conflito de estado/concorrência |
| `forbidden`               | `https://api.projsub.petrobras.com.br/errors/forbidden`               | 403         | Sem permissão para acesso       |
| `internal-error`          | `https://api.projsub.petrobras.com.br/errors/internal-error`          | 500         | Erro interno do servidor        |

---

## Implementação

### 1. Captura de Erros na Camada de Aplicação

A camada de Aplicação deve retornar erros através da classe `Result<T>` com mensagens estruturadas:

**Formato de Mensagens de Erro:**

Para erros de validação por campo:

```csharp
"NomeDoCampo: Mensagem de erro"
```

Para erros genéricos:

```csharp
"Mensagem de erro sem campo específico"
```

**Exemplo - Command Handler:**

```csharp
// PortalAle.Application/Projetos/Commands/CriarProjetoCommandHandler.cs
public class CriarProjetoCommandHandler
    : ICommandHandler<CriarProjetoCommand, Result<CriarProjetoResult>>
{
    public async Task<Result<CriarProjetoResult>> ExecuteAsync(
        CriarProjetoCommand command,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Validações de campo (formato: "Campo: Mensagem")
        if (string.IsNullOrWhiteSpace(command.Nome))
            errors.Add("Nome: O campo Nome é obrigatório");

        if (command.Nome?.Length > 200)
            errors.Add("Nome: O campo Nome deve ter no máximo 200 caracteres");

        if (string.IsNullOrWhiteSpace(command.Descricao))
            errors.Add("Descricao: O campo Descrição é obrigatório");

        if (command.Descricao?.Length > 500)
            errors.Add("Descricao: O campo Descrição deve ter no máximo 500 caracteres");

        // Validações de regra de negócio (sem campo específico)
        if (errors.Any())
            return Result<CriarProjetoResult>.Failure(errors);

        var projetoExistente = await _repository.ObterPorNomeAsync(command.Nome);
        if (projetoExistente != null)
            return Result<CriarProjetoResult>.Failure(
                new[] { "Já existe um projeto com este nome" });

        // Processar comando...
        var projeto = new Projeto(command.Nome, command.Descricao);
        await _repository.AdicionarAsync(projeto);

        var resultado = new CriarProjetoResult(projeto.Id, projeto.Nome);
        return Result<CriarProjetoResult>.Success(resultado);
    }
}
```

**Exemplo - Query Handler:**

```csharp
// PortalAle.Application/Projetos/Queries/ConsultarProjetoQueryHandler.cs
public class ConsultarProjetoQueryHandler
    : IQueryHandler<ConsultarProjetoQuery, Result<ConsultarProjetoResult>>
{
    public async Task<Result<ConsultarProjetoResult>> ExecuteAsync(
        ConsultarProjetoQuery query,
        CancellationToken cancellationToken)
    {
        var projeto = await _repository.ObterPorIdAsync(query.ProjetoId);

        if (projeto == null)
            return Result<ConsultarProjetoResult>.Failure(
                new[] { $"Projeto com ID {query.ProjetoId} não foi encontrado" });

        var resultado = new ConsultarProjetoResult(projeto.Id, projeto.Nome, projeto.Descricao);
        return Result<ConsultarProjetoResult>.Success(resultado);
    }
}
```

### 2. Retorno de Erros com Results.Problem

**Helper Extensions para Converter Result em ProblemDetails:**

```csharp
// PortalAle.Api/Extensions/ResultExtensions.cs
namespace PortalAle.Api.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Converte Result genérico em ProblemDetails.
    /// Usado para erros sem estrutura por campo.
    /// </summary>
    public static IResult ToProblemDetails(
        this Result result,
        int statusCode = StatusCodes.Status400BadRequest,
        string? type = null)
    {
        var errorType = type ?? GetErrorType(statusCode);

        return Results.Problem(
            statusCode: statusCode,
            title: GetTitle(statusCode),
            detail: result.Errors?.FirstOrDefault() ?? "Ocorreu um erro ao processar a requisição",
            type: $"https://api.projsub.petrobras.com.br/errors/{errorType}",
            instance: null, // Será preenchido pelo middleware
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = result.Errors,
                ["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
    }

    private static string GetErrorType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "validation-error",
        StatusCodes.Status404NotFound => "not-found",
        StatusCodes.Status409Conflict => "conflict",
        StatusCodes.Status403Forbidden => "forbidden",
        _ => "error"
    };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Erro de Validação",
        StatusCodes.Status404NotFound => "Recurso Não Encontrado",
        StatusCodes.Status409Conflict => "Conflito de Estado",
        StatusCodes.Status403Forbidden => "Acesso Negado",
        _ => "Erro"
    };
}
```

**Uso nos Endpoints:**

```csharp
// Erro 404 - Recurso não encontrado
private static async Task<IResult> ConsultarProjeto(
    int id,
    [FromServices] IQueryHandler<ConsultarProjetoQuery, Result<ConsultarProjetoResult>> handler,
    CancellationToken cancellationToken)
{
    var query = new ConsultarProjetoQuery(id);
    var result = await handler.ExecuteAsync(query, cancellationToken);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : result.ToProblemDetails(StatusCodes.Status404NotFound);
}

// Erro 409 - Conflito de estado
private static async Task<IResult> ExcluirProjeto(
    int id,
    [FromServices] ICommandHandler<ExcluirProjetoCommand, Result> handler,
    CancellationToken cancellationToken)
{
    var command = new ExcluirProjetoCommand(id);
    var result = await handler.ExecuteAsync(command, cancellationToken);

    return result.IsSuccess
        ? Results.NoContent()
        : result.ToProblemDetails(StatusCodes.Status409Conflict, "conflict");
}
```

### 3. Retorno de Erros com Results.ValidationProblem

**Helper para Converter Result em ValidationProblem:**

Para erros de validação com múltiplos campos, use `ValidationProblem` que agrupa erros por campo:

```csharp
// PortalAle.Api/Extensions/ResultExtensions.cs
public static class ResultExtensions
{
    /// <summary>
    /// Converte Result em ValidationProblem.
    /// Parseia erros no formato "Campo: Mensagem" e agrupa por campo.
    /// </summary>
    public static IResult ToValidationProblem(this Result result)
    {
        var errorsByField = new Dictionary<string, string[]>();

        foreach (var error in result.Errors ?? Enumerable.Empty<string>())
        {
            // Parsear erros no formato "Nome: Campo obrigatório"
            var parts = error.Split(':', 2, StringSplitOptions.TrimEntries);

            if (parts.Length == 2)
            {
                // Erro específico de campo
                var field = parts[0];
                var message = parts[1];

                if (!errorsByField.ContainsKey(field))
                    errorsByField[field] = new[] { message };
                else
                    errorsByField[field] = errorsByField[field].Append(message).ToArray();
            }
            else
            {
                // Erro genérico (sem campo específico)
                if (!errorsByField.ContainsKey("_general"))
                    errorsByField["_general"] = new[] { error };
                else
                    errorsByField["_general"] = errorsByField["_general"].Append(error).ToArray();
            }
        }

        return Results.ValidationProblem(
            errorsByField,
            title: "Erro de Validação",
            detail: "Um ou mais campos estão inválidos",
            type: "https://api.projsub.petrobras.com.br/errors/validation-error",
            statusCode: StatusCodes.Status400BadRequest,
            instance: null, // Será preenchido pelo middleware
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = Activity.Current?.Id
            });
    }
}
```

**Uso nos Endpoints:**

```csharp
// POST - Criar Projeto com validação de múltiplos campos
private static async Task<IResult> CriarProjeto(
    [FromBody] CriarProjetoRequest request,
    [FromServices] ICommandHandler<CriarProjetoCommand, Result<CriarProjetoResult>> handler,
    CancellationToken cancellationToken)
{
    var command = new CriarProjetoCommand(request.Nome, request.Descricao);
    var result = await handler.ExecuteAsync(command, cancellationToken);

    if (!result.IsSuccess)
    {
        // Converter erros de validação para ValidationProblem
        return result.ToValidationProblem();
    }

    return Results.Created($"/api/projetos/{result.Value!.Id}", result.Value);
}
```

**Resposta JSON do ValidationProblem:**

```json
{
  "type": "https://api.projsub.petrobras.com.br/errors/validation-error",
  "title": "Erro de Validação",
  "status": 400,
  "detail": "Um ou mais campos estão inválidos",
  "errors": {
    "Nome": [
      "O campo Nome é obrigatório",
      "O campo Nome deve ter no máximo 200 caracteres"
    ],
    "Descricao": ["O campo Descrição deve ter no máximo 500 caracteres"],
    "_general": ["Já existe um projeto com este nome"]
  },
  "traceId": "00-abc123-def456-00"
}
```

### 4. Middleware de Tratamento Global de Exceções

Configure middleware global para capturar exceções não tratadas e convertê-las em ProblemDetails:

**Configuração no Program.cs:**

```csharp
// PortalAle.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configurar ProblemDetails globalmente
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Adicionar traceId em todas as respostas de erro
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        // Adicionar instance (caminho da requisição)
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;

        // Adicionar timestamp
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        // Informações adicionais apenas em ambiente de desenvolvimento
        if (builder.Environment.IsDevelopment())
        {
            context.ProblemDetails.Extensions["environment"] = "Development";
        }
    };
});

var app = builder.Build();

// Middleware de exception handling com ProblemDetails
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(
            exception,
            "Erro não tratado capturado pelo middleware: {Message}",
            exception?.Message);

        // Criar ProblemDetails baseado no tipo de exceção
        var problemDetails = exception switch
        {
            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Argumento Inválido",
                Detail = app.Environment.IsDevelopment()
                    ? exception.Message
                    : "Um ou mais argumentos fornecidos são inválidos",
                Type = "https://api.projsub.petrobras.com.br/errors/validation-error"
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Acesso Negado",
                Detail = "Você não tem permissão para acessar este recurso",
                Type = "https://api.projsub.petrobras.com.br/errors/forbidden"
            },
            KeyNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso Não Encontrado",
                Detail = app.Environment.IsDevelopment()
                    ? exception.Message
                    : "O recurso solicitado não foi encontrado",
                Type = "https://api.projsub.petrobras.com.br/errors/not-found"
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro Interno do Servidor",
                Detail = app.Environment.IsDevelopment()
                    ? exception?.Message
                    : "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
                Type = "https://api.projsub.petrobras.com.br/errors/internal-error"
            }
        };

        // Adicionar extensões
        problemDetails.Instance = context.Request.Path;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        // Em desenvolvimento, adicionar stack trace
        if (app.Environment.IsDevelopment() && exception != null)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Outros middlewares...
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Registrar endpoints
app.MapProjetoEndpoints();
app.MapRiscoEndpoints();

app.Run();
```

**Resposta de Exceção Não Tratada (Produção):**

```json
{
  "type": "https://api.projsub.petrobras.com.br/errors/internal-error",
  "title": "Erro Interno do Servidor",
  "status": 500,
  "detail": "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
  "instance": "/api/projetos/123",
  "traceId": "00-abc123-def456-00",
  "timestamp": "2026-02-16T10:30:00Z"
}
```

**Resposta de Exceção Não Tratada (Desenvolvimento):**

```json
{
  "type": "https://api.projsub.petrobras.com.br/errors/internal-error",
  "title": "Erro Interno do Servidor",
  "status": 500,
  "detail": "Object reference not set to an instance of an object.",
  "instance": "/api/projetos/123",
  "traceId": "00-abc123-def456-00",
  "timestamp": "2026-02-16T10:30:00Z",
  "environment": "Development",
  "stackTrace": "at PortalAle.Api.Endpoints.ProjetosEndpoints...",
  "exceptionType": "NullReferenceException"
}
```

---

## Exemplos Práticos

### Exemplo 1: Erro de Validação Simples (400)

**Request:**

```http
POST /api/projetos HTTP/1.1
Content-Type: application/json

{
  "nome": "",
  "descricao": ""
}
```

**Response:**

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://api.projsub.petrobras.com.br/errors/validation-error",
  "title": "Erro de Validação",
  "status": 400,
  "detail": "Um ou mais campos estão inválidos",
  "errors": {
    "Nome": ["O campo Nome é obrigatório"],
    "Descricao": ["O campo Descrição é obrigatório"]
  },
  "instance": "/api/projetos",
  "traceId": "00-4bf92f3577b34da6b3af9440bd2ac1ee-00"
}
```

### Exemplo 2: Recurso Não Encontrado (404)

**Request:**

```http
GET /api/projetos/999 HTTP/1.1
```

**Response:**

```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://api.projsub.petrobras.com.br/errors/not-found",
  "title": "Recurso Não Encontrado",
  "status": 404,
  "detail": "Projeto com ID 999 não foi encontrado",
  "errors": ["Projeto com ID 999 não foi encontrado"],
  "instance": "/api/projetos/999",
  "traceId": "00-5cf82f4588c45ea7c4bGT-0551ce3bd2ff-00"
}
```

### Exemplo 3: Conflito de Estado (409)

**Request:**

```http
DELETE /api/projetos/5 HTTP/1.1
```

**Response:**

```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "https://api.projsub.petrobras.com.br/errors/conflict",
  "title": "Conflito de Estado",
  "status": 409,
  "detail": "Não é possível excluir projeto com avançamentos registrados",
  "errors": ["Não é possível excluir projeto com avançamentos registrados"],
  "instance": "/api/projetos/5",
  "traceId": "00-6dg93g5699d56fb8d5ch1662df4ce3gg-00"
}
```

---

## Benefícios

### ✅ Positivos

1. **Padronização Internacional**: Seguir RFC 7807 garante compatibilidade com ferramentas e frameworks
2. **Rastreabilidade**: TraceId permite correlacionar erros entre API, logs e frontend
3. **Frontend Friendly**: Estrutura por campo facilita exibição em formulários Angular
4. **Consistência**: Todos os erros seguem o mesmo formato
5. **Debugging**: Stack traces em desenvolvimento facilitam identificação de problemas
6. **Extensibilidade**: Campo `extensions` permite adicionar metadados customizados
7. **Segurança**: Detalhes técnicos ocultados em produção

### ⚠️ Negativos

1. **Overhead**: Payload de erro ligeiramente maior (~100-200 bytes)
2. **Parsing**: Frontend precisa implementar parsing de ProblemDetails
3. **Complexidade**: Mais complexo que retornar apenas mensagem de texto

### 🔄 Mitigações

- Criar interceptors no Angular para parsing automático de ProblemDetails
- Documentar estrutura no Swagger/OpenAPI
- Fornecer exemplos claros de uso nos guias operacionais

---

## Métricas e Monitoração

### Monitorar via Logs

Todos os erros incluem `traceId` que permite:

- Correlacionar erro no frontend com log no backend
- Buscar logs específicos no CloudWatch/Kibana
- Rastrear fluxo completo da requisição

**Exemplo de Log:**

```
[2026-02-16 10:30:00] [ERROR] [TraceId: 00-abc123-def456-00]
Command execution failed: CriarProjetoCommand
Errors: Nome: O campo Nome é obrigatório
```

### Alertas e Dashboards

Criar alertas para:

- Taxa de erros 500 (exceções não tratadas)
- Taxa de erros 404 (possível problema de integração)
- Taxa de erros 400 acima de threshold (possível ataque)

---

## Referências

### Documentação Interna

- [DT-006: Padrão de Implementação de API REST](DT-006-padrao-implementacao-api-rest.md)

### Documentação Externa

- [RFC 7807 - Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)
- [ASP.NET Core - Problem Details](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [ASP.NET Core - ValidationProblem](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.validationproblem)
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)

---

**Histórico de Revisões**:

- **v1.0 (2026-02-16)**: Versão inicial - extração do DT-006 v2
