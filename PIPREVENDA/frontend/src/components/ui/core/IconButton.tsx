import { useState, type ButtonHTMLAttributes, type CSSProperties } from "react";
import { Icon } from "./Icon";

export type IconButtonType = "primary" | "secondary" | "tertiary" | "ghost";
export type IconButtonSize = "small" | "medium" | "large";

export interface IconButtonProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, "type"> {
  icon: string;
  type?: IconButtonType;
  size?: IconButtonSize;
  disabled?: boolean;
  rounded?: boolean;
  "aria-label"?: string;
}

const SIZES: Record<IconButtonSize, { box: number; icon: number }> = {
  small: { box: 32, icon: 18 },
  medium: { box: 40, icon: 20 },
  large: { box: 48, icon: 24 },
};

interface PaletteState {
  background?: string;
  color?: string;
  boxShadow?: string;
}

interface Palette {
  base: PaletteState;
  hover: PaletteState;
  disabled: PaletteState;
}

function palette(type: IconButtonType): Palette {
  switch (type) {
    case "secondary":
      return {
        base: {
          background: "transparent",
          color: "var(--brand-primary)",
          boxShadow: "inset 0 0 0 1px var(--brand-primary)",
        },
        hover: { background: "var(--brand-primary-soft)" },
        disabled: { color: "var(--text-disabled)", boxShadow: "inset 0 0 0 1px var(--border-default)" },
      };
    case "tertiary":
      return {
        base: { background: "var(--brand-primary-soft)", color: "var(--brand-primary)" },
        hover: { background: "var(--primary-color-primary-light)" },
        disabled: { background: "var(--surface-disabled)", color: "var(--text-disabled)" },
      };
    case "ghost":
      return {
        base: { background: "transparent", color: "var(--text-secondary)" },
        hover: { background: "var(--neutral-color-high-light)", color: "var(--text-primary)" },
        disabled: { background: "transparent", color: "var(--text-disabled)" },
      };
    default:
      return {
        base: { background: "var(--brand-primary)", color: "var(--text-on-brand)" },
        hover: { background: "var(--brand-primary-hover)" },
        disabled: { background: "var(--surface-disabled)", color: "var(--text-disabled)" },
      };
  }
}

/**
 * IconButton — a square, icon-only action. Same type/size system as Button,
 * plus a low-emphasis `ghost` type for toolbars and table rows.
 */
export function IconButton({
  icon,
  type = "primary",
  size = "medium",
  disabled = false,
  rounded = false,
  onClick,
  "aria-label": ariaLabel,
  style,
  ...rest
}: IconButtonProps) {
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
    borderRadius: rounded ? "var(--radius-pill)" : "var(--radius-sm)",
    cursor: disabled ? "not-allowed" : "pointer",
    transition: "background .15s ease, color .15s ease, box-shadow .15s ease",
    ...p.base,
    ...(disabled ? p.disabled : hover ? p.hover : null),
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
