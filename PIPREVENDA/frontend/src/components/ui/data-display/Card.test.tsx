import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Card } from "./Card";

describe("Card", () => {
  it("ComChildren_DeveRenderizarOConteudo", () => {
    // ============ ARRANGE / ACT ============
    render(<Card>Conteúdo do card</Card>);

    // ============ ASSERT ============
    expect(screen.getByText("Conteúdo do card")).toBeInTheDocument();
  });

  it("ComInteractiveEOnClick_DeveChamarOnClickAoClicar", async () => {
    // ============ ARRANGE ============
    const onClick = vi.fn();
    const user = userEvent.setup();
    render(
      <Card interactive onClick={onClick}>
        Contrato PCR 000123
      </Card>,
    );

    // ============ ACT ============
    await user.click(screen.getByText("Contrato PCR 000123"));

    // ============ ASSERT ============
    expect(onClick).toHaveBeenCalledTimes(1);
  });
});
