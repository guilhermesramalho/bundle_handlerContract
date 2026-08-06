import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Select } from "./Select";

// O <button> gatilho tem um <label htmlFor> associado (acessibilidade do
// campo "Segmento"), então seu nome acessível vem do label, não do texto
// visível ("Selecione"/valor escolhido) — por isso as queries abaixo usam
// getByRole("button", { name: <label do campo> }) para abrir/fechar, e
// getByText para conferir o texto exibido dentro do gatilho.

describe("Select", () => {
  it("SemValorSelecionado_DeveExibirOPlaceholder", () => {
    // ============ ARRANGE / ACT ============
    render(<Select label="Segmento" placeholder="Selecione o segmento" options={["Rede", "GRR"]} />);

    // ============ ASSERT ============
    expect(screen.getByText("Selecione o segmento")).toBeInTheDocument();
  });

  it("QuandoClicado_DeveAbrirAListaDeOpcoes", async () => {
    // ============ ARRANGE ============
    const user = userEvent.setup();
    render(<Select label="Segmento" options={["Rede", "GRR", "COFA"]} />);

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Segmento" }));

    // ============ ASSERT ============
    expect(screen.getByRole("button", { name: "GRR" })).toBeInTheDocument();
  });

  it("QuandoUmaOpcaoEEscolhida_DeveChamarOnChangeEFecharALista", async () => {
    // ============ ARRANGE ============
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<Select label="Segmento" options={["Rede", "GRR", "COFA"]} onChange={onChange} />);
    await user.click(screen.getByRole("button", { name: "Segmento" }));

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "GRR" }));

    // ============ ASSERT ============
    expect(onChange).toHaveBeenCalledWith("GRR");
    expect(screen.queryByRole("button", { name: "COFA" })).not.toBeInTheDocument();
  });

  it("ComValorSelecionado_DeveExibirOLabelDaOpcaoNoGatilho", () => {
    // ============ ARRANGE / ACT ============
    render(
      <Select
        label="Segmento"
        value="rede"
        options={[
          { value: "rede", label: "Rede" },
          { value: "grr", label: "GRR" },
        ]}
      />,
    );

    // ============ ASSERT ============
    expect(screen.getByText("Rede").closest("button")).toBeInTheDocument();
  });

  it("QuandoClicaForaDoComponente_DeveFecharALista", async () => {
    // ============ ARRANGE ============
    const user = userEvent.setup();
    render(
      <div>
        <Select label="Segmento" options={["Rede", "GRR"]} />
        <button type="button">Fora</button>
      </div>,
    );
    await user.click(screen.getByRole("button", { name: "Segmento" }));
    expect(screen.getByRole("button", { name: "GRR" })).toBeInTheDocument();

    // ============ ACT ============
    await user.click(screen.getByRole("button", { name: "Fora" }));

    // ============ ASSERT ============
    expect(screen.queryByRole("button", { name: "GRR" })).not.toBeInTheDocument();
  });
});
