import "@testing-library/jest-dom/vitest";
import { afterAll, afterEach, beforeAll } from "vitest";
import { server } from "./src/mocks/server";

// MSW intercepta chamadas HTTP em nível de rede — ver DT-024
// (docs/decisoes-tecnicas/DT-024-padroes-testes-frontend.md), seção 2.4.
// Nenhum componente hoje faz chamada de rede (F2+ ainda não existe), mas o
// server já fica de pé para os testes de integração que virão.
beforeAll(() => server.listen({ onUnhandledRequest: "error" }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
