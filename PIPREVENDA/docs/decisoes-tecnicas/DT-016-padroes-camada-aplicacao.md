# DT-016: Padrões Command e Query na Camada de Aplicação

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Fornecer visão geral dos conceitos e da organização de Commands e Queries na camada de Aplicação
>
> **Quando usar:** Ao entender o modelo conceitual da camada de Aplicação e navegar para os documentos de implementação detalhada
>
> **Palavras-chave:** `cqrs` `command-pattern` `query-pattern` `result-pattern` `clean-architecture` `camada-aplicacao`

## Contexto

A camada de Aplicação orquestra casos de uso entre apresentação, domínio e infraestrutura. Sem padrões claros, surgem inconsistências de nomenclatura, acoplamento indevido com infraestrutura e divergências de implementação entre escrita e leitura.

Os princípios adotados para reduzir esse risco são:

- **CQRS**: separação explícita entre operações de escrita (Commands) e leitura (Queries)
- **Single Responsibility**: um handler por caso de uso
- **Result Pattern**: retorno padronizado de sucesso/falha sem usar exceções para fluxo de negócio
- **Inversão de Dependência**: contratos definidos na Aplicação e implementados na Infraestrutura

---

## Decisão

Adotar o padrão CQRS na camada de Aplicação com regras de implementação consistentes para Commands e Queries.

Este documento define a visão consolidada e os princípios transversais. As regras detalhadas, os padrões normativos e os exemplos completos de implementação ficam centralizados em:

- **Commands (CQRS):** [DT-020: Implementação de Commands na Camada de Aplicação](DT-020-cqrs-commands-camada-aplicacao.md)
- **Queries (CQRS):** [DT-019: Implementação de Query Services na Camada de Aplicação](DT-019-cqrs-query-camada-aplicacao.md)

Essa centralização evita duplicação de conteúdo e reduz o esforço de manutenção da documentação.

### 1. Contrato base de Request

1. Requests de casos de uso são definidos como `record`.
2. O contrato base de request é `IRequest` / `IRequest<TResult>`.
3. `ICommand` não é usado como contrato base de request.
4. `IQuery` permanece como interface marcadora para contratos de consulta (`I{Entidade}Queries`) registrados por varredura no DI da infraestrutura.

### 2. Commands (escrita)

- Modificam estado da aplicação.
- Nomenclatura padrão dos contratos de entrada/saída: `{Ação}{Entidade}Request` e `{Ação}{Entidade}Response`.
- Handler de escrita mantém sufixo `CommandHandler`.
- Handler implementa `ICommandHandler<TRequest, TResponse>` (ou `ICommandHandler<TRequest>` quando não há payload de retorno).
- Handler retorna `Task<Result<TResponse>>` (ou `Task<Result>`).
- Persistência ocorre por repositórios; `SaveChangesAsync` é responsabilidade da infraestrutura.

> **Referência detalhada:** [DT-020: Implementação de Commands na Camada de Aplicação](DT-020-cqrs-commands-camada-aplicacao.md)

### 3. Queries (leitura)

- Não modificam estado.
- Requests seguem padrão `Request` e respostas seguem padrão `Response`.
- Handler implementa `IQueryHandler<TRequest, TResponse>` e retorna `Task<Result<TResponse>>`.
- Queries de leitura usam interface de consulta da aplicação (`I{Entidade}Queries` ou interface por caso de uso, como `IListarProjetosQuery`) com implementação na infraestrutura.
- Em consultas paginadas/ordenadas, usar `PaginationRequest`, `IPaginationRequest`, `ISortableRequest` e `PaginationResponse<T>`.

> **Referência detalhada:** [DT-019: Implementação de Query Services na Camada de Aplicação](DT-019-cqrs-query-camada-aplicacao.md)

### 4. Result Pattern

- Toda operação retorna `Result<T>` ou `Result`.
- O padrão encapsula sucesso/falha e mensagens de validação.
- Exceções são reservadas para cenários excepcionais (não para fluxo de validação de negócio).

---

## Estrutura de Organização

A organização principal permanece por agregado/entidade e ação:

```text
PortalAle.Application/
├── Base/
│   ├── IRequest.cs
│   ├── ICommandHandler.cs
│   ├── IQueryHandler.cs
│   ├── Result.cs
│   └── ...
│
├── Projetos/
│   ├── IProjetoQueries.cs
│   ├── Criar/
│   │   ├── CriarProjetoRequest.cs
│   │   ├── CriarProjetoCommandHandler.cs
│   │   └── CriarProjetoResponse.cs
│   ├── Consultar/
│   │   ├── ConsultarProjetoRequest.cs
│   │   ├── ConsultarProjetoQueryHandler.cs
│   │   └── ConsultarProjetoResponse.cs
│   └── Listar/
│       ├── ListarProjetosRequest.cs
│       ├── ListarProjetosQueryHandler.cs
│       └── ListarProjetosResponse.cs
│
└── DependencyInjection.cs
```

---

## Escopo do Documento

Este DT-016 é intencionalmente um **overview** e não deve concentrar regras detalhadas de implementação.

### O que este documento cobre

- visão conceitual de CQRS na camada de Aplicação;
- responsabilidades de Commands e Queries;
- organização estrutural e contratos-base;
- direcionamento para os documentos normativos de implementação.

### O que este documento não cobre

- padrões detalhados de implementação de commands;
- padrões detalhados de implementação de queries;
- exemplos completos de código de handlers, endpoints e testes.

### Referências obrigatórias para implementação

- **Commands:** [DT-020: Implementação de Commands na Camada de Aplicação](DT-020-cqrs-commands-camada-aplicacao.md)
- **Queries:** [DT-019: Implementação de Query Services na Camada de Aplicação](DT-019-cqrs-query-camada-aplicacao.md)

---

## Visão de Fluxo (Conceitual)

```text
WebAPI -> Handler da Aplicação -> Domínio/Repos/Serviços -> Result
```

- Escrita: `Request` + `CommandHandler` + `Response`.
- Leitura: `Request` + `QueryHandler` + `Response`.
- Contrato base de request: `IRequest`.

---

## Consequências

### Positivas

✅ Separação clara entre leitura e escrita  
✅ Consistência de nomenclatura (`Request/Response`, `CommandHandler`)  
✅ Redução de acoplamento entre Aplicação e Infraestrutura  
✅ Melhor testabilidade e previsibilidade dos casos de uso

### Negativas

❌ Maior volume de artefatos por caso de uso (request, handler, response)  
❌ Necessidade de disciplina para manter padrões de naming e contratos

---

## Referências

- [DT-019: Implementação de Query Services na Camada de Aplicação](DT-019-cqrs-query-camada-aplicacao.md)
- [DT-020: Implementação de Commands na Camada de Aplicação](DT-020-cqrs-commands-camada-aplicacao.md)
- [DT-013: Padrão de Tratamento de Erros (ProblemDetails)](DT-013-padrao-tratamento-erros-problemdetails.md)
- [DT-018: Padrão de Implementação de Repositórios](DT-018-padrao-implementacao-repositorios.md)
- [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)
- [Microsoft: CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)

---

## Verificação de Conformidade

### Commands

- [ ] Request de command definido como `record` e contrato `IRequest<Result<TResponse>>`
- [ ] Handler `{Ação}{Entidade}CommandHandler` implementa `ICommandHandler<TRequest, TResponse>` (ou `ICommandHandler<TRequest>`)
- [ ] Retorno padronizado com `Result<TResponse>` (ou `Result`)
- [ ] Persistência via repositórios
- [ ] `SaveChangesAsync` não é chamado diretamente na camada de Aplicação

### Queries

- [ ] Request de query definido como `record` e contrato `IRequest<Result<TResponse>>`
- [ ] Handler `{Ação}{Entidade}QueryHandler` implementa `IQueryHandler<TRequest, TResponse>`
- [ ] Response nomeado com sufixo `Response`
- [ ] Interface de query definida na Aplicação (`I{Entidade}Queries` ou interface por caso de uso)
- [ ] Implementação de query na Infraestrutura com `.AsNoTracking()`
- [ ] Queries paginadas usam `PaginationRequest`/`PaginationResponse<T>`

### Apresentação (Minimal API)

- [ ] Endpoints de leitura usam `[FromQuery]` para requests de consulta/listagem
- [ ] Endpoints de escrita usam `[FromBody]` para requests de escrita
- [ ] Handlers injetados diretamente no endpoint
- [ ] Resposta HTTP baseada em `Result` (`Ok`, `Created`, `BadRequest`, `NotFound` quando aplicável)

### Diretrizes para Assistentes de IA

✅ Commands e Queries devem permanecer separados  
✅ Contrato base de request deve ser `IRequest`  
✅ Nomenclatura de DTOs de entrada/saída deve usar `Request/Response`  
✅ Implementações devem seguir DT-019 (queries) e DT-020 (commands)
