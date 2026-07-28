# PIPREVENDA-1680 — Sistema de Gestão de Contratos ALE Combustíveis

Antes de qualquer tarefa envolvendo regra de negócio, arquitetura ou
integração, **consulte a skill correspondente em `.claude/skills/` — não
assuma comportamento não documentado.** Se a informação não estiver lá nem em
`context/`, sinalize a lacuna em vez de inventar.

## Roteamento

| Tarefa | Skill / documento |
|---|---|
| Regra de negócio, domínio, glossário ALE | `.claude/skills/regras-negocio-contratos` |
| Qualquer código no backend .NET (entidade, command, query, repositório, endpoint, teste) | `.claude/skills/padroes-tecnicos-backend-dotnet` |
| Tela/componente/rota no frontend Next.js | `.claude/skills/arquitetura-frontend-nextjs` |
| Integração com o SAP | `.claude/skills/integracao-sap` |
| Aba Jurídico / dados do Elaw | `.claude/skills/integracao-elaw` |
| Evento assíncrono, Worker, Service Bus | `.claude/skills/mensageria-service-bus` |

## Fonte de verdade

- Negócio: `context/CONTEXTO-COMPLETO-PROJETO.md` (ver seção 16 do próprio
  documento — como usar, precedência entre versões)
- Jurídico/Elaw: `context/integracao-elaw.html`
- Decisões técnicas de backend: `docs/decisoes-tecnicas/DT-*.md` (indexadas
  pela skill `padroes-tecnicos-backend-dotnet`)

## Regras gerais para qualquer agente neste repositório

1. Itens marcados como "em aberto" no contexto de negócio (seção 14) ou como
   "bloqueado"/"a definir" nas skills de integração **não devem ser
   implementados ou assumidos** sem confirmação do PM ou do cliente.
2. Este projeto ainda está em Discovery/Prototipação — **nenhuma linha de
   código de produto foi escrita ainda** (ver seção 1 do contexto). Não
   assuma que uma estrutura de pastas de código já existe até confirmar.
3. Volume de +20 mil contratos deve influenciar toda decisão de modelagem,
   paginação ou UX de listagem desde o primeiro desenho.
4. Ao criar uma decisão técnica nova (ainda não coberta por nenhum DT),
   registre-a em `docs/decisoes-tecnicas/` seguindo o template `DT-000`, em
   vez de decidir implicitamente dentro do código.
