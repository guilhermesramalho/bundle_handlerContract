# Plano de Implementação — PIPREVENDA-1680

> Detalha **como** executar o que está em `PROXIMOS-PASSOS.md`. Fonte de
> verdade continua sendo `context/CONTEXTO-COMPLETO-PROJETO.md` (negócio) e
> `docs/decisoes-tecnicas/DT-*.md` (técnico) — este documento só ordena e
> detalha a execução.

**Premissas assumidas ao ordenar as fases** (ajuste se não corresponder à realidade):
- Um desenvolvedor (ou poucos) trabalhando de forma majoritariamente sequencial, com backend e frontend podendo avançar em paralelo a partir da Fase 2.
- Sem data-compromisso externa nesta fase — a ordem é por **dependência técnica/bloqueio**, não por prazo. Ajuste com datas reais se houver um cronograma comercial.
- Itens que dependem de terceiros (cliente ALE, Natália/PCR, Thiago/Elaw) são tratados como **bloqueados**, não como tarefas de engenharia — a "ação" nesses casos é cobrança/reunião, não código.

---

## Visão geral das fases

| Fase | Nome | Pode começar já? | Bloqueio |
|---|---|---|---|
| 0 | Dívida técnica do que já existe | ✅ Sim | — |
| 1 | Domínio E1 — Base Contratual (`GrupoEconomico`, `GuardaChuva`, `Contrato`) | ⚠️ Parcial | Item 1.3 depende de decisões de negócio |
| 2 | Frontend — bootstrap Next.js + 1ª tela | ✅ Sim (paralelo à Fase 1) | Precisa da API de Clientes já existente |
| 3 | Infraestrutura mínima (banco dev, CI básico) | ✅ Sim (paralelo) | — |
| 4 | Auth Azure AD B2C | ⚠️ Parcial | Confirmar tenant/política com TI ALE |
| 5 | Integrações externas (SAP, PCR, Elaw, ANP) | ❌ Não | Todas aguardando definição técnica de terceiros |
| 6 | E2/E3/E4/E6 (Performance, Reappraise, Jurídico, Book) | ❌ Não | Aguardando insumos de negócio (seção 14 do contexto) |

---

## Fase 0 — Dívida técnica do que já existe

Objetivo: fechar lacunas na vertical `Cliente` (único código de produto existente) antes de replicar o padrão para novas entidades — evitar propagar buracos.

### 0.1 Testes automatizados (DT-008)
**Situação atual:** não existe projeto de testes no backend (confirmado — nenhum `*.Tests.csproj` na solução).
- [ ] Criar projeto `GestaoContratoAle.Application.Tests` (unit) e `GestaoContratoAle.Data.SqlServer.Tests` ou `IntegrationTests` (integração), conforme ferramental definido em DT-008.
- [ ] Testes de domínio: `Cliente` — CNPJ válido/inválido (usar CNPJs de teste conhecidos), nome/endereço vazios ou > 200 chars, `Atualizar` após `ExcluirLogicamente` deve lançar `DomainException`.
- [ ] Testes de handler (Application): `CriarClienteCommandHandler`, `AtualizarClienteCommandHandler`, `ExcluirClienteCommandHandler`, `ConsultarClienteQueryHandler`, `ListarClientesQueryHandler` — com repositório/queries mockados (verificar se DT-008 recomenda NSubstitute/Moq).
- [ ] Teste de integração do `ClienteRepository`/`ClienteQueries` contra banco real (ou containerizado) — valida `ClienteConfiguration` e a migration.
- **Critério de pronto:** pipeline local roda `dotnet test` verde; cobertura mínima definida no DT-008 atingida para a vertical `Cliente`.
- **Por que antes da Fase 1:** o padrão de teste criado aqui é o que será copiado para `GrupoEconomico`/`Contrato` — errar agora custa menos que corrigir em 3 verticais depois.

### 0.2 Log de auditoria (quem/o quê/quando)
**Situação atual:** não implementado; requisito explícito desde E1 (seção 12 do contexto), sem DT dedicado hoje.
- [ ] Decidir abordagem: interceptor de `SaveChanges` no `ApplicationDbContext` (captura automática de todas as entidades) vs. tabela de auditoria alimentada manualmente por handler. Recomendado: interceptor — não depende de cada handler lembrar de logar.
- [ ] Modelar tabela `LogAuditoria` (Entidade, EntidadeId, Operacao [Criar/Atualizar/Excluir], UsuarioId, DataHora UTC, ValoresAntigos/ValoresNovos em JSON).
- [ ] **Bloqueio parcial:** `UsuarioId` real depende de autenticação (Fase 4, ainda não implementada). Até lá, usar um valor placeholder explícito (ex.: `"sistema"`) e não inventar um usuário fake silenciosamente.
- [ ] Registrar esta decisão como um novo DT (`DT-024-padrao-log-auditoria.md`, usando o template `DT-000`) — é uma decisão técnica nova, não coberta pelos DTs herdados.
- **Critério de pronto:** qualquer Create/Update/Delete em `Cliente` gera uma linha em `LogAuditoria`, validado por teste de integração.

### 0.3 Renomeação `PortalAle.Api` → consistência de nome
**Situação atual:** todos os projetos foram renomeados de `PortalAle.*` para `GestaoContratoAle.*` no commit `254cce9`, exceto `PortalAle.Api` (confirmado lendo `Program.cs`/`ClientesEndpoints.cs` — ainda usam namespace `PortalAle.*`).
- [ ] Decidir: manter `PortalAle.Api` (nome do produto voltado a usuário pode ser diferente do nome técnico da solução) ou renomear para `GestaoContratoAle.Api`.
- [ ] Se renomear: projeto, namespace, referências na `.sln`, string `errorTypeBaseUrl` (`https://api.portalale.com.br/errors`) e o enrich de log `"Aplicacao", "PortalAle-Backend"` em `Program.cs`.
- **Critério de pronto:** nenhuma referência mista `PortalAle`/`GestaoContratoAle` sem justificativa registrada.

### 0.4 Observabilidade — decisão de escopo
**Situação atual:** `CorrelationIdMiddleware` e Serilog estruturado já existem; OpenTelemetry ponta a ponta (frontend → API → worker → broker) ainda não.
- [ ] Decisão: implementar OpenTelemetry agora (sem frontend/worker ainda existentes, valor é baixo) ou adiar para quando houver pelo menos Fase 2 (frontend) e Fase 5 (worker) prontos.
- **Recomendação:** adiar para depois da Fase 2/3 — hoje não há múltiplos serviços para correlacionar além da própria API.

---

## Fase 1 — Domínio E1: Base Contratual Consolidada

Ordem interna importa: `GrupoEconomico` tem menos dependências de decisão de negócio que `GuardaChuva`/`Contrato`.

### 1.1 `GrupoEconomico` (menor risco — começar por aqui)
- [ ] Modelar entidade conforme DT-022: `Codigo` (SAP, 3 dígitos), `Nome`, coleção de `Cliente`s vinculados.
- [ ] Adicionar `GrupoEconomicoId` (nullable) em `Cliente` — migration de alteração, não recriar tabela.
- [ ] CQRS: `Criar`, `Consultar`, `Listar` (avaliar se `Atualizar`/`Excluir` fazem sentido já ou só quando vier do SAP — ver nota abaixo).
- [ ] `GrupoEconomicoConfiguration` (EF Core, DT-004) + migration.
- [ ] Endpoints REST (DT-006) + testes (replicando o padrão fechado na Fase 0.1).
- ⚠️ **Nota de escopo:** a seção 12 do contexto indica que dados de grupo econômico viriam do SAP no desenho-alvo. Como a integração SAP está bloqueada (Fase 5), implementar como cadastro manual por ora é uma decisão de **placeholder consciente** — registrar isso no PR/DT, não deixar implícito.

### 1.2 `GuardaChuva` (bloqueio parcial)
- [ ] Modelar `GuardaChuva` (CNPJ principal + CNPJs adicionais) conforme DT-022.
- [ ] **Antes de fechar o relacionamento com `Contrato`/galonagem**, confirmar com o cliente: *"CNPJ pode ter mais de uma PCR ativa simultaneamente?"* (item já listado na seção 14 do contexto e em `PROXIMOS-PASSOS.md` seção 2) — essa resposta define se a listagem principal é por PCR ou por CNPJ, e se um `GuardaChuva` pode ter múltiplos contratos simultâneos por CNPJ.
- [ ] Modelar regra de galonagem individual (Rede) vs. global (B2B/COFA-COFD-COFAR) — regra já está fechada na seção 9 do contexto, pode ser implementada mesmo com a pendência acima, desde que a entidade não assuma "1 CNPJ = 1 contrato ativo" de forma rígida no schema.
- **Critério de pronto:** modelo suporta os dois regimes de galonagem sem retrabalho estrutural quando a pendência de PCR múltipla for resolvida.

### 1.3 `Contrato` (aggregate root principal — mais bloqueios)
- [ ] Campo de identificação único (PCR/PCF com prefixo, não dois campos separados — já decidido, seção 8 do contexto).
- [ ] Ciclo de vida como enum + tabela `HistoricoEventoContrato` (novo negócio, renovação, readequação, cessão, sucessão, denúncia, encerramento — únicos tipos válidos, seção 9).
- [ ] Tipos de contrato: `PCVM`, `Imagem` (Rede, em descontinuação — não bloquear implementação por isso), `Comodato`.
- [ ] Segmento: Rede / GRR / COFA / COFD / COFAR / Outros / Spot.
- [ ] Repositório e queries **com paginação desde o primeiro commit** (DT-019) — não é opcional dado o volume de +20 mil contratos (RT-07).
- [ ] Índices desde a primeira migration (CNPJ, PCR/PCF, segmento, data de vencimento — os campos mais usados em filtro segundo F04/F12).
- ⚠️ **Não iniciar antes de:** confirmar com o PM se a modelagem pode seguir mesmo com os itens da seção 14 ainda pendentes (a maioria não bloqueia o schema básico, mas vale alinhamento explícito antes de investir tempo de implementação).

### 1.4 Endpoints de listagem/filtro (F04)
- [ ] Filtros combináveis: CNPJ, nº SAP, grupo econômico (busca texto), UF, segmento, hierarquia comercial (Diretor/Gerente Regional/Coordenador/Consultor Comercial — cascata com seleção múltipla, seção 8).
- [ ] Sem "salvar filtro" na v1 (decisão já fechada).
- **Depende de:** 1.1, 1.2, 1.3 completos.

---

## Fase 2 — Frontend: bootstrap (paralelo à Fase 1)

> Plano detalhado de reconstrução do protótipo (`docs/modelo-frontend/prototipo.html`) — design system, tokens, telas — está em `PLANO-IMPLEMENTACAO-FRONTEND.md`. O resumo abaixo é só o essencial de bootstrap.

Pode começar assim que a API de `Cliente` estiver estável (já está) — não precisa esperar `Contrato`.

- [ ] Inicializar projeto Next.js (App Router) dentro de `PIPREVENDA/frontend/` seguindo a skill `arquitetura-frontend-nextjs`.
- [ ] Configurar geração de tipos TypeScript a partir do OpenAPI exposto pela API .NET (`/openapi/v1.json`) — script de geração no `package.json`, rodar a cada mudança de contrato.
- [ ] Configurar React Query para data-fetching.
- [ ] Estrutura de rotas por entregável: iniciar só com o que existe (`/clientes` como prova de conceito) — não criar pastas vazias para `/performance`, `/reappraise` etc. ainda.
- [ ] Primeira tela real: listagem de Clientes consumindo `GET /api/v1/clientes` (paginado) — objetivo é validar o pipeline completo (tipos gerados → fetch → tabela), não é tela de negócio final.
- **Critério de pronto:** `npm run dev` mostra listagem de clientes real vindo da API local.
- **Nota:** shell de autenticação (SSR) fica adiado até a Fase 4 (Auth B2C) — por ora, app roda sem login.

---

## Fase 3 — Infraestrutura mínima viável (paralelo)

Escopo reduzido do desenho-alvo completo (seção 12 do contexto) — só o suficiente para desenvolver e validar localmente/em um ambiente de dev.

- [ ] Banco SQL Server para desenvolvimento (`docker-compose` local ou instância dev na nuvem) — hoje a `ConnectionString` já é lida de `appsettings`, falta só o ambiente.
- [ ] Pipeline CI básico: build + test (bloqueado até Fase 0.1 existir) — antes de montar o pipeline multi-stage completo (build→test→package→push ACR→deploy Helm) descrito na arquitetura-alvo.
- [ ] Adiar Kubernetes/Helm/ACR até haver mais de um serviço implantável (hoje só a API existe) — priorizar velocidade de iteração local.

---

## Fase 4 — Autenticação (Azure AD B2C)

**Bloqueio:** política de sign-in e claims do token ainda não confirmadas com a TI da ALE (ver seção 12/13 do contexto — "decisões técnicas em aberto").
- [ ] Ação de negócio/PM: confirmar com TI ALE se o tenant B2C já existe e qual política de sign-in usar.
- [ ] Após confirmação: `UseAuthentication()` em `Program.cs` (hoje só `AddAuthorization()`/`UseAuthorization()` estão presentes, exatamente preparados para este passo — ver comentário já existente no código).
- [ ] `.RequireAuthorization()` nos endpoints de `ClientesEndpoints` e futuros.
- [ ] Resolver o placeholder de `UsuarioId` no log de auditoria (Fase 0.2) com o usuário real do token.

---

## Fase 5 — Integrações externas (bloqueadas)

Nenhuma tem contrato técnico definitivo. Ação nesta fase é **desbloqueio**, não implementação:

| Integração | O que falta | Quem resolve |
|---|---|---|
| SAP (entrada/saída) | Documentação/acesso às APIs, confirmação do padrão ADF→staging→Service Bus | Thiago Macedo / TI ALE (Lelio, Daniel) |
| PCR | Protocolo de integração do novo sistema (outro fornecedor) | Natália Romão |
| Elaw | Confirmação de campos disponíveis via API | Thiago Macedo (reunião em andamento) |
| ANP | Existência de API pública/estruturada | A definir |

Quando cada uma desbloquear, seguir as skills `integracao-sap`/`integracao-elaw`/`mensageria-service-bus` e DT-011 (Anti-Corruption Layer) — não modelar `Domain` acoplado a DTO externo.

---

## Fase 6 — E2/E3/E4/E6

Todos dependem de insumos de negócio ainda não recebidos (seção 14 do contexto / seção 2 de `PROXIMOS-PASSOS.md`). Não iniciar modelagem de domínio para estes antes de:
- **E3 (Reappraise):** base de critérios de score (Suzana Yamada).
- **E4 (Jurídico):** confirmação de campos da API Elaw (Fase 5) + especificação de campos de garantia (Fernanda).
- **E2 (Performance):** menos bloqueado que E3/E4 — pode ser modelado assim que `Contrato` (Fase 1.3) existir, já que as regras de galonagem/projeção estão bem fechadas na seção 9. Candidato a **entrar antes de E3/E4** na fila.
- **E6 (Book Executivo):** aguarda formatos de relatório de diretoria/Glencore (Ana Caldas) — mas a base técnica (read model dedicado para performance, dado o volume) pode começar a ser desenhada em paralelo à Fase 1.

---

## Resumo executável — por onde começar amanhã

1. **Fase 0.1** — criar projeto de testes e cobrir a vertical `Cliente` (maior ROI: virá de graça para toda entidade nova).
2. **Fase 1.1** — `GrupoEconomico` (menor bloqueio de negócio, reaproveita 100% do padrão testado no passo 1).
3. **Fase 2** (em paralelo, se houver front-end disponível) — bootstrap Next.js consumindo `Cliente`.
4. Em paralelo, via PM: cobrar as respostas da seção 14 do contexto que bloqueiam 1.2/1.3 (CNPJ com múltiplas PCR) e Fase 4 (tenant B2C) — sem isso, Fase 1.3 (`Contrato`) fica de scaffolding pela metade.
