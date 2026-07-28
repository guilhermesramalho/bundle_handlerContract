# DT-018: Padrão de Implementação de Repositórios de Entidades

<!-- Metadados para descoberta por IA/GitHub Copilot -->

> **Componente:** Backend  
> **Tipo:** Decisão Técnica  
> **Propósito:** Padronizar implementação de repositórios para Aggregate Roots seguindo DDD e CQRS  
> **Quando usar:** Implementação de Commands para manipulação de entidades de domínio, criação de repositórios, decisão entre interface genérica ou específica  
> **Palavras-chave:** repositório, repository-pattern, aggregate-root, domain-driven-design, cqrs, entity-framework-core

## Contexto

No desenvolvimento do backend do Projeto +Digital (A11732), seguimos Clean Architecture e Domain-Driven Design (DDD), onde entidades de domínio são a base da modelagem de negócio. Para persistir essas entidades, precisamos de um padrão consistente que:

1. **Abstraia a camada de persistência** do domínio (inversão de dependência)
2. **Respeite o conceito de Aggregate Root** do DDD (apenas raízes de agregação têm repositórios)
3. **Aplique CQRS corretamente** (repositórios apenas em Commands, não em Queries)
4. **Minimize duplicação de código** através de classes base reutilizáveis
5. **Facilite testes unitários** através de interfaces mockáveis
6. **Garanta registro automático** no container de injeção de dependências

O problema central é: **como padronizar a criação e uso de repositórios de forma que desenvolvedores saibam quando criar interfaces específicas, quando implementar classes concretas, e como garantir conformidade com DDD e CQRS?**

Além disso, existe confusão comum sobre o uso de Repository em Queries, o que viola o princípio CQRS e prejudica performance (Queries precisam de projeções otimizadas, enquanto Repository retorna entidades completas).

## Decisão

**Adotamos o padrão Repository com as seguintes regras arquiteturais obrigatórias:**

### 1. Repository APENAS para Aggregate Roots

- ✅ **Criar repositórios** somente para entidades que são **raízes de agregação** (Aggregate Roots)
- ❌ **Não criar repositórios** para entidades filhas (acessadas através da raiz)
- **Definição de Aggregate Root**: Entidade que controla o acesso às entidades internas da agregação e garante consistência de invariantes de negócio

**Exemplos:**

- ✅ `Projeto` (raiz) → possui `IProjetoRepository`
- ✅ `Pedido` (raiz) → possui `IPedidoRepository`
- ❌ `ItemPedido` (filho de Pedido) → sem repositório próprio

### 2. Repository APENAS em Commands (CQRS)

- ✅ **Usar Repository em Commands** (Create, Update, Delete) para manipulação de entidades de domínio
- ❌ **NÃO usar Repository em Queries** (Read) - usar `ApplicationDbContext` diretamente
- **Justificativa**: Repository do DDD foca em manipulação de agregados; Queries precisam de projeções otimizadas

**Exemplos:**

```csharp
// ✅ CORRETO - Command usa Repository
public class CriarProjetoCommandHandler
{
    private readonly IProjetoRepository _repository;

    public async Task<int> Handle(CriarProjetoCommand request, CancellationToken ct)
    {
        var projeto = new Projeto(request.Nome, request.Descricao);
        return await _repository.AdicionarAsync(projeto, ct);
    }
}

// ✅ CORRETO - Query usa DbContext direto
public class ListarProjetosQueryHandler
{
    private readonly ApplicationDbContext _context;

    public async Task<List<ProjetoDto>> Handle(ListarProjetosQuery request, CancellationToken ct)
    {
        return await _context.Projetos
            .Where(p => p.Ativo)
            .Select(p => new ProjetoDto { Id = p.Id, Nome = p.Nome }) // Projeção otimizada
            .ToListAsync(ct);
    }
}
```

### 3. Estrutura de Implementação

**Interfaces de Repositório (Camada de Domínio):**

- **Localização:** `ProjSub.Dominio/Repositorios/I[NomeEntidade]Repository.cs`
- **Herança obrigatória:** Todas as interfaces herdam de `IRepository<T>`
- **Métodos:** Apenas métodos específicos além dos básicos (filtros complexos, includes especiais)

**Implementações Concretas (Camada de Infraestrutura):**

- **Localização:** `ProjSub.Infraestrutura/Persistencia/Repositorios/[NomeEntidade]Repository.cs`
- **Herança obrigatória:** Classes herdam de `Repository<T>` e implementam `I[NomeEntidade]Repository`
- **Criação condicional:** Apenas criar classe concreta se houver métodos específicos

### 4. Classe Base Genérica `Repository<T>`

Reutilizar a classe base `Repository<T>` que fornece métodos básicos:

- `ObterPorIdAsync(int id)` - Obtém entidade por ID
- `ListarTodosAsync()` - Lista todas as entidades
- `AdicionarAsync(T entidade)` - Adiciona nova entidade
- `AtualizarAsync(T entidade)` - Atualiza entidade existente
- `RemoverAsync(int id)` - Remove entidade por ID
- `ExisteAsync(int id)` - Verifica existência por ID

### 5. Métodos Específicos

Adicionar métodos específicos apenas quando necessário:

- Filtros complexos (ex: `ListarProjetosAtivosPorUsuarioAsync`)
- Includes especiais (ex: `ObterProjetoCompletoAsync` com subentidades)
- Validações de negócio (ex: `ExisteProjetoAtivoComNomeAsync`)

**❌ Não duplicar** métodos básicos já existentes em `IRepository<T>`

### 6. Registro Automático de Dependências

- **Registro automático** via `AddScopedFromInterface(typeof(IRepository<>))` em `DependencyInjection.cs`
- **Não registrar manualmente** repositórios individuais
- Sistema detecta automaticamente classes que implementam `IRepository<T>`

### 7. Nomenclatura Padronizada

- **Interface:** `I[NomeEntidade]Repository` (exemplo: `IProjetoRepository`)
- **Classe:** `[NomeEntidade]Repository` (exemplo: `ProjetoRepository`)
- **Métodos:** Verbos descritivos + Async (exemplo: `ListarProjetosAtivosAsync`)

## Alternativas Consideradas

### Alternativa 1: DbContext Diretamente em Commands

**Descrição:** Injetar `ApplicationDbContext` diretamente nos Command Handlers sem usar repositories.

**Rejeitado porque:**

- Viola inversão de dependência (Domain depende de Infrastructure)
- Dificulta testes unitários (DbContext é difícil de mockar)
- Quebra abstração de persistência (acoplamento com EF Core)

### Alternativa 2: Repository em Queries (CQRS)

**Descrição:** Usar Repository tanto em Commands quanto em Queries.

**Rejeitado porque:**

- Repository retorna entidades completas (ineficiente para Queries)
- Queries precisam de projeções otimizadas (`Select(p => new ProjetoDto { ... })`)
- Prejudica performance ao carregar dados desnecessários
- Viola princípio de separação de responsabilidades do CQRS

## Consequências

### Positivas

1. **Separação clara de responsabilidades**: Domínio não depende de infraestrutura
2. **Testabilidade**: Interfaces mockáveis facilitam testes unitários
3. **Reutilização de código**: Métodos básicos herdados de `Repository<T>`
4. **Conformidade com CQRS**: Commands usam Repository, Queries usam DbContext
5. **Performance otimizada**: Queries com projeções evitam carga desnecessária
6. **Manutenibilidade**: Padrão consistente facilita onboarding de novos desenvolvedores
7. **Registro automático**: Menos configuração manual de DI

### Negativas

1. **Boilerplate inicial**: Necessário criar interface e classe para cada Aggregate Root
2. **Curva de aprendizado**: Desenvolvedores precisam entender DDD e CQRS
3. **Decisão de design**: Requer análise para identificar Aggregate Roots corretamente
4. **Duplicação condicional**: Alguns métodos podem parecer redundantes se mal planejados

### Riscos Mitigados

- **Risco de acoplamento com ApplicationDbContext**: Aceito por trazer simplicidade e eficiência nas consultas, podendo ser também mitigado pela abstração de interfaces. Também aceito considerando a baixa complexidade do projeto, o que faz com que o uso obrigatório dos padrões do DDD (ex: Repository) tragam maior verbosidade para a implementação
- **Risco de performance**: Mitigado pela separação CQRS (Queries otimizadas)
- **Risco de inconsistência**: Mitigado pelo padrão obrigatório de Aggregate Roots

## Implementação

### Passo 1: Identificar Aggregate Root

Antes de criar um repositório, validar se a entidade é uma raiz de agregação:

- [ ] Entidade possui identidade própria e ciclo de vida independente?
- [ ] Entidade controla o acesso a entidades filhas?
- [ ] Entidade garante invariantes de negócio da agregação?

**Exemplos de Aggregate Roots no ProjSub:**

- ✅ `Projeto` (raiz) → contém `Subprojetos`, `Marcos` (entidades filhas)
- ✅ `Configuracao` (raiz) → contém `TipoConfiguracao` (entidade relacionada)
- ✅ `Usuario` (raiz) → contém `Permissoes` (objetos de valor)

**Não são Aggregate Roots:**

- ❌ `ItemDePedido` (filho de Pedido) → acessado através de `Pedido`
- ❌ `ItemDeConfiguracao` (filho de Configuracao) → acessado através de `Configuracao`

### Passo 2: Criar Interface de Repositório

**Localização:** `ProjSub.Dominio/Repositorios/I[NomeEntidade]Repository.cs`

**Caso 1: Entidade usa APENAS métodos básicos**

```csharp
namespace ProjSub.Dominio.Repositorios;

/// <summary>
/// Repositório para TipoConfiguracao (usa apenas métodos básicos).
/// </summary>
public interface ITipoConfiguracaoRepository : IRepository<TipoConfiguracao>
{
    // Vazia - herda métodos básicos de IRepository<T>
}
```

**Caso 2: Entidade possui métodos específicos**

```csharp
namespace ProjSub.Dominio.Repositorios;

/// <summary>
/// Repositório para Projeto com métodos específicos.
/// </summary>
public interface IProjetoRepository : IRepository<Projeto>
{
    /// <summary>
    /// Lista projetos ativos de um usuário específico.
    /// </summary>
    Task<List<Projeto>> ListarProjetosAtivosPorUsuarioAsync(
        string usuarioLogin,
        CancellationToken ct = default);

    /// <summary>
    /// Obtém projeto com subprojetos e marcos carregados (Include).
    /// </summary>
    Task<Projeto?> ObterProjetoCompletoAsync(
        int id,
        CancellationToken ct = default);
}
```

**Regras:**

- ✅ Herdar sempre de `IRepository<T>`
- ✅ Adicionar apenas métodos específicos (não duplicar métodos básicos)
- ✅ Adicionar xml comment em todos os métidos customizados
- ✅ Usar `CancellationToken` em métodos assíncronos

### Passo 3: Implementar Repositório Concreto (Se Necessário)

**Localização:** `ProjSub.Infraestrutura/Persistencia/Repositorios/[NomeEntidade]Repository.cs`

**⚠️ Importante:** Apenas criar classe concreta se a entidade **possui métodos específicos**.

**Quando NÃO criar classe concreta:**

- Se a entidade usa apenas métodos básicos de `IRepository<T>`
- Nesse caso, injetar `IRepository<Entidade>` diretamente nos handlers

**Quando criar classe concreta:**

- Se a entidade possui métodos específicos (filtros complexos, includes especiais)

**Exemplo de implementação:**

```csharp
using Microsoft.EntityFrameworkCore;
using ProjSub.Dominio;
using ProjSub.Dominio.Repositorios;

namespace ProjSub.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// Repositório para Projeto com métodos específicos.
/// </summary>
public class ProjetoRepository : Repository<Projeto>, IProjetoRepository
{
    public ProjetoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Projeto>> ListarProjetosAtivosPorUsuarioAsync(
        string usuarioLogin,
        CancellationToken ct = default)
    {
        return await _context.Projetos
            .Where(p => p.UsuarioResponsavelLogin == usuarioLogin && p.Ativo)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync(ct);
    }

    public async Task<Projeto?> ObterProjetoCompletoAsync(
        int id,
        CancellationToken ct = default)
    {
        return await _context.Projetos
            .Include(p => p.Subprojetos)
            .Include(p => p.Marcos)
            .Include(p => p.Status)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}
```

**Regras:**

- ✅ Herdar de `Repository<T>` para reutilizar métodos básicos
- ✅ Implementar `I[NomeEntidade]Repository`
- ✅ Construtor recebe `ApplicationDbContext` e passa para base
- ✅ Usar `_context` (herdado da classe base) para acessar DbSet
- ✅ Implementar apenas métodos específicos declarados na interface

### Passo 4: Registro Automático (Não Requere Ação)

O sistema já possui registro automático de repositórios em `ProjSub.Infraestrutura/DependencyInjection.cs`:

```csharp
// Registro automático de repositórios
services.AddScopedFromInterface(typeof(IRepository<>));
```

**Como funciona:**

1. Sistema detecta classes que implementam `IRepository<T>`
2. Registra automaticamente tanto `IRepository<T>` quanto `I[Nome]Repository`
3. Permite injeção de qualquer uma das interfaces nos handlers

**Não é necessário:**

- ❌ Registrar manualmente cada repositório
- ❌ Adicionar linha no `DependencyInjection.cs` para novos repositórios

### Passo 5: Usar Repositório em Commands (CQRS)

**Opção 1: Injetar interface específica (quando há métodos customizados)**

```csharp
public class AtualizarProjetoCommandHandler
{
    private readonly IProjetoRepository _repository;

    public AtualizarProjetoCommandHandler(IProjetoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(AtualizarProjetoCommand request, CancellationToken ct)
    {
        // Usa método específico do repositório
        var projeto = await _repository.ObterProjetoCompletoAsync(request.Id, ct);

        projeto.AtualizarDados(request.Nome, request.Descricao);
        await _repository.AtualizarAsync(projeto, ct);
    }
}
```

**Opção 2: Injetar interface genérica (quando usa apenas métodos básicos)**

```csharp
public class CriarConfiguracaoCommandHandler
{
    private readonly IRepository<Configuracao> _repository;

    public CriarConfiguracaoCommandHandler(IRepository<Configuracao> repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CriarConfiguracaoCommand request, CancellationToken ct)
    {
        var config = new Configuracao { Nome = request.Nome };
        return await _repository.AdicionarAsync(config, ct); // Método básico
    }
}
```

### Passo 6: NÃO Usar Repositório em Queries

**❌ ERRADO - Usar Repository em Query:**

```csharp
// ❌ NÃO FAÇA ISSO
public class ListarProjetosQueryHandler
{
    private readonly IProjetoRepository _repository;

    public async Task<List<ProjetoDto>> Handle(ListarProjetosQuery request)
    {
        var projetos = await _repository.ListarTodosAsync(); // Carrega tudo
        return projetos.Select(p => new ProjetoDto { Id = p.Id, Nome = p.Nome }).ToList();
    }
}
```

**✅ CORRETO - Usar DbContext em Query:**

```csharp
// ✅ FAÇA ISSO
public class ListarProjetosQueryHandler
{
    private readonly ApplicationDbContext _context;

    public ListarProjetosQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjetoDto>> Handle(ListarProjetosQuery request, CancellationToken ct)
    {
        return await _context.Projetos
            .Where(p => p.Ativo)
            .Select(p => new ProjetoDto { Id = p.Id, Nome = p.Nome }) // Projeção otimizada
            .ToListAsync(ct);
    }
}
```

### Métodos Básicos Disponíveis em `IRepository<T>`

Todos os repositórios herdam automaticamente:

```csharp
// Obter entidade por ID
Task<T?> ObterPorIdAsync(int id, CancellationToken ct = default);

// Listar todas as entidades
Task<List<T>> ListarTodosAsync(CancellationToken ct = default);

// Adicionar nova entidade (retorna ID gerado)
Task<int> AdicionarAsync(T entidade, CancellationToken ct = default);

// Atualizar entidade existente
Task AtualizarAsync(T entidade, CancellationToken ct = default);

// Remover entidade por ID
Task RemoverAsync(int id, CancellationToken ct = default);

// Verificar se entidade existe
Task<bool> ExisteAsync(int id, CancellationToken ct = default);
```

## Verificação de Conformidade

### Checklist de Implementação

- [ ] **Aggregate Root identificado:** Entidade é raiz de agregação (não é entidade filha)
- [ ] **Interface criada:** `I[NomeEntidade]Repository` em `ProjSub.Dominio/Repositorios/`
- [ ] **Interface herda IRepository<T>:** Todas as interfaces herdam de `IRepository<T>`
- [ ] **Métodos específicos documentados:** XML comments em todos os métodos customizados
- [ ] **Classe concreta criada (se necessário):** Apenas se houver métodos específicos
- [ ] **Classe herda Repository<T>:** Implementações concretas herdam de `Repository<T>`
- [ ] **Construtor correto:** Recebe `ApplicationDbContext` e passa para base
- [ ] **Sem duplicação de métodos básicos:** Não reimplementar métodos de `IRepository<T>`
- [ ] **Repository usado apenas em Commands:** Não usado em Queries (usar DbContext)
- [ ] **Registro automático funcionando:** Repositório injetável sem configuração manual

### Checklist de Conformidade Arquitetural

- [ ] **CQRS respeitado:** Commands usam Repository, Queries usam DbContext
- [ ] **DDD respeitado:** Apenas Aggregate Roots possuem repositórios
- [ ] **Clean Architecture respeitada:** Interface em Domain, implementação em Infrastructure
- [ ] **Nomenclatura padronizada:** Interface `I[Nome]Repository`, classe `[Nome]Repository`
- [ ] **CancellationToken em métodos async:** Todos os métodos assíncronos aceitam `CancellationToken`
- [ ] **Performance otimizada:** Queries usam projeções (`Select`) ao invés de Repository

### Checklist de Qualidade de Código

- [ ] **Testes unitários:** Métodos específicos possuem testes unitários
- [ ] **XML comments:** Todos os métodos públicos documentados
- [ ] **Sem lógica de negócio:** Repositório não contém regras de negócio (apenas persistência)
- [ ] **Includes explícitos:** Métodos que carregam relacionamentos usam `.Include()` explicitamente

## Relacionamentos

### Decisões Técnicas Relacionadas

- [DT-004: Mapeamento Objeto-Relacional EF Core](DT-004-mapeamento-objeto-relacional-ef-core.md) - Configuração de entidades no DbContext
- [DT-016: Padrões Command/Query na Camada de Aplicação](DT-016-padroes-camada-aplicacao.md) - Uso de CQRS

### Guias Relacionados

- [GT-002: Geração de Script de Migrations EF Core](../guias/GT-002-geracao-script-migrations-ef-deploybd.md) - Criar migrations após mapear entidades
- [GT-005: Desenvolvimento de Funcionalidade Backend - Passo a Passo](../guias/GT-005-desenvolvimento-funcionalidade-backend.md) - Processo completo incluindo criação de repositórios

### Documentação Externa

- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/reference/) - Conceitos de DDD e Aggregate Roots
- [Aggregate Pattern - Martin Fowler](https://martinfowler.com/bliki/DDD_Aggregate.html) - Explicação de Agregações em DDD
- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html) - Padrão Repository explicado
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/) - Documentação oficial do EF Core
