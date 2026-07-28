---
name: arquitetura-frontend-nextjs
description: Padrões de arquitetura do frontend React/Next.js do PIPREVENDA-1680 — estrutura de rotas por entregável, geração de tipos a partir do OpenAPI, estratégia CSR/SSR, data-fetching com React Query. Use sempre que for criar página, componente, hook de dados ou decidir CSR vs SSR neste projeto.
---

# Arquitetura — Frontend Next.js (PIPREVENDA-1680)

> Status: ainda não há DTs (Decisões Técnicas) detalhadas de frontend
> equivalentes às de backend neste bundle. Esta skill reflete o que já está
> definido em `context/CONTEXTO-COMPLETO-PROJETO.md` (seção 12) — quando
> DTs de frontend forem criadas (nomenclatura de componentes, padrão de
> formulário, gerenciamento de estado, testes de UI), adicione-as em
> `docs/decisoes-tecnicas/` e referencie aqui, seguindo o mesmo padrão da
> skill `padroes-tecnicos-backend-dotnet`.

## Decisões já confirmadas (fonte: contexto do projeto, seção 12)

- **App Router por entregável**: `/contratos`, `/performance`, `/reappraise`,
  `/juridico`, `/book` — módulos espelham os entregáveis E1–E6 do escopo.
- **Tipos TypeScript gerados a partir do OpenAPI** exposto pela API .NET —
  nunca escrever tipo de resposta de API manualmente; gerar para evitar drift.
- **CSR nas telas de dados** (paginação/streaming) — dado o volume de +20 mil
  contratos, **SSR de tabela grande piora TTFB sem ganho real**. Não usar SSR
  para listagens paginadas.
- **SSR apenas no shell/autenticação**.
- **React Query** para data-fetching — não implementar fetch manual com
  `useEffect` para dados de servidor.

## Antes de criar uma tela nova

1. Confirmar a qual entregável (E1–E6) a tela pertence — ver
   `context/CONTEXTO-COMPLETO-PROJETO.md` seção correspondente.
2. Confirmar se o endpoint OpenAPI já existe; se não, sinalizar dependência
   do backend em vez de mockar payload definitivo.
3. Seguir convenções gerais de design da skill `frontend-design` (se
   disponível no ambiente) para tokens visuais, tipografia e estilo.
