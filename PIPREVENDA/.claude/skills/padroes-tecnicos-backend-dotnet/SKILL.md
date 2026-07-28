---
name: padroes-tecnicos-backend-dotnet
description: Padrões técnicos OBRIGATÓRIOS para qualquer implementação no backend .NET do PIPREVENDA-1680 (entidades de domínio, commands, queries, repositórios, endpoints REST, EF Core, logs, exceções, testes, integrações externas). Use esta skill SEMPRE que for criar ou alterar código no backend — antes de escrever qualquer Handler, Entity, Repository, Endpoint ou teste. Não implemente por convenção própria ou "melhores práticas" genéricas: siga as Decisões Técnicas (DT) referenciadas aqui.
---

# Padrões técnicos — Backend .NET (PIPREVENDA-1680)

## ⚠️ Aviso de adaptação de stack (leia antes de aplicar qualquer DT)

As Decisões Técnicas (DT) indexadas abaixo foram originalmente escritas para outro
projeto (nomenclatura interna "ProjSub" / "+Digital", padrão corporativo Petrobras
PE-2TIC-00319). O **padrão arquitetural, os nomes de camadas, o CQRS, o Result
Pattern, o Decorator de transação e as convenções de código são para adotar
tal como estão** — é isso que este bundle formaliza como padrão do PIPREVENDA-1680.

Mas alguns detalhes de stack **citados dentro dos DTs não se aplicam literalmente**
ao PIPREVENDA-1680 e devem ser mentalmente substituídos pelo agente:

| No DT está escrito | Neste projeto (PIPREVENDA-1680) é |
|---|---|
| PostgreSQL (DT-003, DT-004) | **SQL Server** — usar a mesma lógica de convenção de nomenclatura do DT-003, mas validando tipos/sintaxe contra SQL Server, não Postgres |
| .NET 9 (DT-005, DT-006) | **.NET 10** |
| `ProjSub.*` como prefixo de projeto/namespace | `PortalAle.*` (ou o nome real da solution quando criada) |
| SonarQube `sonar.petrobras.com.br` | instância de SonarQube deste projeto, quando definida |
| Integrações citadas como exemplo (CAv4, AIDA, Força Trabalho) no DT-011 | nossas integrações reais: **SAP, PCR, Elaw, ANP, Data Lake** (ver skills `integracao-sap` e `integracao-elaw`) |

Se um DT tiver instrução que conflite diretamente com a arquitetura do
PIPREVENDA-1680 documentada em `context/CONTEXTO-COMPLETO-PROJETO.md` (seção 12),
**a arquitetura do projeto prevalece** — sinalize o conflito em vez de aplicar
silenciosamente.

O DT-023 (módulo de Anuências Ambientais / Cronoweb) **não é um padrão técnico
genérico** — é uma feature de outro sistema. Foi mantido em
`docs/decisoes-tecnicas/_referencia-externa-nao-aplicavel/` só como exemplo de
como documentar um módulo de domínio via DT. Não implemente nada relacionado a
anuências/Cronoweb neste projeto.

---

## Como usar esta skill

Cada linha da tabela abaixo = um arquivo em `docs/decisoes-tecnicas/`. **Não
leia todos de uma vez.** Identifique a camada/tarefa da sua tarefa atual e abra
só o(s) DT(s) relevante(s).

## Índice por camada

### Domínio
| DT | Quando consultar |
|---|---|
| [DT-022](../../../docs/decisoes-tecnicas/DT-022-padroes-implementacao-entidades-dominio.md) | Criar/alterar entidade de domínio, agregado, value object, invariantes |

### Aplicação (CQRS)
| DT | Quando consultar |
|---|---|
| [DT-016](../../../docs/decisoes-tecnicas/DT-016-padroes-camada-aplicacao.md) | Visão geral de Commands/Queries — **ler primeiro**, depois ir ao DT-019 ou DT-020 |
| [DT-019](../../../docs/decisoes-tecnicas/DT-019-cqrs-query-camada-aplicacao.md) | Implementar uma Query (leitura) — handler, paginação, ordenação |
| [DT-020](../../../docs/decisoes-tecnicas/DT-020-cqrs-commands-camada-aplicacao.md) | Implementar um Command (escrita) — handler, Request/Response, Result |
| [DT-021](../../../docs/decisoes-tecnicas/DT-021-controle-transacional-automatico.md) | Transações em CommandHandlers — **regra: não injete `IUnitOfWork` manualmente**, o decorator já cuida disso, salvo casos de lote/checkpoint |

### Infraestrutura / Persistência
| DT | Quando consultar |
|---|---|
| [DT-018](../../../docs/decisoes-tecnicas/DT-018-padrao-implementacao-repositorios.md) | Criar/alterar repositório de um Aggregate Root |
| [DT-004](../../../docs/decisoes-tecnicas/DT-004-mapeamento-objeto-relacional-ef-core.md) | Mapear entidade no EF Core (`IEntityTypeConfiguration<T>`) |
| [DT-003](../../../docs/decisoes-tecnicas/DT-003-padroes-nomenclatura-banco-dados.md) | Nomear tabelas/colunas/índices — **adaptar de PostgreSQL para SQL Server**, ver aviso acima |

### API / Apresentação
| DT | Quando consultar |
|---|---|
| [DT-006](../../../docs/decisoes-tecnicas/DT-006-padrao-implementacao-api-rest.md) | Criar endpoint REST (Minimal API) |
| [DT-013](../../../docs/decisoes-tecnicas/DT-013-padrao-tratamento-erros-problemdetails.md) | Qualquer retorno de erro HTTP — sempre ProblemDetails (RFC 7807) |

### Transversal
| DT | Quando consultar |
|---|---|
| [DT-005](../../../docs/decisoes-tecnicas/DT-005-padroes-estilo-codigo-backend.md) | Estilo de código C#, nomenclatura, formatação — **aplicar sempre**, em qualquer arquivo novo |
| [DT-007](../../../docs/decisoes-tecnicas/DT-007-padroes-logs-tratamento-excecoes.md) | Logging (Serilog), CorrelationId, o que pode/não pode ir para log |
| [DT-011](../../../docs/decisoes-tecnicas/DT-011-padroes-integracao-apis-externas.md) | Integração com API externa — Anti-Corruption Layer. Ver também skills `integracao-sap`/`integracao-elaw` para os contratos específicos do PIPREVENDA-1680 |
| [DT-008](../../../docs/decisoes-tecnicas/DT-008-padroes-testes-backend.md) | Escrever testes unitários/integração — ferramental e cobertura mínima esperada |

### Template
| DT | Quando consultar |
|---|---|
| [DT-000](../../../docs/decisoes-tecnicas/DT-000-template-decisao-tecnica.md) | Ao **criar um novo DT** para uma decisão ainda não documentada (não para implementar — para registrar uma decisão nova) |

---

## Checklist rápido antes de abrir um Pull Request (resumo — não substitui os checklists de cada DT)

- [ ] Nomenclatura de código segue DT-005 (PT-BR nos nomes de domínio/aplicação, PascalCase/camelCase corretos)
- [ ] Se criou/alterou entidade → DT-022 (invariantes no domínio, não em serviços externos)
- [ ] Se criou Command → `Request`/`Response`, `ICommandHandler`, `Result<T>`, sem `IUnitOfWork` manual (DT-020, DT-021)
- [ ] Se criou Query → sufixo `Response`, `.AsNoTracking()` na infraestrutura, paginação se listagem (DT-019)
- [ ] Se criou endpoint → erros via ProblemDetails, não exceção crua para o cliente (DT-006, DT-013)
- [ ] Se integrou serviço externo → Anti-Corruption Layer, não domínio dependendo de DTO externo (DT-011)
- [ ] Logs sem dado sensível (senha, token, documento completo) — ver lista proibida no DT-007
- [ ] Testes cobrindo o handler/regra criada, seguindo padrão de ferramental do DT-008
- [ ] Nada de nomenclatura/tipagem específica de PostgreSQL vazando para o schema SQL Server (ver aviso de adaptação no topo deste arquivo)
