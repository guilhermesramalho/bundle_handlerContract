# Próximos Passos — PIPREVENDA-1680

> Preencha este arquivo conforme as decisões forem tomadas. Ele não substitui
> `context/CONTEXTO-COMPLETO-PROJETO.md` (fonte de verdade de negócio) nem os
> `docs/decisoes-tecnicas/DT-*.md` (fonte de verdade técnica) — serve para
> planejar o que fazer a seguir, não para redocumentar o que já existe.

**Última atualização:** AAAA-MM-DD
**Responsável por este arquivo:**

---

## 1. Status atual (referência — não editar, ver resumo completo na conversa)

- Documentação de negócio e arquitetura: concluída (contexto consolidado, DTs, skills).
- Backend: scaffolding Clean Architecture pronto + 1 feature completa (`Cliente` CRUD).
- Frontend: ainda não iniciado (apenas `CLAUDE.md` de roteamento).
- Integrações (SAP, PCR, Elaw, ANP): nenhuma implementada — todas bloqueadas por definição técnica externa.

---

## 2. Decisões pendentes que bloqueiam avanço

> Copie da seção 14 do `CONTEXTO-COMPLETO-PROJETO.md` os itens relevantes para
> o próximo ciclo e marque o status real.

| Item | Quem desbloqueia | Status | Prazo/Nota |
|---|---|---|---|
| Base do reappraise (critérios de score) | Suzana Yamada | [ ] Pendente | |
| Campos disponíveis na API do Elaw | Thiago Macedo | [ ] Pendente | |
| CNPJ pode ter mais de uma PCR ativa? | Cliente | [ ] Pendente | |
| Protocolo da API do PCR | Natália Romão | [ ] Pendente | |
| _adicionar outros conforme surgirem_ | | | |

---

## 3. Backlog por entregável (E1–E6)

> Marque o que entra no próximo ciclo/sprint. Detalhe em issues/tasks separadas
> quando for para execução.

### E1 — Base Contratual Consolidada
- [ ] Modelar entidade `Contrato` (ver DT-022) — depende de: definição de PCR único vs PCR/PCF (seção 14)
- [ ] Modelar `GuardaChuva`, `GrupoEconomico`
- [ ] Endpoints de listagem com paginação (obrigatório — volume +20 mil contratos)
- [ ] Estratégia de carga de legado (SICOF) — janela fecha com migração SAP (RT-03)

### E2 — Acompanhamento de Performance
- [ ] _a detalhar_

### E3 — Reappraise
- [ ] _bloqueado até base de critérios de score (Suzana)_

### E4 — Módulo Jurídico
- [ ] _bloqueado até confirmação de campos da API Elaw (Thiago)_

### E5 — Integrações SAP / Data Lake
- [ ] Definir tier do Azure Service Bus e namespace
- [ ] _demais itens, ver seção 12/13 do contexto_

### E6 — Book de Contratos
- [ ] _a detalhar_

---

## 4. Backend — próximos itens técnicos

- [ ] Próxima entidade/feature a implementar após `Cliente`:
- [ ] Cobertura de testes (DT-008) para o CRUD de `Cliente` — status atual:
- [ ] Revisar se `PortalAle.Api` deve ser renomeado para consistência com `GestaoContratoAle.*` (demais projetos já renomeados no commit `254cce9`)
- [ ] Log de auditoria (quem/o quê/quando) — ainda não implementado, é requisito desde E1
- [ ] Observabilidade (OpenTelemetry) — não implementado

---

## 5. Frontend — próximos itens técnicos

- [ ] Inicializar projeto Next.js (ver `.claude/skills/arquitetura-frontend-nextjs`)
- [ ] Geração de tipos TypeScript a partir do OpenAPI da API .NET
- [ ] Primeira tela: _definir (ex.: listagem de Clientes/Contratos)_

---

## 6. Infraestrutura / DevOps

- [ ] Pipeline CI/CD (Azure DevOps) — não iniciado
- [ ] Ambiente de banco de dados (SQL Server) para desenvolvimento/homologação
- [ ] Auth Azure AD B2C — configuração pendente

---

## 7. Riscos a monitorar neste ciclo

> Copiar da seção 13 do contexto os riscos relevantes ao período atual.

| Risco | Ação de mitigação planejada |
|---|---|
| | |

---

## 8. Notas / decisões tomadas neste ciclo

> Registre aqui decisões rápidas que não justificam um DT formal, mas que
> precisam ficar registradas. Se a decisão for técnica e recorrente, criar um
> DT novo em `docs/decisoes-tecnicas/` usando o template `DT-000`.

-
