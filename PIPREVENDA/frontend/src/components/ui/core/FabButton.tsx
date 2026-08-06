import { useState, type ButtonHTMLAttributes, type CSSProperties } from "react";
import { Icon } from "./Icon";

export type FabButtonType = "primary" | "secondary" | "tertiary";
export type FabButtonSize = "small" | "medium" | "large";

export interface FabButtonProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, "type"> {
  icon: string;
  type?: FabButtonType;
  size?: FabButtonSize;
  disabled?: boolean;
  "aria-label"?: string;
}

const SIZES: Record<FabButtonSize, { box: number; icon: number }> = {
  small: { box: 40, icon: 20 },
  medium: { box: 56, icon: 24 },
  large: { box: 64, icon: 28 },
};

interface PaletteState {
  background?: string;
  color?: string;
  boxShadow?: string;
}

function palette(type: FabButtonType): { base: PaletteState; hover: PaletteState } {
  switch (type) {
    case "secondary":
      return {
        base: {
          background: "var(--surface-card)",
          color: "var(--brand-primary)",
          boxShadow: "inset 0 0 0 1px var(--brand-primary), var(--shadow-md)",
        },
        hover: { background: "var(--brand-primary-soft)" },
      };
    case "tertiary":
      return {
        base: { background: "var(--brand-primary-soft)", color: "var(--brand-primary)", boxShadow: "var(--shadow-md)" },
        hover: { background: "var(--primary-color-primary-light)" },
      };
    default:
      return {
        base: { background: "var(--brand-primary)", color: "var(--text-on-brand)", boxShadow: "var(--shadow-md)" },
        hover: { background: "var(--brand-primary-hover)" },
      };
  }
}

/**
 * FAB (Floating Action Button) — a circular, elevated primary action,
 * typically pinned to a corner of the viewport.
 */
export function FabButton({
  icon,
  type = "primary",
  size = "medium",
  disabled = false,
  onClick,
  "aria-label": ariaLabel,
  style,
  ...rest
}: FabButtonProps) {
  const [hover, setHover] = useState(false);
  const s = SIZES[size] || SIZES.medium;
  const p = palette(type);

  const mergedStyle: CSSProperties = {
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    width: s.box,
    height: s.box,
    border: "none",
    borderRadius: "var(--radius-circular)",
    cursor: disabled ? "not-allowed" : "pointer",
    opacity: disabled ? 0.4 : 1,
    transition: "background .15s ease, transform .1s ease",
    ...p.base,
    ...(!disabled && hover ? p.hover : null),
    ...style,
  };

  return (
    <button
      type="button"
      aria-label={ariaLabel}
      disabled={disabled}
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={mergedStyle}
      {...rest}
    >
      <Icon name={icon} size={s.icon} />
    </button>
  );
}
