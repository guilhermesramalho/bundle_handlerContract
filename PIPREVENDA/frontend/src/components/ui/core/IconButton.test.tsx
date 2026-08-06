import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { IconButton } from "./IconButton";

describe("IconButton", () => {
  it("ComAriaLabelEIcone_DeveRenderizarBotaoAcessivel", () => {
    // ============ ARRANGE / ACT ============
    render(<IconButton icon="edit" aria-label="Editar contrato" />);

    // ============ ASSERT ============
    expect(screen.getByRole("button", { name: "Editar contrato" })).toBeInTheDocument();
    expect(screen.getByText("edit")).toBeInTheDocument();
  });

  it("QuandoClicado_DeveChamarOnClick", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(<IconButton icon="delete" aria-label="Excluir" onClick={onClick} />);

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Excluir" }));

    // ============ ASSERT ============
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("ComDisabledTrue_NaoDeveChamarOnClickAoClicar", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(<IconButton icon="delete" aria-label="Excluir" onClick={onClick} disabled />);

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Excluir" }));

    // ============ ASSERT ============
    expect(onClick).not.toHaveBeenCalled();
  });
});
