import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { Icon } from "./Icon";

describe("Icon", () => {
  it("ComNomeDeIcone_DeveRenderizarOTextoDoMaterialSymbol", () => {
    // ============ ARRANGE / ACT ============
    render(<Icon name="home" />);

    // ============ ASSERT ============
    const icon = screen.getByText("home");
    expect(icon).toBeInTheDocument();
    expect(icon).toHaveClass("material-symbols-rounded");
    expect(icon).toHaveAttribute("aria-hidden", "true");
  });

  it("ComSizeECor_DeveAplicarEstiloCorrespondente", () => {
    // ============ ARRANGE / ACT ============
    render(<Icon name="warning" size={32} color="rgb(255, 0, 0)" />);

    // ============ ASSERT ============
    const icon = screen.getByText("warning");
    expect(icon).toHaveStyle({ fontSize: "32px", color: "rgb(255, 0, 0)" });
  });

  it("ComFillTrue_DeveIncluirFill1NaFontVariationSettings", () => {
    // ============ ARRANGE / ACT ============
    render(<Icon name="favorite" fill />);

    // ============ ASSERT ============
    const icon = screen.getByText("favorite");
    expect(icon.style.fontVariationSettings).toContain("'FILL' 1");
  });
});
