# DT-021: Controle Transacional Automático para CommandHandlers

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Implementar controle transacional automático em todos os CommandHandlers sem necessidade de código explícito
>
> **Quando usar:** Ao criar novos CommandHandlers ou entender como o controle transacional funciona no projeto
>
> **Palavras-chave:** `transaction` `unit-of-work` `decorator-pattern` `command-handler` `clean-architecture` `ef-core`

---

## Contexto

Os CommandHandlers realizam operações de escrita que podem envolver múltiplas entidades e repositórios. Sem controle transacional adequado, surgem problemas de inconsistência de dados e falta de atomicidade. A solução deve ser automática, sem código explícito nos handlers.

---

## Decisão

Implementar controle transacional automático usando **Decorator Pattern** combinado com **Unit of Work Pattern**:

- **Decorator intercepta** todos os CommandHandlers automaticamente
- **UnitOfWork gerencia** transações (Begin/Commit/Rollback)
- **SaveChangesAsync mantido** nos repositórios para gerar IDs imediatamente

### Componentes da Solução

### Componentes da Solução

#### 1. IUnitOfWork (Domínio)

**Arquivo:** `ProjSub.Dominio/Base/IUnitOfWork.cs`

Interface que define o contrato agnóstico de infraestrutura para controle transacional:

```csharp
public interface IUnitOfWork : IDisposable
{
    bool HasActiveTransaction { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

#### 2. UnitOfWork (Infraestrutura)

**Arquivo:** `ProjSub.Infraestrutura/Persistencia/UnitOfWork.cs`

Implementação concreta usando EF Core + PostgreSQL com logging estruturado e tratamento de erros.

#### 3. TransactionCommandHandlerDecorator (Aplicação)

**Arquivo:** `ProjSub.Aplicacao/Base/TransactionCommandHandlerDecorator.cs`

Decorator que intercepta automaticamente todos CommandHandlers e gerencia transações:

**Fluxo:**

1. Verifica se já existe transação ativa (suporta composição)
2. Inicia nova transação se necessário
3. Executa o handler real
4. Commit se `result.IsSuccess == true`
5. Rollback se `result.IsSuccess == false` ou exceção

#### 4. Registro no DI

**Infraestrutura:**

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Aplicação (usando Scrutor):**

```csharp
services.Decorate(typeof(ICommandHandler<,>), typeof(TransactionCommandHandlerDecorator<,>));
services.Decorate(typeof(ICommandHandler<>), typeof(TransactionCommandHandlerDecorator<>));
```

---

## Uso no Dia a Dia

### Padrão (Automático - Recomendado)

**Não injete `IUnitOfWork` no handler.** O controle transacional é automático via decorator:

```csharp
public class CriarSubprojetoCommandHandler
    : ICommandHandler<CriarSubprojetoRequest, CriarSubprojetoResponse>
{
    private readonly ISubprojetoRepository _repository;
    // ❌ Não injetar IUnitOfWork aqui

    public async Task<Result<CriarSubprojetoResponse>> ExecuteAsync(
        CriarSubprojetoRequest request, CancellationToken ct)
    {
        // Validações...

        var subprojeto = Subprojeto.Criar(request.Descricao, request.ProjetoId);

        // Repository salva e gera ID (dentro da transação automática)
        await _repository.AddAsync(subprojeto, ct);

        // Decorator faz Commit automaticamente se IsSuccess
        return Result<CriarSubprojetoResponse>.Success(
            new CriarSubprojetoResponse(subprojeto.Id));
    }
}
```

✅ **Transação automática:**

- BeginTransaction antes do handler
- Commit se retornar `Result.Success`
- Rollback se retornar `Result.Failure` ou exceção

---

### Manipulação Manual (Casos Especiais)

**Injete `IUnitOfWork` apenas quando precisar de controle explícito:**

```csharp
public class ProcessarLoteCommandHandler
    : ICommandHandler<ProcessarLoteRequest, ProcessarLoteResponse>
{
    private readonly ISubprojetoRepository _repository;
    private readonly IUnitOfWork _unitOfWork; // ✅ Injeção explícita

    public async Task<Result<ProcessarLoteResponse>> ExecuteAsync(
        ProcessarLoteRequest request, CancellationToken ct)
    {
        // Decorator já iniciou transação, mas você pode manipular:

        foreach (var item in request.Items)
        {
            var subprojeto = await _repository.GetByIdAsync(item.Id, ct);
            subprojeto.Atualizar(item.Descricao);
            await _repository.UpdateAsync(subprojeto, ct);

            // SaveChanges intermediário se necessário (dentro da mesma transação)
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // Decorator ainda fará Commit/Rollback final baseado no Result
        return Result<ProcessarLoteResponse>.Success(new ProcessarLoteResponse());
    }
}
```

**Quando usar injeção manual:**

- ⚠️ Processamento em lote com checkpoints intermediários
- ⚠️ Necessidade de múltiplos `SaveChanges` para flush de memória
- ⚠️ Controle fino sobre o momento do SaveChanges

**Para 95% dos casos: não injete `IUnitOfWork`, deixe o decorator gerenciar automaticamente.**

---

## Decisões Importantes

### Por que manter SaveChangesAsync nos Repositórios?

**Decisão:** Manter `await _context.SaveChangesAsync()` nas operações de escrita dos repositórios.

**Justificativa:**

- IDs gerados pelo PostgreSQL (sequences) só ficam disponíveis após `SaveChanges`
- Handlers podem precisar do ID imediatamente para operações subsequentes
- Múltiplos `SaveChanges` dentro de uma transação são seguros no EF Core

**Exemplo:**

```csharp
// 1. Repository.AddAsync salva e retorna entidade com ID
var subprojeto = await _subprojetoRepository.AddAsync(entity, ct);

// 2. ID gerado pode ser usado imediatamente
var marco = new Marco(subprojeto.Id, "Marco Inicial");
await _marcoRepository.AddAsync(marco, ct);

// 3. Decorator garante Commit ou Rollback ao final
```

---

## Consequências

### Positivas

✅ **Zero código transacional** nos handlers (automático)  
✅ **Atomicidade garantida** (tudo ou nada)  
✅ **Rollback automático** em falhas  
✅ **IDs gerados** disponíveis imediatamente  
✅ **Clean Architecture** (IUnitOfWork no Domínio)  
✅ **Logging estruturado** de todas transações

### Negativas

❌ Overhead mínimo do decorator  
⚠️ Desenvolvedores devem entender quando NOT injetar IUnitOfWork

---

## Exemplos

## Exemplos

### Handler com Múltiplas Operações

```csharp
public class CriarProjetoComMarcoCommandHandler
    : ICommandHandler<CriarProjetoComMarcoRequest, CriarProjetoResponse>
{
    private readonly IProjetoRepository _projetoRepo;
    private readonly IMarcoRepository _marcoRepo;

    public async Task<Result<CriarProjetoResponse>> ExecuteAsync(
        CriarProjetoComMarcoRequest request, CancellationToken ct)
    {
        var projeto = Projeto.Criar(request.Nome);
        var projetoSalvo = await _projetoRepo.AddAsync(projeto, ct);

        // ID gerado está disponível imediatamente
        var marco = Marco.Criar(projetoSalvo.Id, "Marco Inicial");
        await _marcoRepo.AddAsync(marco, ct);

        // Se ambos sucesso → Decorator faz Commit
        // Se algum falhar → Decorator faz Rollback
        return Result<CriarProjetoResponse>.Success(
            new CriarProjetoResponse(projetoSalvo.Id));
    }
}
```

### Handler com Falha de Validação (Rollback Automático)

```csharp
public async Task<Result<ExcluirSubprojetoResponse>> ExecuteAsync(
    ExcluirSubprojetoRequest request, CancellationToken ct)
{
    var subprojeto = await _repository.GetByIdAsync(request.Id, ct);

    if (subprojeto == null)
    {
        // Result.Failure → Decorator faz Rollback automático
        return Result<ExcluirSubprojetoResponse>.Failure("Não encontrado");
    }

    if (subprojeto.TemMarcos())
    {
        // Validação de negócio falhou → Rollback automático
        return Result<ExcluirSubprojetoResponse>.Failure(
            "Não é possível excluir subprojeto com marcos");
    }

    await _repository.RemoveAsync(subprojeto, ct);

    return Result<ExcluirSubprojetoResponse>.Success(
        new ExcluirSubprojetoResponse());
}
```

---

## Verificação de Conformidade

### Checklist de Implementação

- [x] IUnitOfWork definido na camada de Domínio
- [x] UnitOfWork implementado na camada de Infraestrutura
- [x] TransactionCommandHandlerDecorator criado na camada de Aplicação
- [x] Decorators registrados no DI usando Scrutor.Decorate
- [x] IUnitOfWork registrado como Scoped no DI
- [x] SaveChangesAsync mantido nos repositórios

### Checklist de Validação

- [ ] Handlers existentes funcionam sem modificação
- [ ] Logs de transação aparecem no console/CloudWatch
- [ ] Rollback funciona em caso de Result.Failure
- [ ] Rollback funciona em caso de exceção
- [ ] IDs gerados são acessíveis imediatamente após AddAsync

---

## Referências

- [DT-016: Padrões Command e Query na Camada de Aplicação](DT-016-padroes-camada-aplicacao.md)
- [DT-018: Padrão de Implementação de Repositórios](DT-018-padrao-implementacao-repositorios.md)
- [DT-020: Implementação de Commands na Camada de Aplicação](DT-020-cqrs-commands-camada-aplicacao.md)
- [Microsoft: Transactions in EF Core](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- [Martin Fowler: Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)

---

## Histórico de Alterações

| Data       | Versão | Autor          | Descrição                    |
| ---------- | ------ | -------------- | ---------------------------- |
| 2026-03-03 | 1.0    | GitHub Copilot | Criação inicial do documento |
