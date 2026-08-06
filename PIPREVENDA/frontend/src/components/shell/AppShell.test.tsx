import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AppShell } from "./AppShell";

const push = vi.fn();
let currentPathname = "/";

vi.mock("next/navigation", () => ({
  usePathname: () => currentPathname,
  useRouter: () => ({ push }),
}));

describe("AppShell", () => {
  beforeEach(() => {
    push.mockClear();
    currentPathname = "/";
  });

  it("DeveRenderizarOsLinksDeNavegacaoContratosEDashboard", () => {
    // ============ ARRANGE / ACT ============
    render(<AppShell>Conteúdo</AppShell>);

    // ============ ASSERT ============
    expect(screen.getByRole("link", { name: /Contratos/ })).toHaveAttribute("href", "/contratos");
    expect(screen.getByRole("link", { name: /Dashboard/ })).toHaveAttribute("href", "/dashboard");
  });

  it("DeveRenderizarOConteudoFilhoDentroDoShell", () => {
    // ============ ARRANGE / ACT ============
    render(<AppShell>Conteúdo da página</AppShell>);

    // ============ ASSERT ============
    expect(screen.getByText("Conteúdo da página")).toBeInTheDocument();
  });

  it("QuandoPathnameEstaEmContratos_DeveMarcarOLinkContratosComoAtivo", () => {
    // ============ ARRANGE ============
    currentPathname = "/contratos";

    // ============ ACT ============
    render(<AppShell>Conteúdo</AppShell>);

    // ============ ASSERT ============
    const contratosLink = screen.getByRole("link", { name: /Contratos/ });
    const dashboardLink = screen.getByRole("link", { name: /Dashboard/ });
    expect(contratosLink.className).toMatch(/navLinkActive/);
    expect(dashboardLink.className).not.toMatch(/navLinkActive/);
  });

  it("QuandoBuscaGlobalESubmetida_DeveNavegarParaContratosComOParametroQ", async () => {
    // ============ ARRANGE ============
    const user = userEvent.setup();
    render(<AppShell>Conteúdo</AppShell>);

    // ============ ACT ============
    await user.type(screen.getByLabelText("Busca global de contratos"), "12.345.678/0001-99");
    await user.keyboard("{Enter}");

    // ============ ASSERT ============
    expect(push).toHaveBeenCalledWith("/contratos?q=12.345.678%2F0001-99");
  });

  it("ComBuscaVazia_NaoDeveNavegarAoSubmeter", async () => {
    // ============ ARRANGE ============
    const user = userEvent.setup();
    render(<AppShell>Conteúdo</AppShell>);

    // ============ ACT ============
    await user.click(screen.getByLabelText("Busca global de contratos"));
    await user.keyboard("{Enter}");

    // ============ ASSERT ============
    expect(push).not.toHaveBeenCalled();
  });

  it("QuandoClicaNoSinoDeNotificacoes_DeveAbrirOPainelComEstadoVazio", async () => {
    // ============ ARRANGE ============
    const user = userEvent.setup();
    render(<AppShell>Conteúdo</AppShell>);
    expect(screen.queryByRole("menu")).not.toBeInTheDocument();

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Notificações" }));

    // ============ ASSERT ============
    expect(screen.getByRole("menu")).toBeInTheDocument();
    expect(screen.getByText("Nenhuma notificação no momento.")).toBeInTheDocument();
  });

  it("QuandoClicaForaDoPainelDeNotificacoesAberto_DeveFecharOPainel", async () => {
    // ============ ARRANGE ============
    const user = userEvent.setup();
    render(<AppShell>Conteúdo</AppShell>);
    await user.click(screen.getByRole("button", { name: "Notificações" }));
    expect(screen.getByRole("menu")).toBeInTheDocument();

    // ============ ACT ============
    await user.click(screen.getByText("Conteúdo"));

    // ============ ASSERT ============
    expect(screen.queryByRole("menu")).not.toBeInTheDocument();
  });
});
