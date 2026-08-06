import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Button } from "./Button";

describe("Button", () => {
  it("ComChildren_DeveRenderizarOTextoDoBotao", () => {
    // ============ ARRANGE / ACT ============
    render(<Button>Salvar</Button>);

    // ============ ASSERT ============
    expect(screen.getByRole("button", { name: "Salvar" })).toBeInTheDocument();
  });

  it("QuandoClicado_DeveChamarOnClick", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(<Button onClick={onClick}>Confirmar</Button>);

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Confirmar" }));

    // ============ ASSERT ============
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("ComDisabledTrue_NaoDeveChamarOnClickAoClicar", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(
      <Button onClick={onClick} disabled>
        Indisponível
      </Button>,
    );

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Indisponível" }));

    // ============ ASSERT ============
    expect(onClick).not.toHaveBeenCalled();
    expect(screen.getByRole("button")).toBeDisabled();
  });

  it("ComLoadingTrue_DeveDesabilitarBotaoENaoRenderizarChildren", () => {
    // ============ ARRANGE / ACT ============
    render(<Button loading>Enviando</Button>);

    // ============ ASSERT ============
    expect(screen.getByRole("button")).toBeDisabled();
    expect(screen.queryByText("Enviando")).not.toBeInTheDocument();
  });

  it("ComLeadingEIconeTrailing_DeveRenderizarOsDoisIcones", () => {
    // ============ ARRANGE / ACT ============
    render(
      <Button leadingIcon="add" trailingIcon="arrow_forward">
        Novo
      </Button>,
    );

    // ============ ASSERT ============
    expect(screen.getByText("add")).toBeInTheDocument();
    expect(screen.getByText("arrow_forward")).toBeInTheDocument();
  });
});
