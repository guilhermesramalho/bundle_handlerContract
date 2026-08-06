import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Toggle } from "./Toggle";

describe("Toggle", () => {
  it("ComLabel_DeveAssociarLabelAoInputPeloHtmlFor", () => {
    // ============ ARRANGE / ACT ============
    render(<Toggle label="Ativo" checked={false} />);

    // ============ ASSERT ============
    expect(screen.getByRole("checkbox", { name: "Ativo" })).toBeInTheDocument();
  });

  it("QuandoClicado_DeveChamarOnChangeComOValorInvertido", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<Toggle label="Ativo" checked={false} onChange={onChange} />);

    // ============ ACT ============
    await user.click(screen.getByRole("checkbox", { name: "Ativo" }));

    // ============ ASSERT ============
    expect(onChange).toHaveBeenCalledWith(true);
  });

  it("ComDisabledTrue_NaoDeveChamarOnChangeAoClicar", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<Toggle label="Ativo" checked={false} onChange={onChange} disabled />);

    // ============ ACT ============
    await user.click(screen.getByRole("checkbox", { name: "Ativo" }));

    // ============ ASSERT ============
    expect(onChange).not.toHaveBeenCalled();
  });
});
