# DT-020: Implementação de Commands na Camada de Aplicação

> **Metadados do Documento**  
> **Componente:** `Backend`
> **Tipo:** Decisão Técnica
>
> **Propósito:** Definir padrão de implementação de operações de escrita (commands no contexto CQRS) na camada de Aplicação usando contratos `Request/Response`, `IRequest`, Result Pattern e controle transacional
>
> **Quando usar:** Ao implementar operações de escrita (create, update, delete) em casos de uso, criar handlers de command, ou modificar estado da aplicação
>
> **Palavras-chave:** `cqrs` `command-pattern` `command-handler` `irequest` `request-response` `transações` `result-pattern` `clean-architecture` `repositórios`

## Contexto

Na arquitetura Clean Architecture com CQRS, Commands são responsáveis por operações de escrita que modificam o estado da aplicação. Sem padrões claros para implementação de Commands, surgem problemas:

**❌ Problemas Identificados:**

1. **Inconsistência Transacional**: Operações que deveriam ser atômicas executadas sem controle transacional adequado
2. **Acoplamento com Infraestrutura**: Handlers chamando `SaveChangesAsync` diretamente, violando separação de responsabilidades
3. **Falta de Padronização**: Cada desenvolvedor implementa validações e tratamento de erros de forma diferente
4. **Dificuldade de Testes**: Lógica de persistência misturada com lógica de negócio dificulta testes unitários
5. **Ausência de Result Pattern**: Uso de exceções para fluxo de controle de negócio ao invés de resultados tipados

---

## Decisão

Adotar padrão **Command Handler** na camada de Aplicação com as seguintes diretrizes:

1. Requests de escrita são definidos como `record` e implementam `IRequest<Result<TResponse>>` (ou `IRequest<Result>` quando não houver payload de retorno).
2. O sufixo dos contratos de entrada e saída do caso de uso é `Request/Response`.
3. Handlers de escrita implementam `ICommandHandler<TRequest, TResponse>` (ou `ICommandHandler<TRequest>`).
4. `Result Pattern` permanece obrigatório para sucesso/falha sem exceções de fluxo de negócio.

### Princípio Arquitetural

```
┌─────────────────────────────────────────────────────────────┐
│                    Camada de Aplicação                       │
│                                                              │
│  ┌──────────────┐         ┌────────────────────────┐       │
│  │   Request    │────────▶│  CommandHandler        │       │
│  │  (Escrita)   │         │  (Use Case)            │       │
│  └──────────────┘         └────────────────────────┘       │
│                                      │                       │
│                           ┌──────────┼──────────┐           │
│                           ▼          ▼          ▼           │
│                    Repositórios  Serviços  DbContext        │
│                    (Domain)      Externos  (Transação)      │
│                           │          │          │           │
└───────────────────────────┼──────────┼──────────┼───────────┘
                            │          │          │
                            ▼          ▼          ▼
┌───────────────────────────────────────────────────────────────┐
│                  Camada de Infraestrutura                     │
│                                                               │
│  - Implementações de Repositórios (I{Entidade}Repository)    │
│  - Implementações de Serviços Externos                       │
│  - ApplicationDbContext (EF Core)                            │
└───────────────────────────────────────────────────────────────┘
```

**Fluxo de Execução:**

1. **Request de escrita** chega no **CommandHandler** (camada de Aplicação)
2. Handler abre uma **transação** via `DbContext`
3. Handler executa **validações básicas** de entrada
4. Handler chama **repositórios** para operações de domínio
5. Repositórios aplicam **regras de negócio** e chamam `SaveChangesAsync` internamente
6. Handler faz **commit da transação** se tudo ocorreu bem
7. Handler retorna **Result<T>** indicando sucesso ou falha

**Responsabilidades por Camada:**

| Responsabilidade            | Camada         | Componente                     |
| --------------------------- | -------------- | ------------------------------ |
| Validações de entrada       | Application    | CommandHandler                 |
| Orquestração do caso de uso | Application    | CommandHandler                 |
| Gerenciamento de transações | Application    | CommandHandler (via DbContext) |
| Regras de negócio           | Domain         | Entidades/Serviços de Domínio  |
| Persistência                | Infrastructure | Repositórios                   |
| SaveChanges                 | Infrastructure | Repositórios                   |

---

## Consequências

### Positivos

✅ **Atomicidade Garantida**: Transações explícitas garantem que múltiplas operações são atômicas  
✅ **Separação de Responsabilidades**: Handler orquestra, repositórios persistem  
✅ **Result Pattern**: Tratamento de erros estruturado sem exceções para fluxo de controle  
✅ **Testabilidade**: Handlers testáveis com mocks de repositórios e serviços  
✅ **Consistência**: Padrão uniforme para todas as operações de escrita  
✅ **Clean Architecture**: Aplicação depende de abstrações, não de implementações  
✅ **Rastreabilidade**: Cada Request de escrita representa um caso de uso de negócio claro

### Negativos

❌ **Boilerplate**: Necessário criar Request + Handler + Response para cada operação  
❌ **Verbosidade**: Código mais verboso comparado a abordagens mais simples  
❌ **Curva de Aprendizado**: Equipe precisa entender CQRS, Result Pattern e transações

---

## Implementação

### 1. Estrutura de Pastas

```
PortalAle.Application/
├── Base/
│   ├── IRequest.cs                         # Interface base para Requests
│   ├── ICommandHandler.cs                  # Interface base para Handlers
│   └── Result.cs                           # Result Pattern
│
└── Projetos/                               # Agregado Projeto
    ├── IProjetoRepository.cs               # Interface do Repositório (Domain)
    ├── Criar/
    │   ├── CriarProjetoRequest.cs          # Request
    │   ├── CriarProjetoCommandHandler.cs   # Handler (Use Case)
    │   └── CriarProjetoResponse.cs         # Response
    ├── Atualizar/
    │   ├── AtualizarProjetoCommand.cs
    │   ├── AtualizarProjetoCommandHandler.cs
    │   └── AtualizarProjetoResult.cs
    └── Excluir/
        ├── ExcluirProjetoCommand.cs
        └── ExcluirProjetoCommandHandler.cs

PortalAle.Data/
└── Persistencia/
    ├── ApplicationDbContext.cs
    └── Repositorios/
        └── ProjetoRepository.cs            # Implementação concreta
```

**Observações:**

- Commands organizados por agregado e depois por ação
- Interface de repositório (`IProjetoRepository`) fica na camada de Aplicação (ou Domain)
- Implementação concreta do repositório fica na Infraestrutura
- Cada caso de uso de escrita tem sua própria pasta com Request + Handler + Response

---

### 2. Request (Escrita)

**Arquivo:** `PortalAle.Application/Projetos/Criar/CriarProjetoRequest.cs`

```csharp
using PortalAle.Application.Base;

namespace PortalAle.Application.Projetos.Criar;

/// <summary>
/// Request de escrita para criar um novo projeto.
/// </summary>
public record CriarProjetoRequest(
    string Codigo,
    string Nome,
    DateTime DataInicio,
    string? Descricao = null
);
```

**Princípios:**

✅ Usa `record` para imutabilidade  
✅ Implementa `IRequest<TResult>` (interface base)  
✅ Retorna `Result<T>` (Result Pattern)  
✅ Contém apenas dados de entrada (sem lógica)  
✅ Propriedades descrevem a intenção de negócio

---

### 3. CommandHandler (Use Case)

**Arquivo:** `PortalAle.Application/Projetos/Criar/CriarProjetoCommandHandler.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PortalAle.Application.Base;
using PortalAle.Application.Projetos;
using PortalAle.Domain.Projetos;

namespace PortalAle.Application.Projetos.Criar;

public class CriarProjetoCommandHandler
    : ICommandHandler<CriarProjetoRequest, CriarProjetoResponse>
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IAidaProjetoService _aidaService;
    private readonly DbContext _context;

    public CriarProjetoCommandHandler(
        IProjetoRepository projetoRepository,
        IAidaProjetoService aidaService,
        DbContext context)
    {
        _projetoRepository = projetoRepository;
        _aidaService = aidaService;
        _context = context;
    }

    public async Task<Result<CriarProjetoResponse>> ExecuteAsync(
        CriarProjetoRequest request,
        CancellationToken ct = default)
    {
        // Gerenciamento de transação na camada de aplicação
        using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Validações básicas de entrada
            if (string.IsNullOrWhiteSpace(request.Codigo))
                return Result<CriarProjetoResponse>.Failure("Código do projeto é obrigatório");

            if (string.IsNullOrWhiteSpace(request.Nome))
                return Result<CriarProjetoResponse>.Failure("Nome do projeto é obrigatório");

            if (request.DataInicio > DateTime.Today)
                return Result<CriarProjetoResponse>.Failure("Data de início não pode ser futura");

            // 2. Verificar duplicidade (regra de negócio)
            var projetoExistente = await _projetoRepository
                .ObterPorCodigoAsync(request.Codigo, ct);

            if (projetoExistente != null)
                return Result<CriarProjetoResponse>.Failure(
                    $"Já existe um projeto com o código {request.Codigo}");

            // 3. Buscar dados externos (se necessário)
            var projetoAida = await _aidaService
                .BuscarProjetoPorCodigoAsync(request.Codigo, ct);

            if (projetoAida == null)
                return Result<CriarProjetoResponse>.Failure(
                    "Projeto não encontrado no sistema AIDA");

            // 4. Criar entidade do domínio
            var projeto = new Projeto(
                codigo: request.Codigo,
                nome: request.Nome,
                dataInicio: request.DataInicio,
                descricao: request.Descricao
            );

            // 5. Aplicar regras de negócio (método de domínio)
            var validacao = projeto.Validar();
            if (!validacao.IsValid)
                return Result<CriarProjetoResponse>.Failure(validacao.Errors);

            // 6. Persistir (SaveChangesAsync chamado DENTRO do repositório)
            await _projetoRepository.AdicionarAsync(projeto, ct);

            // 7. Commit da transação
            await transaction.CommitAsync(ct);

            // 8. Retornar resultado de sucesso
            return Result<CriarProjetoResponse>.Success(new CriarProjetoResponse(
                Id: projeto.Id,
                Codigo: projeto.Codigo,
                Nome: projeto.Nome
            ));
        }
        catch (Exception ex)
        {
            // Rollback automático ao sair do using, mas podemos fazer explicitamente
            await transaction.RollbackAsync(ct);

            // Re-throw para camadas superiores tratarem
            throw;
        }
    }
}
```

**Princípios aplicados:**

✅ **Transação Explícita**: `BeginTransactionAsync` / `CommitAsync`  
✅ **Validações de Entrada**: No próprio handler  
✅ **Regras de Negócio**: Delegadas para entidades de domínio  
✅ **Persistência Transparente**: `SaveChangesAsync` dentro do repositório  
✅ **Result Pattern**: Retorna `Result<T>` em todos os cenários  
✅ **Injeção de Abstração**: Injeta `DbContext`, não `ApplicationDbContext`  
✅ **Separação de Responsabilidades**: Handler orquestra, não persiste

---

### 4. Response

**Arquivo:** `PortalAle.Application/Projetos/Criar/CriarProjetoResponse.cs`

```csharp
namespace PortalAle.Application.Projetos.Criar;

/// <summary>
/// Response da operação de criação de projeto.
/// </summary>
public record CriarProjetoResponse(
    Guid Id,
    string Codigo,
    string Nome
);
```

**Princípios:**

✅ Usa `record` para imutabilidade  
✅ Contém apenas dados de resposta (sem lógica)  
✅ DTO específico para o caso de uso

---

### 6. Exemplo de Command com Múltiplas Operações

**Cenário:** Aprovar solicitação de acesso (envolve atualizar Solicitacao + criar AcessoUsuario)

```csharp
public class AprovarSolicitacaoCommandHandler
    : ICommandHandler<AprovarSolicitacaoRequest, AprovarSolicitacaoResponse>
{
    private readonly ISolicitacaoAcessoRepository _solicitacaoRepository;
    private readonly IAcessoUsuarioRepository _acessoRepository;
    private readonly INotificacaoService _notificacaoService;
    private readonly DbContext _context;

    public AprovarSolicitacaoCommandHandler(
        ISolicitacaoAcessoRepository solicitacaoRepository,
        IAcessoUsuarioRepository acessoRepository,
        INotificacaoService notificacaoService,
        DbContext context)
    {
        _solicitacaoRepository = solicitacaoRepository;
        _acessoRepository = acessoRepository;
        _notificacaoService = notificacaoService;
        _context = context;
    }

    public async Task<Result<AprovarSolicitacaoResponse>> ExecuteAsync(
        AprovarSolicitacaoRequest request,
        CancellationToken ct = default)
    {
        // Transação garante atomicidade de múltiplas operações
        using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Validações
            if (request.SolicitacaoId == Guid.Empty)
                return Result<AprovarSolicitacaoResponse>.Failure("ID da solicitação inválido");

            // 2. Buscar solicitação
            var solicitacao = await _solicitacaoRepository
                .ObterPorIdAsync(request.SolicitacaoId, ct);

            if (solicitacao == null)
                return Result<AprovarSolicitacaoResponse>.Failure("Solicitação não encontrada");

            // 3. Aplicar regra de negócio (método de domínio)
            var resultadoAprovacao = solicitacao.Aprovar(request.AprovadorId, request.Observacao);
            if (!resultadoAprovacao.IsValid)
                return Result<AprovarSolicitacaoResponse>.Failure(resultadoAprovacao.Errors);

            // 4. Atualizar solicitação (SaveChanges dentro do repositório)
            await _solicitacaoRepository.AtualizarAsync(solicitacao, ct);

            // 5. Criar acesso do usuário
            var acesso = new AcessoUsuario(
                usuarioId: solicitacao.UsuarioId,
                projetoId: solicitacao.ProjetoId,
                perfil: solicitacao.PerfilSolicitado
            );

            // 6. Persistir acesso (SaveChanges dentro do repositório)
            await _acessoRepository.AdicionarAsync(acesso, ct);

            // 7. Enviar notificação (operação externa)
            await _notificacaoService.NotificarAprovacaoAsync(solicitacao, ct);

            // 8. Commit da transação (atomicidade garantida)
            await transaction.CommitAsync(ct);

            // 9. Retornar resultado
            return Result<AprovarSolicitacaoResponse>.Success(new AprovarSolicitacaoResponse(
                SolicitacaoId: solicitacao.Id,
                AcessoId: acesso.Id,
                Status: solicitacao.Status
            ));
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
```

**Observações:**

- Transação garante que **todas** as operações são executadas ou **nenhuma** é executada
- Cada repositório chama `SaveChangesAsync` internamente
- Handler não chama `SaveChangesAsync` diretamente
- Commit final confirma todas as alterações de uma vez

---

### 8. Uso em MinimalAPI Endpoint

**Arquivo:** `PortalAle.Api/Endpoints/ProjetosEndpoints.cs`

```csharp
public static class ProjetosEndpoints
{
    public static void MapProjetosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projetos").WithTags("Projetos");

        group.MapPost("/", async (
            CriarProjetoRequest request,
            ICommandHandler<CriarProjetoRequest, CriarProjetoResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.ExecuteAsync(request, ct);

            return result.IsSuccess
                ? Results.Created($"/api/projetos/{result.Value.Codigo}", result.Value)
                : Results.BadRequest(new { errors = result.Errors });
        })
        .WithName("CriarProjeto")
        .Produces<CriarProjetoResponse>(201)
        .Produces<object>(400);

        group.MapPut("/{codigo}", async (
            string codigo,
            AtualizarProjetoCommand command,
            AtualizarProjetoCommandHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.ExecuteAsync(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { errors = result.Errors });
        })
        .WithName("AtualizarProjeto")
        .Produces<AtualizarProjetoResult>(200)
        .Produces<object>(400);

        group.MapDelete("/{codigo}", async (
            string codigo,
            ExcluirProjetoCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new ExcluirProjetoCommand(codigo);
            var result = await handler.ExecuteAsync(command, ct);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { errors = result.Errors });
        })
        .WithName("ExcluirProjeto")
        .Produces(204)
        .Produces<object>(400);
    }
}
```

---

## Boas Práticas

### 1. Nomenclatura Consistente

**Operações de escrita (Commands no CQRS):**

- Classe Request: `{Ação}{Entidade}Request` (ex: `CriarProjetoRequest`, `AprovarSolicitacaoRequest`)
- Classe Handler: `{Ação}{Entidade}CommandHandler` (**obrigatório** sufixo `CommandHandler`)
- Classe Response: `{Ação}{Entidade}Response`
- Namespace: `PortalAle.Application.{Entidade}.{Ação}`

### 2. Validações em Camadas

**Validações de Entrada (Handler):**

```csharp
// ✅ CORRETO: Validações simples no handler
if (string.IsNullOrWhiteSpace(command.Nome))
    return Result.Failure("Nome é obrigatório");

if (command.DataInicio > DateTime.Today)
    return Result.Failure("Data de início não pode ser futura");
```

**Validações de Negócio (Domínio):**

```csharp
// ✅ CORRETO: Regras de negócio na entidade
var projeto = new Projeto(/*...*/);
var validacao = projeto.Validar();  // Método de domínio

if (!validacao.IsValid)
    return Result.Failure(validacao.Errors);
```

### 3. Controle Transacional

**✅ CORRETO: Transação gerenciada pelo handler**

```csharp
using var transaction = await _context.Database.BeginTransactionAsync(ct);

try
{
    // Múltiplas operações
    await _repository1.AdicionarAsync(entidade1, ct);
    await _repository2.AtualizarAsync(entidade2, ct);

    // Commit da transação
    await transaction.CommitAsync(ct);

    return Result.Success();
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

**❌ ERRADO: Sem transação**

```csharp
// Sem transação - não garante atomicidade!
await _repository1.AdicionarAsync(entidade1, ct);
await _repository2.AtualizarAsync(entidade2, ct);  // Se falhar, primeira persiste!
```

### 5. Result Pattern

**✅ CORRETO: Sempre retornar Result**

```csharp
// Sucesso
return Result<CriarProjetoResponse>.Success(resultado);

// Falha com um erro
return Result<CriarProjetoResponse>.Failure("Mensagem de erro");

// Falha com múltiplos erros
return Result<CriarProjetoResponse>.Failure(validacao.Errors);

// Sucesso sem valor de retorno
return Result.Success();
```

**❌ ERRADO: Lançar exceções para fluxo de negócio**

```csharp
if (projetoExistente != null)
    throw new BusinessException("Projeto já existe");  // ❌ Não fazer isso!
```

### 7. Um Command por Caso de Uso

**✅ CORRETO: Commands específicos**

```csharp
// Um command para cada intenção de negócio
CriarProjetoRequest
AtualizarProjetoRequest
AtivarProjetoRequest
DesativarProjetoRequest
```

**❌ ERRADO: Command genérico**

```csharp
// ❌ Muito genérico, dificulta rastreabilidade
GerenciarProjetoCommand { Acao = "criar" | "atualizar" | "excluir" }
```

### 8. Testes Unitários

**Exemplo de teste do CommandHandler:**

```csharp
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class CriarProjetoCommandHandlerTests
{
    [TestMethod]
    public async Task ExecuteAsync_ProjetoValido_RetornaSucesso()
    {
        // Arrange
        var mockRepository = new Mock<IProjetoRepository>();
        mockRepository
            .Setup(r => r.ObterPorCodigoAsync("PROJ001", default))
            .ReturnsAsync((Projeto?)null);  // Não existe duplicado

        var mockAidaService = new Mock<IAidaProjetoService>();
        mockAidaService
            .Setup(s => s.BuscarProjetoPorCodigoAsync("PROJ001", default))
            .ReturnsAsync(new ProjetoAida { Codigo = "PROJ001", Nome = "Teste" });

        var mockContext = new Mock<DbContext>();
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockContext
            .Setup(c => c.Database.BeginTransactionAsync(default))
            .ReturnsAsync(mockTransaction.Object);

        var handler = new CriarProjetoCommandHandler(
            mockRepository.Object,
            mockAidaService.Object,
            mockContext.Object);

        var request = new CriarProjetoRequest(
            Codigo: "PROJ001",
            Nome: "Projeto Teste",
            DataInicio: DateTime.Today);

        // Act
        var result = await handler.ExecuteAsync(request, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("PROJ001", result.Value.Codigo);

        mockRepository.Verify(r => r.AdicionarAsync(
            It.Is<Projeto>(p => p.Codigo == "PROJ001"),
            default), Times.Once);

        mockTransaction.Verify(t => t.CommitAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_CodigoVazio_RetornaFalha()
    {
        // Arrange
        var handler = new CriarProjetoCommandHandler(/*mocks*/);
        var request = new CriarProjetoRequest(
            Codigo: "",  // Inválido
            Nome: "Projeto Teste",
            DataInicio: DateTime.Today);

        // Act
        var result = await handler.ExecuteAsync(request, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("obrigatório", result.Errors.First());
    }

    [TestMethod]
    public async Task ExecuteAsync_ProjetoDuplicado_RetornaFalha()
    {
        // Arrange
        var mockRepository = new Mock<IProjetoRepository>();
        mockRepository
            .Setup(r => r.ObterPorCodigoAsync("PROJ001", default))
            .ReturnsAsync(new Projeto(/*...*/ ));  // Já existe

        var handler = new CriarProjetoCommandHandler(mockRepository.Object, /*outros mocks*/);
        var request = new CriarProjetoRequest(
            Codigo: "PROJ001",
            Nome: "Projeto Teste",
            DataInicio: DateTime.Today);

        // Act
        var result = await handler.ExecuteAsync(request, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("já existe", result.Errors.First());
    }
}
```

---

## Referências

**Padrões Arquiteturais:**

- [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)
- [Microsoft: CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Vladimir Khorikov: Result Pattern](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

**Entity Framework Core:**

- [Microsoft: Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- [Microsoft: DbContext Lifetime](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)

**Decisões Técnicas Relacionadas:**

- [DT-004: Mapeamento Objeto-Relacional com EF Core](DT-004-mapeamento-objeto-relacional-ef-core.md)
- [DT-016: Padrões Command e Query na Camada de Aplicação](DT-016-padroes-camada-aplicacao.md)
- [DT-018: Padrão de Implementação de Repositórios](DT-018-padrao-implementacao-repositorios.md)
- [DT-019: Implementação de Query Services na Camada de Aplicação](DT-019-cqrs-query-camada-aplicacao.md)

---

## Verificação de Conformidade

Use este checklist para implementar e auditar se os padrões estão sendo seguidos:

### Request (Escrita)

- [ ] Classe `{Ação}{Entidade}Request` criada
- [ ] Usa `record` para imutabilidade
- [ ] Implementa `IRequest<Result<TResponse>>` (ou `IRequest<Result>`)
- [ ] Contém apenas propriedades de entrada (sem lógica)
- [ ] Propriedades com nomes descritivos do negócio
- [ ] Namespace: `PortalAle.Application.{Entidade}.{Ação}`

### CommandHandler (Use Case)

- [ ] Classe `{Ação}{Entidade}CommandHandler` criada (**obrigatório** sufixo `CommandHandler`)
- [ ] Implementa `ICommandHandler<TRequest, TResponse>` (ou `ICommandHandler<TRequest>`)
- [ ] Injeta `DbContext` (abstração) para transações
- [ ] Injeta `I{Entidade}Repository` (interfaces, não implementações concretas)
- [ ] Injeta serviços externos via interfaces
- [ ] Validações básicas de entrada implementadas
- [ ] Verificações de duplicidade/existência antes de persistir
- [ ] Regras de negócio delegadas para entidades de domínio
- [ ] **Transação explícita**: `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`
- [ ] **NÃO chama `SaveChangesAsync`** (responsabilidade do repositório)
- [ ] Retorna `Result<TResponse>` (ou `Result`) em todos os cenários (sucesso e falha)
- [ ] Try-catch com rollback de transação

### Response

- [ ] Classe `{Ação}{Entidade}Response` criada
- [ ] Usa `record` para imutabilidade
- [ ] Contém apenas dados de resposta (sem lógica)
- [ ] DTO específico para o caso de uso

### Repositório (Infrastructure)

- [ ] Interface `I{Entidade}Repository` definida na camada de Aplicação
- [ ] Implementação concreta em `Infraestrutura/Persistencia/Repositorios/`
- [ ] Métodos retornam entidades de domínio (não DTOs)
- [ ] **`SaveChangesAsync` chamado DENTRO dos métodos do repositório**
- [ ] Usa `Include()` para carregar agregados quando necessário
- [ ] Suporte a `CancellationToken` em todos os métodos async

### Registro no DI

- [ ] Repositórios registrados: `builder.Services.AddScoped<I{Entidade}Repository, {Entidade}Repository>()`
- [ ] `ApplicationDbContext` registrado como `DbContext` (abstração)
- [ ] Handlers registrados automaticamente via scan do assembly

### MinimalAPI Endpoints

- [ ] Endpoint injeta handler via parâmetro
- [ ] Command construído a partir de dados da requisição
- [ ] Retorna códigos HTTP corretos:
  - 201 (Created) para criação com sucesso
  - 200 (OK) para atualização com sucesso
  - 204 (No Content) para exclusão com sucesso
  - 400 (Bad Request) para erros de validação
- [ ] Endpoint documentado com `.WithName()` e `.WithTags()`
- [ ] Usa `Results` do MinimalAPI

### Testes Unitários

- [ ] Testes do handler usando mocks de repositórios e serviços
- [ ] Testes cobrem cenários de sucesso
- [ ] Testes cobrem cenários de validação (entrada inválida)
- [ ] Testes cobrem cenários de negócio (duplicidade, não encontrado, etc.)
- [ ] Verificação de chamadas aos métodos usando `Verify()`
- [ ] Verificação de commit/rollback de transação

### Arquitetura

- [ ] Application não referencia Infrastructure diretamente
- [ ] Application depende apenas de abstrações (interfaces)
- [ ] Infrastructure implementa interfaces definidas na Application
- [ ] Dependências corretas: `WebAPI → Application ← Infrastructure`
- [ ] Sem dependência cíclica

### Orientações para GitHub Copilot / Assistentes de IA

Ao gerar código de Commands, sempre seguir:

✅ **Nomenclatura**: `{Ação}{Entidade}Request/CommandHandler/Response`  
✅ **Nomenclatura de contratos**: `{Ação}{Entidade}Request/Response`  
✅ **Handlers com sufixo**: Sempre `CommandHandler` (nunca `UseCase` ou `Service`)  
✅ **Contrato base de request**: usar `IRequest`  
✅ **Transações Explícitas**: Usar `BeginTransactionAsync` / `CommitAsync`  
✅ **SaveChanges no Repositório**: NUNCA chamar no handler  
✅ **Injetar Abstrações**: `DbContext` e `I{Entidade}Repository` (não implementações)  
✅ **Result Pattern**: Sempre retornar `Result<T>` ou `Result`  
✅ **Validações em Camadas**: Entrada no handler, negócio no domínio  
✅ **Um Command por Caso de Uso**: Não criar commands genéricos  
✅ **Try-Catch com Rollback**: Garantir rollback em caso de exceção  
✅ **Testes com Mocks**: Testar handlers isoladamente
