import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Radio } from "./Radio";

function RadioGroup({ onChange }: { onChange: (value: string) => void }) {
  return (
    <>
      <Radio name="segmento" value="rede" label="Rede" checked={false} onChange={onChange} />
      <Radio name="segmento" value="b2b" label="B2B" checked={true} onChange={onChange} />
    </>
  );
}

describe("Radio", () => {
  it("ComLabel_DeveAssociarLabelAoInputPeloHtmlFor", () => {
    // ============ ARRANGE / ACT ============
    render(<Radio name="segmento" value="rede" label="Rede" checked={false} />);

    // ============ ASSERT ============
    expect(screen.getByRole("radio", { name: "Rede" })).toBeInTheDocument();
  });

  it("QuandoOpcaoNaoSelecionadaEClicada_DeveChamarOnChangeComOValorDaOpcao", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<RadioGroup onChange={onChange} />);

    // ============ ACT ============
    await user.click(screen.getByRole("radio", { name: "Rede" }));

    // ============ ASSERT ============
    expect(onChange).toHaveBeenCalledWith("rede");
  });

  it("ComCheckedTrue_OpcaoDeveEstarMarcada", () => {
    // ============ ARRANGE / ACT ============
    render(<RadioGroup onChange={() => {}} />);

    // ============ ASSERT ============
    expect(screen.getByRole("radio", { name: "B2B" })).toBeChecked();
    expect(screen.getByRole("radio", { name: "Rede" })).not.toBeChecked();
  });
});
