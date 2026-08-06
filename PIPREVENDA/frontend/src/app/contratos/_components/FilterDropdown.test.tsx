import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { FilterDropdown } from "./FilterDropdown";

describe("FilterDropdown", () => {
  it("SelecaoMultiplaClicaDuasOpcoes_ChamaOnChangeComAsDuasSelecionadas", async () => {
    // Arrange
    const user = userEvent.setup();
    const onChange = vi.fn();
    const { rerender } = render(
      <FilterDropdown label="Segmento" placeholder="Todos" options={["Rede", "GRR", "COFA"]} selected={[]} multiple onChange={onChange} />,
    );

    // Act
    await user.click(screen.getByRole("button", { name: "Filtro Segmento" }));
    await user.click(screen.getByRole("option", { name: "Rede" }));

    // Assert
    expect(onChange).toHaveBeenCalledWith(["Rede"]);

    // Act — simula o estado controlado após a primeira seleção e marca uma segunda opção.
    rerender(
      <FilterDropdown label="Segmento" placeholder="Todos" options={["Rede", "GRR", "COFA"]} selected={["Rede"]} multiple onChange={onChange} />,
    );
    await user.click(screen.getByRole("option", { name: "GRR" }));

    // Assert
    expect(onChange).toHaveBeenLastCalledWith(["Rede", "GRR"]);
  });

  it("SelecaoMultiplaDesmarcaOpcaoJaSelecionada_RemoveDaLista", async () => {
    // Arrange
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(
      <FilterDropdown label="Segmento" placeholder="Todos" options={["Rede", "GRR"]} selected={["Rede", "GRR"]} multiple onChange={onChange} />,
    );

    // Act
    await user.click(screen.getByRole("button", { name: "Filtro Segmento" }));
    await user.click(screen.getByRole("option", { name: "Rede" }));

    // Assert
    expect(onChange).toHaveBeenCalledWith(["GRR"]);
  });

  it("SelecaoUnicaEscolheOpcao_ChamaOnChangeEFechaMenu", async () => {
    // Arrange
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(
      <FilterDropdown label="Bandeira ANP" placeholder="Todas" options={["Bandeira ALE", "Bandeira Branca"]} selected={[]} multiple={false} onChange={onChange} />,
    );

    // Act
    await user.click(screen.getByRole("button", { name: "Filtro Bandeira ANP" }));
    await user.click(screen.getByRole("option", { name: "Bandeira ALE" }));

    // Assert
    expect(onChange).toHaveBeenCalledWith(["Bandeira ALE"]);
    expect(screen.queryByRole("listbox")).not.toBeInTheDocument();
  });

  it("SelecaoUnicaClicaOpcaoPlaceholder_LimpaSelecao", async () => {
    // Arrange
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(
      <FilterDropdown label="Bandeira ANP" placeholder="Todas" options={["Bandeira ALE"]} selected={["Bandeira ALE"]} multiple={false} onChange={onChange} />,
    );

    // Act
    await user.click(screen.getByRole("button", { name: "Filtro Bandeira ANP" }));
    await user.click(screen.getByRole("button", { name: "Todas" }));

    // Assert
    expect(onChange).toHaveBeenCalledWith([]);
  });

  it("DuasOpcoesSelecionadas_ExibeContagemNoTrigger", () => {
    // Arrange / Act
    render(<FilterDropdown label="UF" placeholder="Todas" options={["SP", "RJ", "MG"]} selected={["SP", "RJ"]} onChange={vi.fn()} />);
    // Assert
    expect(screen.getByRole("button", { name: "Filtro UF" })).toHaveTextContent("2 selecionadas");
  });
});
