import type { HttpHandler } from "msw";

/**
 * Handlers MSW compartilhados entre testes e ambiente de desenvolvimento
 * (ver DT-024, seção 2.4). Vazio de propósito: `useContratos()` (Fase F5,
 * `PLANO-IMPLEMENTACAO-FRONTEND.md`) já chama a API real
 * (`GET /api/v1/contratos`), então cada teste registra o handler que
 * precisa via `server.use(...)` no próprio arquivo de teste — não há um
 * "shape" de resposta compartilhado o suficiente entre eles para justificar
 * um handler default aqui ainda.
 */
export const handlers: HttpHandler[] = [];
