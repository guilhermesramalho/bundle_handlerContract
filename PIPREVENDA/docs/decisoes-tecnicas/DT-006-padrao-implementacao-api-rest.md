# DT-006: Padrão de Implementação de API REST com Minimal APIs e CQRS

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Padronizar implementação de endpoints REST usando Minimal APIs e CQRS.
>
> **Quando usar:** Ao criar novos endpoints REST, refatorar controllers para Minimal APIs ou implementar padrão CQRS na camada de apresentação
>
> **Palavras-chave:** `minimal-apis` `rest-api` `cqrs` `dotnet9` `endpoints` `clean-architecture`

## Contexto

A API PortalAle necessita de padronização na implementação de endpoints REST para garantir:

1. **Conformidade com Clean Architecture**: Endpoints devem atuar apenas como camada de apresentação, delegando processamento para a camada de Aplicação
2. **Separação de Responsabilidades**: Configuração de rotas separada da lógica de processamento
3. **Testabilidade**: Código facilmente testável sem dependências da infraestrutura HTTP
4. **CQRS**: Separação clara entre operações de leitura (Queries) e escrita (Commands)
5. **Performance**: Uso de Minimal APIs do .NET 9 para otimização de recursos
6. **Consistência**: Convenções claras de nomenclatura, rotas e códigos HTTP

---

## Decisão

Adotamos **Minimal APIs do .NET 9** com as seguintes diretrizes arquiteturais:

### 1. Estrutura de Endpoints

**Organização por Contexto de Negócio:**

```
PortalAle.Api/
  └── Endpoints/
      ├── ProjetosEndpoints.cs      # Endpoints de Projetos
      ├── RiscosEndpoints.cs        # Endpoints de Riscos
      ├── TarefasEndpoints.cs       # Endpoints de Tarefas
      └── [OutrosContextos]Endpoints.cs
```

**Estrutura Interna da Classe:**

```csharp
namespace PortalAle.Api.Endpoints;

public static class ProjetosEndpoints
{
    // ✅ Método público: APENAS configuração de rotas
    public static void MapProjetoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projetos")
            .WithTags("Projetos");

        group.MapGet("/", ListarProjetos)
            .WithName("ListarProjetos")
            .WithSummary("Lista todos os projetos")
            .Produces<ListarProjetosResponse>(StatusCodes.Status200OK);

        // Outras rotas...
    }

    // ✅ Métodos privados: implementação da lógica
    private static async Task<IResult> ListarProjetos(
        [FromServices] IQueryHandler<ListarProjetosRequest, ListarProjetosResponse> handler,
        CancellationToken cancellationToken)
    {
        var request = new ListarProjetosRequest();
        var result = await handler.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }
}
```

### 2. Padrão CQRS para Handlers

**Queries (Operações de Leitura - GET):**

```csharp
// ✅ Usar IQueryHandler para operações GET
private static async Task<IResult> ConsultarProjeto(
    int id,
    [FromServices] IQueryHandler<ConsultarProjetoRequest, ConsultarProjetoResponse> handler,
    CancellationToken cancellationToken)
{
    var request = new ConsultarProjetoRequest(id);
    var result = await handler.ExecuteAsync(request, cancellationToken);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : result.ToProblemDetails(StatusCodes.Status404NotFound);
}
```

**Commands (Operações de Escrita - POST/PUT/DELETE):**

```csharp
// ✅ Usar ICommandHandler para operações POST/PUT/DELETE
private static async Task<IResult> CriarProjeto(
    [FromBody] CriarProjetoRequest request,
    [FromServices] ICommandHandler<CriarProjetoRequest, CriarProjetoResponse> handler,
    CancellationToken cancellationToken)
{
    var result = await handler.ExecuteAsync(request, cancellationToken);

    return result.IsSuccess
        ? Results.Created($"/api/projetos/{result.Value!.Id}", result.Value)
        : result.ToProblemDetails();
}
```

### 3. Retorno de Respostas com IResult e ProblemDetails (RFC 7807)

**Respostas de Sucesso:**

```csharp
// 200 OK - Operação de leitura bem-sucedida
return Results.Ok(result.Value);

// 201 Created - Recurso criado com sucesso
return Results.Created($"/api/projetos/{id}", result.Value);

// 204 No Content - Operação bem-sucedida sem retorno
return Results.NoContent();
```

**Respostas de Erro com ProblemDetails (RFC 7807):**

Para tratamento de erros, consultarprojeto **[DT-013: Padrão de Tratamento de Erros com ProblemDetails (RFC 7807)](DT-013-padrao-tratamento-erros-problemdetails.md)**, que define:

- Estrutura obrigatória ProblemDetails para todos os erros
- Uso de `Results.Problem()` para erros simples (400, 404, 409)
- Uso de `Results.ValidationProblem()` para erros de validação agrupados por campo
- Helpers para converter `Result<T>` em ProblemDetails
- Middleware global para capturar exceções não tratadas
- Formato de mensagens de erro na camada de Aplicação ("Campo: Mensagem")

**Exemplo sucinto:**

```csharp
// Se handler retorna Result<T>, converter erro para ProblemDetails
if (!result.IsSuccess)
    return result.ToValidationProblem(); // Erros por campo
    // OU
    return result.ToProblemDetails(StatusCodes.Status404NotFound); // Erro simples
```

> 📘 Para detalhes completos sobre tratamento de erros, consulte [DT-013-padrao-tratamento-erros-problemdetails.md](DT-013-padrao-tratamento-erros-problemdetails.md)

### 4. Records para Request/Response

**Quando Criar Records:**

✅ **Criar Records quando:**

- Parâmetros do endpoint diferem dos DTOs de Request/Response da camada de Aplicação
- Há necessidade de transformação ou validação adicional na camada de apresentação
- API pública requer contrato diferente da camada de aplicação

✅ **Reutilizar DTOs quando:**

- Parâmetros do endpoint são idênticos aos DTOs da camada de Aplicação
- Não há necessidade de transformação

**Exemplo de Records:**

```csharp
// Records específicos do endpoint
public record CriarProjetoRequest(string Nome, string Descricao);
public record AtualizarProjetoRequest(string Nome, string Descricao);

// Para filtros complexos com múltiplos parâmetros
public record ListarProjetosFiltrosRequest(
    string? Nome = null,
    string? Descricao = null,
    string? Status = null);
```

### 5. Convenções de Nomenclatura

**Rotas RESTful:**

| Operação         | Método HTTP | Rota                          | Nome do Endpoint   |
| ---------------- | ----------- | ----------------------------- | ------------------ |
| Listar todos     | GET         | `/api/projetos`               | `ListarProjetos`   |
| Consultar por ID | GET         | `/api/projetos/{id}`          | `ConsultarProjeto` |
| Criar            | POST        | `/api/projetos`               | `CriarProjeto`     |
| Atualizar        | PUT         | `/api/projetos/{id}`          | `AtualizarProjeto` |
| Excluir          | DELETE      | `/api/projetos/{id}`          | `ExcluirProjeto`   |
| Ação customizada | POST        | `/api/projetos/{id}/importar` | `ImportarProjeto`  |

**Nomenclatura de Métodos:**

- Usar verbos em português no infinitivo (Criar, Consultar, Listar, Atualizar, Excluir)
- Usar substantivo do recurso no singular (Projeto, Risco, Tarefa)
- Para coleções, usar verbo "Listar" + substantivo no plural (ListarProjetos)

### 6. Códigos de Status HTTP Padronizados

| Código HTTP               | Uso                            | Quando Usar                                    |
| ------------------------- | ------------------------------ | ---------------------------------------------- |
| 200 OK                    | Sucesso com retorno (GET, PUT) | Operação bem-sucedida com dados no corpo       |
| 201 Created               | Recurso criado (POST)          | Recurso criado com sucesso                     |
| 204 No Content            | Sucesso sem retorno (DELETE)   | Operação bem-sucedida sem corpo de resposta    |
| 400 Bad Request           | Erro de validação              | Request inválido (validação, regra de negócio) |
| 401 Unauthorized          | Não autenticado                | Token JWT ausente ou inválido                  |
| 403 Forbidden             | Não autorizado                 | Autenticado mas sem permissão para operação    |
| 404 Not Found             | Recurso não encontrado         | Registro solicitado não existe                 |
| 500 Internal Server Error | Erro do servidor               | Exceção não tratada (deve ser rara)            |

### 7. Metadados OpenAPI Obrigatórios

Todos os endpoints devem incluir:

```csharp
group.MapGet("/{id}", ConsultarProjeto)
    .WithName("ConsultarProjeto")                          // ✅ Nome único para geração de links
    .WithTags("Projetos")                                  // ✅ Agrupamento no Swagger
    .WithSummary("Consulta projeto por ID")                // ✅ Título no Swagger
    .WithDescription("Retorna detalhes...")                // ⚠️ Recomendado
    .WithOpenApi()                                         // ✅ Habilita documentação OpenAPI
    .Produces<ConsultarProjetoResponse>(StatusCodes.Status200OK)  // ✅ Tipo de resposta de sucesso
    .ProducesProblem(StatusCodes.Status400BadRequest)      // ✅ Códigos de erro possíveis (RFC 7807)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .RequireAuthorization();                               // ✅ Política de autorização
```

> **⚠️ IMPORTANTE: Use `ProducesProblem` para Erros**
>
> **CORRETO** ✅:

```csharp
.Produces<ResponseDto>(StatusCodes.Status200OK)      // Respostas de sucesso
.ProducesProblem(StatusCodes.Status400BadRequest)    // Erros (ProblemDetails RFC 7807)
.ProducesProblem(StatusCodes.Status404NotFound)
```

> **INCORRETO** ❌:

```csharp
.Produces(400)                // Número mágico
.Produces(StatusCodes.Status400BadRequest)  // Tipo errado (não documenta ProblemDetails)
```

> **Razão**: `.Produces()` documenta resposta genérica `application/json`. `.ProducesProblem()` documenta corretamente o schema RFC 7807 `application/problem+json` com campos `type`, `title`, `status`, `detail`, `errors`.

---

## Alternativas Consideradas

### 1. Controllers Tradicionais (ASP.NET Core MVC)

**Descrição**: Usar classes `Controller` com atributos `[HttpGet]`, `[HttpPost]`, etc.

**Vantagens:**

- Familiar para desenvolvedores com experiência em ASP.NET MVC
- Suporte a model binding mais rico
- Convenções estabelecidas

**Desvantagens:**

- Overhead de herança de `ControllerBase`
- Menos performático que Minimal APIs (~30% mais lento)
- Mais verboso (precisa de atributos em todos os métodos)
- Dificulta aplicação de Clean Architecture (tentação de colocar lógica no Controller)

**Por que foi rejeitada**: Minimal APIs oferecem melhor performance, menor verbosidade e facilitam separação de responsabilidades (configuração x lógica). Para APIs REST modernas, Minimal APIs são a recomendação oficial da Microsoft desde .NET 6.

---

### 2. ApiResponse como Envelope Padrão

**Descrição**: Encapsular todas as respostas em objeto `ApiResponse<T>` com propriedades `Sucesso`, `Mensagem`, `Dados`, `Erros`.

**Estrutura proposta:**

```csharp
public class ApiResponse<T>
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public T? Dados { get; set; }
    public IReadOnlyList<string>? Erros { get; set; }
}
```

**Vantagens:**

- Estrutura uniforme para todas as respostas
- Facilita tratamento genérico no frontend (alguns cenários)

**Desvantagens:**

- Viola princípio YAGNI (You Aren't Gonna Need It) - overhead desnecessário
- Duplica informação já presente em HTTP (status code, headers)
- Dificulta consumo direto dos dados: `response.data.Dados` em vez de `response.data`
- Não segue padrões RESTful modernos
- Aumenta tamanho do payload
- Confunde responsabilidades: status HTTP já indica sucesso/erro
- Dificulta cache HTTP (envelope muda estrutura)
- Repete a classe `Result<T>` já implementada na camada de Aplicação (Commands/Queries retornam Result, criar ApiResponse seria duplicação de responsabilidade)

**Por que foi rejeitada**:

1. HTTP já possui mecanismos para indicar sucesso/erro (status codes)
2. Retornar dados diretamente no corpo é o padrão RESTful aceito
3. Frontend moderno (Angular HttpClient) já trata responses de forma genérica via interceptors
4. Viola Clean Architecture: camada de apresentação não deve adicionar estruturas desnecessárias

**Referência**: Esta era a abordagem do DT-006, substituída pela presente versão.

---

## Más Práticas e Anti-Padrões a Evitar

Esta seção documenta abordagens que **não devem ser utilizadas** no projeto, mesmo que sejam tecnicamente possíveis ou encontradas em outros projetos.

### ❌ 1. Lambdas Inline no MapEndpoints

**Descrição**: Colocar toda a lógica do endpoint diretamente no lambda do método de mapeamento.

**Exemplo do que NÃO fazer:**

```csharp
group.MapGet("/", async ([FromServices] IQueryHandler<...> handler) =>
{
    // 20-30 linhas de código aqui...
    var request = new ListarProjetosRequest();
    var result = await handler.ExecuteAsync(request);
    // validações, tratamentos...
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(...);
});
```

**Por que é uma má prática:**

- ❌ Viola Single Responsibility Principle (mistura configuração com implementação)
- ❌ Impossibilita testes unitários (não há método isolado para testar)
- ❌ Reduz drasticamente a legibilidade quando lógica cresce
- ❌ Impede reutilização de lógica entre endpoints
- ❌ Dificulta manutenção e code reviews

**Solução correta**: Separar configuração (MapEndpoints) da implementação (métodos privados) conforme seção de Decisão deste documento.

---

### ❌ 2. Parâmetros Individuais em Query Strings

**Descrição**: Mapear cada query parameter individualmente como parâmetros do método.

**Exemplo do que NÃO fazer:**

```csharp
private static async Task<IResult> ListarProjetos(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? nome = null,
    [FromQuery] string? status = null,
    [FromQuery] string? gerencia = null,
    // 10+ parâmetros...
    [FromServices] IQueryHandler<...> handler)
```

**Por que é uma má prática:**

- ❌ Extremamente verboso e difícil de manter
- ❌ Impossibilita validação centralizada dos parâmetros
- ❌ Dificulta testes (precisa passar todos os parâmetros individualmente)
- ❌ Gera documentação Swagger poluída e confusa
- ❌ Impede reutilização do contrato de filtros

**Solução correta**: Reutilizar Request da camada de Aplicação com `[AsParameters]`:

#### ✅ Opção A: Reutilizar Request da camada de Aplicação diretamente (RECOMENDADA)

**Quando usar:** Na maioria dos casos, especialmente quando:

- API é para consumo interno (frontend do próprio projeto)
- Request definido na camada de aplicação já possui todos os parâmetros necessários
- Não há necessidade de contrato API diferente do contrato de aplicação

```csharp
// ✅ 1. Request já definido na camada de Aplicação
// PortalAle.Application/Projetos/Listar/ListarProjetosRequest.cs
public record ListarProjetosRequest(
    int Page = 1,
    int PageSize = 10,
    string? Nome = null,
    string? Status = null);

// ✅ 2. Reutilizar diretamente no endpoint
// PortalAle.Api/Endpoints/ProjetosEndpoints.cs
private static async Task<IResult> ListarProjetos(
    [AsParameters] ListarProjetosRequest request,
    [FromServices] IQueryHandler<ListarProjetosRequest, ListarProjetosResponse> handler,
    CancellationToken cancellationToken)
{
    var result = await handler.ExecuteAsync(request, cancellationToken);
    return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
}
```

**Benefícios:**

- ✅ **Máxima redução de código**: Elimina criação de classes Request duplicadas
- ✅ **DRY (Don't Repeat Yourself)**: Request é o único contrato
- ✅ **Manutenibilidade**: Mudanças em parâmetros ocorrem em um único lugar
- ✅ **Testabilidade**: Testes de integração reutilizam mesmos objetos da aplicação
- ✅ **Consistência**: API reflete exatamente o contrato da camada de aplicação

#### ⚠️ Opção B: Request Separado (Apenas quando necessário)

**Quando usar:** Casos específicos onde:

- API pública externa precisa de contrato estável independente da implementação
- Versioning de API exige manter contrato antigo enquanto aplicação evolui
- API precisa de campos diferentes do que a camada de aplicação
- Nomear a classe/record com sufixo APIRequest ou APIResponse para diferenciar da camada de aplicação

```csharp
// ⚠️ 1. Request específico para API (apenas quando justificável)
// PortalAle.Api/Endpoints/Projetos/ListarProjetosFiltrosAPIRequest.cs
public record ListarProjetosFiltrosAPIRequest(
    int Page = 1,
    int PageSize = 10,
    string? Nome = null,
    string? Status = null);

// ⚠️ 2. Endpoint precisa mapear Request de API → Request de Aplicação
private static async Task<IResult> ListarProjetos(
    [AsParameters] ListarProjetosFiltrosAPIRequest request,
    [FromServices] IQueryHandler<ListarProjetosRequest, ListarProjetosResponse> handler,
    CancellationToken cancellationToken)
{
    // Mapeamento adicional necessário
    var appRequest = new ListarProjetosRequest(
        request.Page,
        request.PageSize,
        request.Nome,
        request.Status);

    var result = await handler.ExecuteAsync(appRequest, cancellationToken);
    return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
}
```

**Desvantagens:**

- ❌ Duplicação de código (Request de API + Request de Aplicação com estrutura semelhante)
- ❌ Mapeamento manual adicional (Request de API → Request de Aplicação)
- ❌ Maior superfície de manutenção (mudanças em 2 lugares)

**Recomendação Geral:**

> **Regra de Ouro**: Prefira **sempre** reutilizar Request da camada de Aplicação diretamente (Opção A). Crie Request separado (Opção B) **apenas** quando houver justificativa arquitetural explícita (API pública, versioning, contratos legados).

---

## Consequências

### Positivas ✅

1. **Clean Architecture**: Endpoints atuam apenas como camada de apresentação, delegando processamento para Aplicação
2. **Testabilidade**: Métodos privados facilmente testáveis sem dependências HTTP
3. **Performance**: Minimal APIs ~30% mais rápido que Controllers tradicionais
4. **CQRS**: Separação explícita entre Queries (leitura) e Commands (escrita)
5. **Legibilidade**: Configuração de rotas separada da lógica facilita compreensão
6. **Manutenibilidade**: Convenções claras reduzem inconsistências entre endpoints
7. **RESTful**: Respostas diretas sem envelope seguem padrões REST modernos
8. **Frontend Friendly**: Dados diretamente no corpo facilitam consumo no frontend React/Next.js

### Negativas ❌

1. **Curva de Aprendizado**: Desenvolvedores acostumados com Controllers precisam adaptar-se
2. **Mudança de Paradigma**: Equipes vindas de padrão ApiResponse<T> precisam refatorar código existente
3. **Documentação**: Requer disciplina para manter metadados OpenAPI completos
4. **Testes**: Métodos privados exigem estratégias de teste diferentes de Controllers públicos

### Mitigações ⚠️

- Code reviews rigorosos para garantir conformidade
- Ferramentas de análise estática para validar convenções
- Refatoração gradual de endpoints existentes (não quebrar produção)
- Utilizar templates e exemplos completos presentes neste documento

---

### Registro no Program.cs

```csharp
// PortalAle.Api/Program.cs
var app = builder.Build();

// Registrar todos os endpoints
app.MapProjetoEndpoints();
app.MapRiscoEndpoints();
app.MapTarefaEndpoints();

app.Run();
```

---

## Métricas/Monitoração

### Conformidade com o Padrão

**Code Reviews**: Verificar se novos endpoints seguem:

- ✅ Separação configuração x implementação (MapEndpoints + métodos privados)
- ✅ Uso correto de IQueryHandler (GET) e ICommandHandler (POST/PUT/DELETE)
- ✅ Retorno direto com IResult
- ✅ Metadados OpenAPI completos
- ✅ Nomenclatura RESTful consistente

**Análise Estática**: Ferramentas como Roslyn Analyzers para validar:

- Métodos MapXxx devem ser públicos
- Métodos de implementação devem ser privados
- Todos os endpoints devem ter WithName, WithTags, WithSummary

### Qualidade de Código

**Métricas SonarQube**:

- Complexidade ciclomática dos métodos de endpoint (alvo: < 10)
- Cobertura de testes dos métodos privados (alvo: > 70%)
- Duplicação de código entre endpoints (alvo: < 3%)

---

## Referências

### Documentação Interna

- [backend-webapi-component.md](../arquitetura/backend-webapi-components.md)

### Documentação Externa

- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) - Documentação oficial Microsoft
- [Minimal API OpenAPI Support](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi) - Documentação Swagger/OpenAPI
- [RESTful API Design Best Practices](https://restfulapi.net/) - Padrões REST
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) - Códigos de status HTTP
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Robert C. Martin
