import { setupServer } from "msw/node";
import { handlers } from "./handlers";

// Setup do MSW para os testes (Vitest, ambiente node/jsdom). Ver vitest.setup.ts.
export const server = setupServer(...handlers);
