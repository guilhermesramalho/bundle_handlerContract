import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Input } from "./Input";

describe("Input", () => {
  it("ComLabel_DeveAssociarLabelAoCampoPeloHtmlFor", () => {
    // ============ ARRANGE / ACT ============
    render(<Input label="CNPJ" value="" onChange={() => {}} />);

    // ============ ASSERT ============
    expect(screen.getByLabelText("CNPJ")).toBeInTheDocument();
  });

  it("QuandoDigitado_DeveChamarOnChangeParaCadaCaractere", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<Input label="Busca" value="" onChange={onChange} />);

    // ============ ACT ============
    await user.type(screen.getByLabelText("Busca"), "abc");

    // ============ ASSERT ============
    expect(onChange).toHaveBeenCalledTimes(3);
  });

  it("ComErrorEHelperText_DeveExibirAMensagemDeErro", () => {
    // ============ ARRANGE / ACT ============
    render(<Input label="CNPJ" value="" onChange={() => {}} error helperText="Campo obrigatório" />);

    // ============ ASSERT ============
    expect(screen.getByText("Campo obrigatório")).toBeInTheDocument();
  });

  it("ComOptionalTrue_DeveExibirIndicacaoDeCampoOpcional", () => {
    // ============ ARRANGE / ACT ============
    render(<Input label="Observação" optional value="" onChange={() => {}} />);

    // ============ ASSERT ============
    expect(screen.getByText("(Opcional)")).toBeInTheDocument();
  });

  it("ComDisabledTrue_CampoDeveEstarDesabilitado", () => {
    // ============ ARRANGE / ACT ============
    render(<Input label="CNPJ" value="Não editável" onChange={() => {}} disabled />);

    // ============ ASSERT ============
    expect(screen.getByLabelText("CNPJ")).toBeDisabled();
  });
});
