import { setupWorker } from "msw/browser";
import { handlers } from "./handlers";

// Setup do MSW para desenvolvimento local no browser (mock de API antes do
// backend real estar disponível numa tela). Não usado pela suíte de testes
// (essa usa src/mocks/server.ts) nem ativado automaticamente ainda — ligar
// explicitamente quando a Fase F5 precisar simular a API no navegador.
export const worker = setupWorker(...handlers);
