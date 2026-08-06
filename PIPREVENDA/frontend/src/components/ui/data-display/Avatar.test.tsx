import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { Avatar } from "./Avatar";

describe("Avatar", () => {
  it("ComNomeCompleto_DeveRenderizarAsDuasPrimeirasIniciais", () => {
    // ============ ARRANGE / ACT ============
    render(<Avatar name="Marina Alves Souza" />);

    // ============ ASSERT ============
    expect(screen.getByText("MA")).toBeInTheDocument();
  });

  it("ComInitialsExplicito_DevePriorizarInitialsSobreNome", () => {
    // ============ ARRANGE / ACT ============
    render(<Avatar name="Marina Alves" initials="XY" />);

    // ============ ASSERT ============
    expect(screen.getByText("XY")).toBeInTheDocument();
    expect(screen.queryByText("MA")).not.toBeInTheDocument();
  });

  it("ComSrc_DeveRenderizarImagemComAltIgualAoNome", () => {
    // ============ ARRANGE / ACT ============
    render(<Avatar name="Marina Alves" src="https://example.com/avatar.png" />);

    // ============ ASSERT ============
    const img = screen.getByRole("img", { name: "Marina Alves" });
    expect(img).toHaveAttribute("src", "https://example.com/avatar.png");
  });

  it("ComStatusOnline_DeveRenderizarUmIndicadorAMaisDoQueSemStatus", () => {
    // ============ ARRANGE / ACT ============
    const { container: withStatus } = render(<Avatar name="Marina Alves" status="online" />);
    const { container: withoutStatus } = render(<Avatar name="Marina Alves" />);

    // ============ ASSERT ============
    expect(withStatus.querySelectorAll("span").length).toBe(withoutStatus.querySelectorAll("span").length + 1);
  });
});
