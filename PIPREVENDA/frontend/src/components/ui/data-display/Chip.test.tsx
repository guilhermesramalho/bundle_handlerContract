import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Chip } from "./Chip";

describe("Chip", () => {
  it("ComSelectedTrue_DeveRenderizarIconeDeCheck", () => {
    // ============ ARRANGE / ACT ============
    render(<Chip selected>Rede</Chip>);

    // ============ ASSERT ============
    expect(screen.getByText("check_circle")).toBeInTheDocument();
  });

  it("ComSelectedFalse_NaoDeveRenderizarIconeDeCheck", () => {
    // ============ ARRANGE / ACT ============
    render(<Chip selected={false}>B2B</Chip>);

    // ============ ASSERT ============
    expect(screen.queryByText("check_circle")).not.toBeInTheDocument();
  });

  it("QuandoClicado_DeveChamarOnClick", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(<Chip onClick={onClick}>Segmento</Chip>);

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Segmento" }));

    // ============ ASSERT ============
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("ComDisabledTrue_NaoDeveChamarOnClickAoClicar", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(
      <Chip onClick={onClick} disabled>
        Indisponível
      </Chip>,
    );

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Indisponível" }));

    // ============ ASSERT ============
    expect(onClick).not.toHaveBeenCalled();
  });
});
