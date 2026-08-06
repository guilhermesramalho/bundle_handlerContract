import { defineConfig, devices } from "@playwright/test";

// Testes E2E (DT-024, seção 9): reservados para fluxos críticos de negócio
// (login, CRUD de Contrato, aprovação). Nenhum desses fluxos existe ainda
// (F0/F1 — fundação visual e shell); ver e2e/app-shell.spec.ts para o que já
// é possível cobrir hoje.
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: "html",
  use: {
    baseURL: "http://localhost:3000",
    trace: "on-first-retry",
  },
  projects: [
    { name: "chromium", use: { ...devices["Desktop Chrome"] } },
  ],
  webServer: {
    command: "npm run dev",
    url: "http://localhost:3000",
    reuseExistingServer: !process.env.CI,
    timeout: 60_000,
  },
});
