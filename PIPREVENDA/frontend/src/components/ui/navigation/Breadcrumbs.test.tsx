import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Breadcrumbs } from "./Breadcrumbs";

describe("Breadcrumbs", () => {
  it("ComItens_DeveRenderizarTodosOsLabels", () => {
    // ============ ARRANGE / ACT ============
    render(<Breadcrumbs items={["Contratos", "PCR 000123", "Visão geral"]} />);

    // ============ ASSERT ============
    expect(screen.getByText("Contratos")).toBeInTheDocument();
    expect(screen.getByText("PCR 000123")).toBeInTheDocument();
    expect(screen.getByText("Visão geral")).toBeInTheDocument();
  });

  it("OUltimoItem_DeveEstarMarcadoComoAriaCurrentENaoSerClicavel", () => {
    // ============ ARRANGE / ACT ============
    render(<Breadcrumbs items={["Contratos", "Visão geral"]} />);

    // ============ ASSERT ============
    const current = screen.getByText("Visão geral");
    expect(current).toHaveAttribute("aria-current", "page");
    expect(current.tagName).not.toBe("BUTTON");
  });

  it("QuandoItemIntermediarioEClicado_DeveChamarOnNavigateComOItemEIndice", async () => {
    // ============ ARRANGE ============
    const onNavigate = vi.fn();
    const user = userEvent.setup();
    render(<Breadcrumbs items={["Contratos", "Visão geral"]} onNavigate={onNavigate} />);

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Contratos" }));

    // ============ ASSERT ============
    expect(onNavigate).toHaveBeenCalledWith("Contratos", 0);
  });
});
