import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { Badge } from "./Badge";

describe("Badge", () => {
  it("ComCountAbaixoDoMax_DeveExibirONumeroExato", () => {
    // ============ ARRANGE / ACT ============
    render(<Badge count={12} />);

    // ============ ASSERT ============
    expect(screen.getByText("12")).toBeInTheDocument();
  });

  it("ComCountAcimaDoMax_DeveExibirMaxSeguidoDeMaisSinal", () => {
    // ============ ARRANGE / ACT ============
    render(<Badge count={150} max={99} />);

    // ============ ASSERT ============
    expect(screen.getByText("99+")).toBeInTheDocument();
    expect(screen.queryByText("150")).not.toBeInTheDocument();
  });

  it("ComDotTrue_NaoDeveRenderizarTextoDeContagem", () => {
    // ============ ARRANGE / ACT ============
    const { container } = render(<Badge dot count={5} />);

    // ============ ASSERT ============
    expect(screen.queryByText("5")).not.toBeInTheDocument();
    expect(container.querySelector("span")).toBeInTheDocument();
  });
});
