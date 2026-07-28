# DT-019: Implementação de Query Services na Camada de Aplicação

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Definir padrão de implementação de Query (CQRS) usando inversão de dependência para evitar acoplamento da camada de Aplicação com Entity Framework
>
> **Quando usar:** Ao implementar operações de leitura (queries) em casos de uso, criar handlers de query, ou consultar dados para APIs
>
> **Palavras-chave:** `cqrs` `query-services` `inversão-dependência` `clean-architecture` `irequest` `paginationrequest` `isortablerequest` `paginationextensions` `sortingextensions`

## Contexto

Na arquitetura Clean Architecture com CQRS, a separação entre Commands (escrita) e Queries (leitura) é fundamental. Porém, surge um problema arquitetural quando handlers de Query na camada de Aplicação precisam acessar dados:

**❌ Problema Identificado:**

Injetar diretamente `DbContext` ou `ApplicationDbContext` nos handlers de Query gera:

1. **Dependência Cíclica**: Application passa a depender de Infrastructure (Entity Framework)
2. **Violação da Clean Architecture**: Dependências devem apontar para dentro, mas Application → Infrastructure viola essa regra
3. **Acoplamento com EF**: Application fica acoplada a uma tecnologia específica (Entity Framework)
4. **Dificuldade de Testes**: Handlers dependem de implementação concreta, dificultando mocks
5. **Baixa Flexibilidade**: Impossível trocar implementação (EF → Dapper, SP, etc.) sem modificar Application

**❌ Problema Alternativo (Usar Repositórios para Queries):**

Usar repositórios existentes (projetados para o domínio) para consultas também é problemático:

1. **Ineficiência de Performance**: Repositórios retornam entidades completas do domínio, carregando todos os dados mesmo quando apenas alguns campos são necessários
2. **Tradução Adicional**: Necessidade de mapear `Entidade → DTO` após carregar a entidade completa, desperdiçando memória e processamento
3. **Quebra de Single Responsibility**: Repositórios devem servir o domínio (operações com agregados), não servir queries otimizadas para apresentação
4. **Queries Complexas**: Dificulta implementação de queries com agregações, joins complexos ou projeções específicas
5. **Rastreamento de Entidades**: EF rastreia entidades carregadas por repositórios, consumindo recursos desnecessários em operações read-only

---

## Decisão

Adotar **inversão de dependência** para queries usando interfaces de Query Services:

**Padronizações adotadas na camada de Aplicação (Base):**

1. Requests de casos de uso são definidos como **records** e seguem o contrato comum `IRequest`.
2. `ICommand`/`IQuery` não são usados como contratos de request; `IRequest` centraliza esse papel.
3. Paginação é padronizada por `IPaginationRequest` + `PaginationRequest` e resposta `PaginationResponse<T>`.
4. Ordenação é padronizada por `ISortableRequest`.
5. `IQuery` permanece como interface marcadora para **contratos de consulta** (`I{Entidade}Queries`) registrados por varredura no DI da infraestrutura.

### Princípio Arquitetural

```
┌─────────────────────────────────────────────────────────────┐
│                    Camada de Aplicação                       │
│                                                              │
│  ┌──────────────┐         ┌────────────────────────┐       │
│  │ QueryHandler │ ───────▶│ I{Entidade}Queries     │       │
│  │              │         │ (Interface)            │       │
│  └──────────────┘         └────────────────────────┘       │
│                                      ▲                       │
└──────────────────────────────────────┼───────────────────────┘
                                       │
                                       │ implementa
                                       │
┌──────────────────────────────────────┼───────────────────────┐
│                    Camada de Infraestrutura                  │
│                                      │                       │
│                   ┌──────────────────┴──────────────────┐   │
│                   │ {Entidade}Queries                   │   │
│                   │ (Implementação Concreta)            │   │
│                   │  - Usa ApplicationDbContext         │   │
│                   │  - EF, Dapper, SQL, etc.            │   │
│                   └─────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

**Fluxo:**

1. **Application** define o contrato `I{Entidade}Queries` (interface)
2. **Infrastructure** implementa a interface usando `ApplicationDbContext` (EF Core)
3. **QueryHandler** (Application) injeta apenas a interface
4. **WebAPI** registra a implementação via DI

✅ Application depende apenas de abstração  
✅ Infrastructure depende de Application (implementa contrato)  
✅ Sem dependência cíclica  
✅ Sem acoplamento com EF

---

## Alternativas Consideradas

### 1. Usar DbContext Diretamente na Aplicação

**Rejeitado**: Gera dependência cíclica e viola Clean Architecture.

```csharp
// ❌ Problema: Application → Infrastructure
public class QueryHandler
{
    private readonly DbContext _context;  // ❌ Dependência errada!
}
```

### 2. Usar Repositórios para Queries

**Rejeitado**: Repositórios existem para servir o domínio — carregam a entidade de domínio completa para que regras de negócio e invariantes possam ser aplicadas. Usá-los em QueryHandlers para retornar dados de leitura é um **anti-pattern** com impactos diretos de performance e arquitetura.

> **⚠️ Anti-pattern: QueryHandler usando repositório**
>
> ```csharp
> // ❌ ERRADO — Não faça isto em QueryHandlers de leitura
> public class ListarVeiculosQueryHandler : IQueryHandler<ListarVeiculosRequest, PaginationResponse<VeiculoResponse>>
> {
>     private readonly IVeiculoRepository _veiculoRepository; // ❌ Repositório de domínio
>
>     public async Task<Result<PaginationResponse<VeiculoResponse>>> ExecuteAsync(...)
>     {
>         // ❌ Carrega entidade completa + rastreamento EF
>         var (veiculos, total) = await _veiculoRepository.ListarAsync(...);
>
>         // ❌ Mapeamento manual posterior (desperdiça memória já alocada)
>         var items = veiculos.Select(v => new VeiculoResponse(v.Id, v.Nome, ...)).ToList();
>         ...
>     }
> }
> ```
>
> **Problemas:**
>
> 1. O repositório carrega a entidade completa do banco (todos os campos, incluindo agregados)
> 2. O EF rastreia todas as entidades carregadas (`ChangeTracker`), consumindo memória desnecessária em read-only
> 3. O mapeamento `Entidade → DTO` ocorre depois que os dados já foram transferidos do banco para a memória
> 4. Impossível criar projeções com campos calculados, JOINs otimizados ou agregações via SQL
> 5. O `SELECT *` implícito nunca aproveita índices de cobertura

O padrão correto é sempre usar `I{Entidade}Queries`, conforme detalhado na seção de Implementação abaixo.

**Exceção única**: operações que exigem verificação de regras de domínio _antes_ de retornar dados (ex.: consultar uma entidade para validar se ela pode ser editada). Mesmo nesses casos, prefira uma query otimizada para leitura e acesse o repositório apenas para a operação de escrita subsequente.

---

## Consequências

### Positivos

✅ **Clean Architecture Mantida**: Application não depende de Infrastructure  
✅ **Inversão de Dependência**: Application define contratos, Infrastructure implementa  
✅ **Flexibilidade**: Pode-se trocar implementação (EF → Dapper) sem afetar Application  
✅ **Testabilidade**: Fácil criar mocks das interfaces para testes unitários  
✅ **Separação de Responsabilidades**: Queries otimizadas isoladas em classes específicas  
✅ **Performance**: Implementação pode usar projeções otimizadas, Dapper, SQL bruto, etc.  
✅ **Sem Acoplamento**: Application desacoplada de tecnologia específica (EF Core)

### Negativos

❌ **Boilerplate**: Necessário criar interface + implementação para cada entidade  
❌ **Mais Arquivos**: Organização requer pasta `Interfaces/` na Application e `Queries/` na Infrastructure  
❌ **Curva de Aprendizado**: Equipe precisa entender inversão de dependência

---

## Implementação

### 1. Estrutura de Pastas

```
ProjSub.Aplicacao/                          # Camada de Aplicação
├── Base/
│   ├── IRequest.cs
│   ├── IQuery.cs
│   ├── IQueryHandler.cs
│   ├── IPaginationRequest.cs
│   ├── PaginationRequest.cs
│   ├── ISortableRequest.cs
│   ├── PaginationResponse.cs
│   └── Result.cs
│
└── Projetos/
    ├── IProjetoQueries.cs                  # Interface de Query (no agregado)
    ├── Consultar/
    │   ├── ConsultarProjetoRequest.cs
    │   ├── ConsultarProjetoQueryHandler.cs  # Usa IProjetoQueries
    │   └── ConsultarProjetoResponse.cs
    └── Listar/
        ├── ListarProjetosRequest.cs
        ├── ListarProjetosQueryHandler.cs
        └── ListarProjetosResponse.cs

ProjSub.Aplicacao/
└── SolicitacoesAcesso/
    ├── ISolicitacaoAcessoQueries.cs        # Interface de Query (no agregado)
    └── ...

ProjSub.Infraestrutura/                     # Camada de Infraestrutura
├── Persistencia/
│   ├── ApplicationDbContext.cs
│   ├── Configuracoes/
│   ├── Repositorios/
│   └── Queries/                            # Implementações de Query
│       ├── ProjetoQueries.cs               # Implementa IProjetoQueries
│       ├── SolicitacaoAcessoQueries.cs
│       └── ...
└── DependencyInjection.cs
```

**Observações:**

- **Interfaces de Query no agregado**: `IProjetoQueries.cs` fica em `Aplicacao/Projetos/`
- **Infrastructure/Persistencia/Queries/**: Implementações concretas usando EF Core
- Organização por funcionalidade (agregado), não por tipo (interface)
- Cada entidade tem sua própria interface e implementação

---

### 2. Definir Interface de Query (Application)

**Arquivo:** `ProjSub.Aplicacao/Projetos/IProjetoQueries.cs`

Este documento aceita **duas estratégias válidas** para contratos de consulta na Application:

1. **Interface por agregado** (ex.: `IProjetoQueries`) centralizando múltiplas consultas do agregado.
2. **Interface por caso de uso** (ex.: `IListarProjetosQuery`) localizada no módulo do caso de uso (ex.: `Projetos/Listar/`) com apenas o método daquela consulta.

Ambas as opções são válidas; a escolha deve considerar coesão e complexidade do agregado/caso de uso.

```csharp
using ProjSub.Aplicacao.Projetos.Consultar;
using ProjSub.Aplicacao.Projetos.Listar;
using ProjSub.Aplicacao.Base;

namespace ProjSub.Aplicacao.Projetos;

/// <summary>
/// Contrato de consultas (read-only) para Projetos.
/// Implementação concreta na camada de Infraestrutura.
/// </summary>
public interface IProjetoQueries : IQuery
{
    /// <summary>
    /// Consulta projeto por código.
    /// </summary>
    Task<ConsultarProjetoResponse?> ConsultarProjetoPorCodigoAsync(
        string codigo,
        CancellationToken ct = default);

    /// <summary>
    /// Lista projetos com paginação e filtros.
    /// </summary>
    Task<PaginationResponse<ListarProjetosResponse>> ListarProjetosAsync(
        ListarProjetosRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Obtém estatísticas de um projeto.
    /// </summary>
    Task<EstatisticasProjetoResponse> ObterEstatisticasAsync(
        Guid projetoId,
        CancellationToken ct = default);
}
```

**Princípios:**

✅ Interface definida na camada de Aplicação  
✅ Retorna DTOs criados na Application  
✅ Nenhuma referência a EF ou DbContext  
✅ Apenas contrato (sem implementação)  
✅ Métodos com sufixo `Async` e `CancellationToken`  
✅ Interface pode herdar de `IQuery` para registro automático no DI (query services)
✅ Para listagens, preferir passar um único `Request` com paginação/ordenação/filtros

**Exemplo alternativo por caso de uso:**

```csharp
namespace ProjSub.Aplicacao.Projetos.Listar;

public interface IListarProjetosQuery : IQuery
{
    Task<PaginationResponse<ListarProjetosResponse>> ExecuteAsync(
    ListarProjetosRequest request,
        CancellationToken ct = default);
}
```

---

### 3. QueryHandler Usando a Interface (Application)

**Arquivo:** `ProjSub.Aplicacao/Projetos/Consultar/ConsultarProjetoQueryHandler.cs`

```csharp
using ProjSub.Aplicacao.Base;
using ProjSub.Aplicacao.Projetos;

namespace ProjSub.Aplicacao.Projetos.Consultar;

public class ConsultarProjetoQueryHandler
    : IQueryHandler<ConsultarProjetoRequest, ConsultarProjetoResponse>
{
    private readonly IProjetoQueries _projetoQueries;

    public ConsultarProjetoQueryHandler(IProjetoQueries projetoQueries)
    {
        _projetoQueries = projetoQueries;
    }

    public async Task<Result<ConsultarProjetoResponse>> ExecuteAsync(
        ConsultarProjetoRequest request,
        CancellationToken ct = default)
    {
        // Validações básicas
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return Result<ConsultarProjetoResponse>.Failure("Código do projeto obrigatório");

        // Delega para o serviço de query (implementado na Infrastructure)
        var resultado = await _projetoQueries.ConsultarProjetoPorCodigoAsync(request.Codigo, ct);

        if (resultado == null)
            return Result<ConsultarProjetoResponse>.Failure("Projeto não encontrado");

        return Result<ConsultarProjetoResponse>.Success(resultado);
    }
}
```

**Princípios:**

✅ Injeta apenas `IProjetoQueries` (interface)  
✅ Sem dependência de DbContext ou EF  
✅ Validações simples no handler  
✅ Retorna `Result<T>` (Result Pattern)  
✅ Fácil criar mock para testes unitários

---

### 3.1 Exemplo de request simplificado com records (paginação + ordenação)

```csharp
public record ListarProjetosRequest : PaginationRequest, ISortableRequest
{
    public string? FiltroNome { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}
```

**Padrão aplicado:**

✅ Request em `record`  
✅ Paginação padronizada por `PaginationRequest`  
✅ Ordenação padronizada por `ISortableRequest`  
✅ Uso do mesmo request no endpoint, handler e query service
✅ Uso de `NormalizedPageIndex`/`NormalizedPageSize` no fluxo de listagem

---

### 4. Implementação Concreta (Infrastructure)

**Arquivo:** `ProjSub.Infraestrutura/Persistencia/Queries/ProjetoQueries.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ProjSub.Aplicacao.Projetos;
using ProjSub.Aplicacao.Projetos.Consultar;
using ProjSub.Aplicacao.Projetos.Listar;
using ProjSub.Aplicacao.Base;
using ProjSub.Infraestrutura.Persistencia.Extensions;

namespace ProjSub.Infraestrutura.Persistencia.Queries;

public class ProjetoQueries : IProjetoQueries
{
    private readonly ApplicationDbContext _context;
    private static readonly IReadOnlyDictionary<string, Expression<Func<Projeto, object>>> SortMap =
        new Dictionary<string, Expression<Func<Projeto, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["codigo"] = p => p.Codigo,
            ["nome"] = p => p.Nome,
            ["status"] = p => p.Status
        };

    public ProjetoQueries(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConsultarProjetoResponse?> ConsultarProjetoPorCodigoAsync(
        string codigo,
        CancellationToken ct = default)
    {
        return await _context.Projetos
            .AsNoTracking()  // Read-only (sem rastreamento EF)
            .Where(p => p.Codigo == codigo)
            .Select(p => new ConsultarProjetoResponse  // Projeção otimizada
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nome = p.Nome,
                Status = p.Status,
                DataInicio = p.DataInicio,
                QuantidadeSolicitacoes = p.Solicitacoes.Count  // COUNT no banco
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginationResponse<ListarProjetosResponse>> ListarProjetosAsync(
        ListarProjetosRequest request,
        CancellationToken ct = default)
    {
        var queryable = _context.Projetos.AsNoTracking();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(request.FiltroNome))
            queryable = queryable.Where(p => p.Nome.Contains(request.FiltroNome));

        // Ordenação + Paginação + Projeção padronizadas
        return await queryable
            .ApplySorting(request.SortBy, request.IsSortDescending, SortMap)
            .Select(p => new ListarProjetosResponse(
                p.Codigo,
                p.Nome,
                p.Status
            ))
            .ToPagedResponseAsync(request.NormalizedPageIndex, request.NormalizedPageSize, ct);
    }

    public async Task<EstatisticasProjetoResponse> ObterEstatisticasAsync(
        Guid projetoId,
        CancellationToken ct = default)
    {
        // Exemplo: Query complexa com múltiplas tabelas
        var stats = await _context.Projetos
            .AsNoTracking()
            .Where(p => p.Id == projetoId)
            .Select(p => new EstatisticasProjetoResponse
            {
                TotalSolicitacoes = p.Solicitacoes.Count,
                SolicitacoesAprovadas = p.Solicitacoes.Count(s => s.Status == "Aprovada"),
                TotalDocumentos = p.Documentos.Count,
                UltimaAtualizacao = p.Documentos.Max(d => d.DataCriacao)
            })
            .FirstOrDefaultAsync(ct);

        return stats ?? new EstatisticasProjetoResponse();
    }
}
```

**Técnicas de Otimização Utilizadas:**

✅ **`.AsNoTracking()`**: Desabilita rastreamento EF (performance)  
✅ **`.Select()` direto para DTO**: Não materializa entidades completas  
✅ **Agregações no banco**: `Count()`, `Max()`, `Sum()` executados via SQL  
✅ **Paginação padronizada**: `PaginationExtensions.ToPagedResponseAsync(...)`  
✅ **Ordenação padronizada**: `SortingExtensions.ApplySorting(...)` com `sortMap`

**Flexibilidade:**

- Pode-se trocar para **Dapper** sem afetar Application
- Pode-se usar **SQL bruto** para queries complexas
- Pode-se usar **Stored Procedures** se necessário
- Pode-se criar **Views materializadas** no banco

---

### 5. Registro no DI (WebAPI)

**Arquivo:** `ProjSub.WebAPI/Program.cs` ou `ProjSub.Infraestrutura/DependencyInjection.cs`

```csharp
using ProjSub.Aplicacao.Base;

// Registrar automaticamente implementações de interfaces que herdam de IQuery
services.AddScopedFromInterface(typeof(IQuery));
```

**Alternativa também válida via Repositório:**

- Em vez de classe `ProjetoQueries`, a implementação pode ser feita no próprio repositório da raiz de agregação (ex.: `ProjetoRepositorio`), desde que implemente explicitamente o contrato de query (`IProjetoQueries` ou `IListarProjetosQuery`).
- Essa abordagem também é aceita em revisão quando mantém coesão, performance de leitura e ausência de acoplamento da Aplicação com `DbContext`.

**Princípio:**

✅ WebAPI referencia Application + Infrastructure  
✅ Application referencia apenas Domain  
✅ Infrastructure referencia Application (implementa contratos)  
✅ Dependências corretas: `WebAPI → Application ← Infrastructure`

---

### 5.1 Padrão de uso da interface `IQuery`

No contexto deste projeto, `IQuery` é um **marcador de contratos de consulta** na Aplicação.

- `I{Entidade}Queries : IQuery` define o que a aplicação precisa consultar.
- A Infraestrutura implementa o contrato concreto (ex.: `{Entidade}Queries`).
- O DI registra automaticamente interfaces derivadas de `IQuery`.

Esse padrão mantém a aplicação orientada a contrato e desacoplada da implementação de persistência.

---

## Boas Práticas

### 1. Nomenclatura Consistente

**Interfaces:**

- `I{Entidade}Queries` (ex: `IProjetoQueries`, `ISolicitacaoAcessoQueries`)
- Localização: `ProjSub.Aplicacao/{Entidade}/` (ex: `ProjSub.Aplicacao/Projetos/IProjetoQueries.cs`)

**Implementações:**

- `{Entidade}Queries` (ex: `ProjetoQueries`, `SolicitacaoAcessoQueries`)
- Localização: `ProjSub.Infraestrutura/Persistencia/Queries/`

**Métodos:**

- Verbos claros: `ConsultarPorCodigoAsync`, `ListarAsync`, `ObterEstatisticasAsync`
- Sempre com sufixo `Async` e parâmetro `CancellationToken`

**DTOs da camada de Aplicação:**

- Usar sufixos `Request` e `Response`
- Evitar sufixos `Query` e `Result` para DTOs

### 2.Retornar DTOs Específicos

**✅ CORRETO:**

```csharp
// Interface retorna DTO específico da query
Task<ConsultarProjetoResponse?> ConsultarProjetoPorCodigoAsync(
    string codigo,
    CancellationToken ct = default);
```

**❌ ERRADO:**

```csharp
// Retornar entidade de domínio quebra o propósito
Task<Projeto> ConsultarProjetoPorCodigoAsync(
    string codigo,
    CancellationToken ct = default);
```

### 3. Queries Sempre Read-Only

**✅ CORRETO:**

```csharp
// Query sem efeitos colaterais
public async Task<ConsultarProjetoResponse?> ConsultarProjetoPorCodigoAsync(/*...*/)
{
    return await _context.Projetos
        .AsNoTracking()  // Read-only
    .Select(p => new ConsultarProjetoResponse { /*...*/ })
        .FirstOrDefaultAsync(ct);
}
```

**❌ ERRADO:**

```csharp
// Query com efeito colateral (NUNCA fazer isso!)
public async Task<ConsultarProjetoResponse?> ConsultarProjetoPorCodigoAsync(/*...*/)
{
    var projeto = await _context.Projetos.FindAsync(id);

    projeto.UltimaConsulta = DateTime.Now;  // ❌ Efeito colateral!
    await _context.SaveChangesAsync();      // ❌ Modificando estado!

    return new ConsultarProjetoResponse { /*...*/ };
}
```

### 4. Organização das Interfaces de Query

**Organização:**

- **Por agregado**: `IProjetoQueries` concentra consultas de Projeto
- **Por caso de uso**: `IListarProjetosQuery` em `Projetos/Listar/` concentra apenas a consulta de listagem
- Implementação pode estar em classe `{Entidade}Queries` **ou** no repositório do agregado (ex.: `ProjetoRepositorio`)
- Ambas as opções são válidas

**Evitar:**

- ❌ Interface genérica `IQueryService` para todas as entidades
- ❌ Divisão sem critério de coesão (múltiplas interfaces redundantes para o mesmo contexto)

---

## Referências

**Padrões Arquiteturais:**

- [Martin Fowler: Inversion of Control](https://martinfowler.com/bliki/InversionOfControl.html)
- [Microsoft: Dependency Inversion Principle](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles#dependency-inversion)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern - Microsoft](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)

**Entity Framework Core:**

- [Microsoft: Querying Data](https://learn.microsoft.com/en-us/ef/core/querying/)
- [Microsoft: AsNoTracking Queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking#no-tracking-queries)
- [Microsoft: Projections](https://learn.microsoft.com/en-us/ef/core/querying/projections)

**Decisões Técnicas Relacionadas:**

- [DT-004: Mapeamento Objeto-Relacional com EF Core](DT-004-mapeamento-objeto-relacional-ef-core.md)
- [DT-016: Padrões Command e Query na Camada de Aplicação](DT-016-padroes-command-query-camada-aplicacao.md)
- [DT-018: Padrão de Implementação de Repositórios](DT-018-padrao-implementacao-repositorios.md)
- [DT-020: Implementação de Commands na Camada de Aplicação](DT-020-cqrs-commands-camada-aplicacao.md)

---

## Verificação de Conformidade

Use este checklist para implementar e auditar se os padrões estão sendo seguidos:

### Interfaces de Query (Application)

- [ ] Interface `I{Entidade}Queries` criada em `ProjSub.Aplicacao/{Entidade}/` (ex: `Projetos/IProjetoQueries.cs`)
- [ ] Ou interface específica por caso de uso (ex: `Projetos/Listar/IListarProjetosQuery.cs`)
- [ ] Requests de query definidos como `record` e usando `IRequest`
- [ ] Não usar `ICommand`/`IQuery` como contrato de request
- [ ] Métodos com sufixo `Async` e parâmetro `CancellationToken`
- [ ] Métodos retornam DTOs específicos (nunca entidades de domínio)
- [ ] DTOs seguem nomenclatura `Request`/`Response` (evitar `Query`/`Result` para DTOs)
- [ ] Métodos de listagem recebem `Request` único (com paginação/ordenação/filtros), evitando parâmetros soltos
- [ ] Nenhuma referência a EF Core, DbContext ou ApplicationDbContext
- [ ] Documentação XML nos métodos explicando o propósito
- [ ] Nomenclatura consistente: `Consultar{Algo}Async`, `Listar{Algo}Async`, `Obter{Algo}Async`
- [ ] Interfaces de query service herdam de `IQuery` (marcador para DI)

### Implementação de Query (Infrastructure)

- [ ] Implementação criada em `ProjSub.Infraestrutura/Persistencia/Queries/`
- [ ] Implementação em classe `{Entidade}Queries` **ou** em repositório do agregado (ex.: `ProjetoRepositorio`)
- [ ] Classe de implementação (query ou repositório) implementa `I{Entidade}Queries` e/ou interface específica (ex.: `IListarProjetosQuery`)
- [ ] Injeta `ApplicationDbContext` no construtor
- [ ] Usa `.AsNoTracking()` para consultas read-only
- [ ] Usa `.Select()` para projetar direto para DTOs (evita materializar entidades completas)
- [ ] Usa `SortingExtensions.ApplySorting(...)` quando houver ordenação dinâmica
- [ ] Usa `PaginationExtensions.ToPagedResponseAsync(...)` quando houver paginação
- [ ] Agregações e cálculos executados no banco (não em memória)
- [ ] **Sem efeitos colaterais** (não modifica estado, não chama `SaveChangesAsync`)
- [ ] Queries complexas podem usar Dapper, SQL bruto ou Stored Procedures

### Query Handlers (Application)

- [ ] Handler injeta `I{Entidade}Queries` (nunca `DbContext` ou `ApplicationDbContext`)
- [ ] Handler implementa `IQueryHandler<TRequest, TResponse>` e retorna `Result<TResponse>`
- [ ] Validações básicas no handler (ex: parâmetros obrigatórios)
- [ ] Retorna `Result<T>` usando Result Pattern
- [ ] Sem lógica de acesso a dados (delegado para `I{Entidade}Queries`)
- [ ] Sem transações (queries são read-only)

### Registro no DI

- [ ] Registro automático habilitado: `services.AddScopedFromInterface(typeof(IQuery))`
- [ ] Registrado em `Program.cs` ou `DependencyInjection.cs` da Infrastructure

### Testes Unitários

- [ ] Testes unitários do handler usando mock de `I{Entidade}Queries`
- [ ] Testes cobrem cenários de sucesso (dados encontrados)
- [ ] Testes cobrem cenários de falha (dados não encontrados, validações)
- [ ] Verificação de chamadas aos métodos da interface usando `Verify()`

### Arquitetura

- [ ] Application não referencia Infrastructure ou EF Core
- [ ] Infrastructure referencia Application (implementa interfaces)
- [ ] Dependências corretas: `WebAPI → Application ← Infrastructure`
- [ ] Sem dependência cíclica
- [ ] Queries sempre read-only (sem efeitos colaterais)

### Orientações para GitHub Copilot / Assistentes de IA

Ao gerar código de Query Services, sempre seguir:

✅ **Interfaces na Application**: Criar `I{Entidade}Queries` em `Aplicacao/{Entidade}/` (ex: `Projetos/IProjetoQueries.cs`)  
✅ **Alternativa válida por caso de uso**: Criar interfaces específicas (ex: `IListarProjetosQuery`) em `Aplicacao/{Entidade}/{CasoDeUso}/`  
✅ **Alternativa válida de implementação**: Repositório da raiz de agregação (ex: `ProjetoRepositorio`) pode implementar `IProjetoQueries` ou `IListarProjetosQuery`  
✅ **Requests em records**: Modelar requests de casos de uso com `record` e contrato `IRequest`  
✅ **Paginação e ordenação padronizadas**: Preferir `PaginationRequest`/`ISortableRequest` nos requests  
✅ **Implementações na Infrastructure**: Criar `{Entidade}Queries` em `Infraestrutura/Persistencia/Queries/`  
✅ **Handlers injetam apenas interfaces**: NUNCA injetar `DbContext` ou `ApplicationDbContext`  
✅ **Inversão de Dependência**: Application define contratos, Infrastructure implementa  
✅ **Projeções Otimizadas**: Usar `.AsNoTracking()` + `.Select()` direto para DTOs  
✅ **Extensões de padronização**: Usar `SortingExtensions` e `PaginationExtensions` para reduzir duplicação  
✅ **Sem Efeitos Colaterais**: Queries nunca modificam estado ou persistem dados  
✅ **Result Pattern**: Handlers retornam `Result<T>`  
✅ **Nomenclatura de DTOs**: usar `{Ação}{Entidade}Request` e `{Ação}{Entidade}Response`  
✅ **Nomenclatura de handlers/serviços**: `I{Entidade}Queries`, `{Entidade}Queries`, `{Ação}{Entidade}QueryHandler`  
✅ **Listagens com request único**: preferir `Listar{Entidade}Async({Entidade}Request request, ...)`  
✅ **Testes com Mocks**: Usar `Mock<I{Entidade}Queries>` para testar handlers  
✅ **Queries Complexas**: Permitir uso de Dapper, SQL bruto ou SPs quando necessário

---

_Documento criado em: 27/02/2026_  
_Última atualização: 27/02/2026_
