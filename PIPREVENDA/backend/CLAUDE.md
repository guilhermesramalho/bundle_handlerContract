# Backend — PIPREVENDA-1680

Você está trabalhando no backend .NET 10 (Clean Architecture). Antes de
escrever qualquer código aqui, consulte
`.claude/skills/padroes-tecnicos-backend-dotnet/SKILL.md` (raiz do repo) — ela
indexa todas as Decisões Técnicas (DT) obrigatórias por camada.

## Camadas (referência rápida — detalhe completo no contexto, seção 12)

```
Domain → Application (CQRS) → Data / Data.SqlServer (EF Core) → Data.SapStaging → IoC → Api → Worker
```

- `Domain`: entidades puras (Contrato, Aditivo, Guarda-chuva, Grupo
  Econômico, Reappraise) — ver DT-022.
- `Application`: commands/queries CQRS, portas de integração
  (`ISapIntegrationPort`, `IElawIntegrationPort`, `IContratoRepository`) —
  ver DT-016/DT-019/DT-020/DT-021.
- `Data`/`Data.SqlServer`: EF Core — ver DT-004 (adaptar de PostgreSQL para
  SQL Server, ver aviso na skill técnica) e DT-018 (repositórios).
- `Data.SapStaging`: isola staging do ADF do domínio — nunca misturar.
- `Api`: Controllers/Minimal APIs, versionamento, OpenAPI — ver DT-006, DT-013.
- `Worker`: serviço hospedado separado, consumidor/publicador assíncrono via
  Service Bus, deploy independente da API — ver skill `mensageria-service-bus`.

Este repositório ainda não tem código de produto — ao criar a primeira
solução, seguir esta estrutura de projetos e não inventar uma nova.
