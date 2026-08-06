import { test, expect } from "@playwright/test";

// Ainda não há fluxo crítico de negócio implementado (login, CRUD de
// Contrato — ver DT-024 seção 9): estas telas dependem das Fases F2+ do
// PLANO-IMPLEMENTACAO-FRONTEND.md. Este smoke test cobre o que já existe
// (shell da Fase F1) para não deixar o Playwright configurado sem uso; deve
// ser substituído/complementado pelos fluxos críticos reais assim que
// Contratos e autenticação existirem.
test.describe("Shell da aplicação", () => {
  test("Usuario_AoAbrirAHome_DeveVerNavegacaoParaContratosEDashboard", async ({ page }) => {
    // ============ ARRANGE / ACT ============
    await page.goto("/");

    // ============ ASSERT ============
    await expect(page.getByRole("link", { name: "Contratos" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Dashboard" })).toBeVisible();
  });

  test("Usuario_AoAbrirDesignSystem_DeveVerOsComponentesPortados", async ({ page }) => {
    // ============ ARRANGE / ACT ============
    await page.goto("/design-system");

    // ============ ASSERT ============
    await expect(page.getByRole("heading", { name: /Design System/ })).toBeVisible();
    await expect(page.getByRole("button", { name: "Primary" })).toBeVisible();
  });

  test("Usuario_AoDigitarNaBuscaGlobalEConfirmar_DeveNavegarParaContratosComQuery", async ({ page }) => {
    // ============ ARRANGE ============
    await page.goto("/");

    // ============ ACT ============
    await page.getByLabel("Busca global de contratos").fill("12.345.678/0001-99");
    await page.getByLabel("Busca global de contratos").press("Enter");

    // ============ ASSERT ============
    await expect(page).toHaveURL(/\/contratos\?q=/);
  });
});
