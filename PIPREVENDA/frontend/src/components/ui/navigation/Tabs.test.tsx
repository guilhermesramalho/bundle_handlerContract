import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Tabs } from "./Tabs";

const TABS = [
  { value: "overview", label: "Visão geral" },
  { value: "juridico", label: "Jurídico", badge: 3 },
];

describe("Tabs", () => {
  it("ComTabs_DeveRenderizarTodosOsLabels", () => {
    // ============ ARRANGE / ACT ============
    render(<Tabs tabs={TABS} value="overview" />);

    // ============ ASSERT ============
    expect(screen.getByRole("tab", { name: "Visão geral" })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: /Jurídico/ })).toBeInTheDocument();
  });

  it("ComValueIgualAoTab_DeveMarcarAriaSelectedTrue", () => {
    // ============ ARRANGE / ACT ============
    render(<Tabs tabs={TABS} value="overview" />);

    // ============ ASSERT ============
    expect(screen.getByRole("tab", { name: "Visão geral" })).toHaveAttribute("aria-selected", "true");
    expect(screen.getByRole("tab", { name: /Jurídico/ })).toHaveAttribute("aria-selected", "false");
  });

  it("ComBadge_DeveExibirOValorDoBadgeNaAba", () => {
    // ============ ARRANGE / ACT ============
    render(<Tabs tabs={TABS} value="overview" />);

    // ============ ASSERT ============
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("QuandoUmaAbaEClicada_DeveChamarOnChangeComOValorDaAba", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<Tabs tabs={TABS} value="overview" onChange={onChange} />);

    // ============ ACT ============
    await user.click(screen.getByRole("tab", { name: /Jurídico/ }));

    // ============ ASSERT ============
    expect(onChange).toHaveBeenCalledWith("juridico");
  });
});
