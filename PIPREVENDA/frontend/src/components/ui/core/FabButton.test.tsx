import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { FabButton } from "./FabButton";

describe("FabButton", () => {
  it("ComAriaLabelEIcone_DeveRenderizarBotaoAcessivel", () => {
    // ============ ARRANGE / ACT ============
    render(<FabButton icon="add" aria-label="Adicionar contrato" />);

    // ============ ASSERT ============
    const button = screen.getByRole("button", { name: "Adicionar contrato" });
    expect(button).toBeInTheDocument();
    expect(screen.getByText("add")).toBeInTheDocument();
  });

  it("QuandoClicado_DeveChamarOnClick", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(<FabButton icon="edit" aria-label="Editar" onClick={onClick} />);

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Editar" }));

    // ============ ASSERT ============
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("ComDisabledTrue_BotaoDeveEstarDesabilitado", () => {
    // ============ ARRANGE / ACT ============
    render(<FabButton icon="delete" aria-label="Excluir" disabled />);

    // ============ ASSERT ============
    expect(screen.getByRole("button", { name: "Excluir" })).toBeDisabled();
  });
});
