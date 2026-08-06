import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Checkbox } from "./Checkbox";

describe("Checkbox", () => {
  it("ComLabel_DeveAssociarLabelAoInputPeloHtmlFor", () => {
    // ============ ARRANGE / ACT ============
    render(<Checkbox label="Denúncia" checked={false} />);

    // ============ ASSERT ============
    expect(screen.getByRole("checkbox", { name: "Denúncia" })).toBeInTheDocument();
  });

  it("QuandoClicado_DeveChamarOnChangeComOValorInvertido", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<Checkbox label="Denúncia" checked={false} onChange={onChange} />);

    // ============ ACT ============
    await user.click(screen.getByRole("checkbox", { name: "Denúncia" }));

    // ============ ASSERT ============
    expect(onChange).toHaveBeenCalledWith(true);
  });

  it("ComDisabledTrue_NaoDeveChamarOnChangeAoClicar", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<Checkbox label="Denúncia" checked={false} onChange={onChange} disabled />);

    // ============ ACT ============
    await user.click(screen.getByRole("checkbox", { name: "Denúncia" }));

    // ============ ASSERT ============
    expect(onChange).not.toHaveBeenCalled();
  });

  it("ComIndeterminateTrue_DeveRenderizarIconeDeRemove", () => {
    // ============ ARRANGE / ACT ============
    render(<Checkbox label="Selecionar todos" indeterminate />);

    // ============ ASSERT ============
    expect(screen.getByText("remove")).toBeInTheDocument();
  });
});
