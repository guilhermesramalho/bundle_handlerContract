import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { TagStatus } from "./TagStatus";

describe("TagStatus", () => {
  it("ComChildren_DeveRenderizarOTexto", () => {
    // ============ ARRANGE / ACT ============
    render(<TagStatus tone="success">Vigente</TagStatus>);

    // ============ ASSERT ============
    expect(screen.getByText("Vigente")).toBeInTheDocument();
  });

  it("ComLeadingIconETrailingIcon_DeveRenderizarOsDoisIcones", () => {
    // ============ ARRANGE / ACT ============
    render(
      <TagStatus tone="warning" leadingIcon="warning" trailingIcon="chevron_right">
        Vencido por galonagem
      </TagStatus>,
    );

    // ============ ASSERT ============
    expect(screen.getByText("warning")).toBeInTheDocument();
    expect(screen.getByText("chevron_right")).toBeInTheDocument();
    expect(screen.getByText("Vencido por galonagem")).toBeInTheDocument();
  });
});
