import { useState, type ButtonHTMLAttributes, type CSSProperties, type ReactNode } from "react";
import { Icon } from "./Icon";

export type ButtonType = "primary" | "secondary" | "tertiary" | "link";
export type ButtonSize = "small" | "medium" | "large";

export interface ButtonProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, "type"> {
  children?: ReactNode;
  type?: ButtonType;
  size?: ButtonSize;
  leadingIcon?: string;
  trailingIcon?: string;
  loading?: boolean;
  disabled?: boolean;
  fullWidth?: boolean;
}

/* Per-size geometry — matches the ALE Figma button set (h32/40/48). */
const SIZES: Record<ButtonSize, { height: number; padX: string; font: string; icon: number }> = {
  small: { height: 32, padX: "var(--space-xxs)", font: "var(--font-size-xs)", icon: 18 },
  medium: { height: 40, padX: "var(--space-xxs)", font: "var(--font-size-xs)", icon: 20 },
  large: { height: 48, padX: "var(--space-xs)", font: "var(--font-size-xs)", icon: 24 },
};

interface PaletteState {
  background?: string;
  color?: string;
  boxShadow?: string;
  paddingLeft?: number;
  paddingRight?: number;
}

interface Palette {
  base: PaletteState;
  hover: PaletteState;
  press: PaletteState;
  disabled: PaletteState;
}

/* Per-type colour roles. Hover/press are handled with inline event state. */
function palette(type: ButtonType): Palette {
  switch (type) {
    case "secondary":
      return {
        base: {
          background: "transparent",
          color: "var(--brand-primary)",
          boxShadow: "inset 0 0 0 1px var(--brand-primary)",
        },
        hover: { background: "var(--brand-primary-soft)" },
        press: { background: "var(--primary-color-primary-light)" },
        disabled: {
          background: "transparent",
          color: "var(--text-disabled)",
          boxShadow: "inset 0 0 0 1px var(--border-default)",
        },
      };
    case "tertiary":
      return {
        base: { background: "var(--brand-primary-soft)", color: "var(--brand-primary)" },
        hover: { background: "var(--primary-color-primary-light)" },
        press: { background: "var(--primary-color-primary-light)", color: "var(--brand-primary-press)" },
        disabled: { background: "var(--surface-disabled)", color: "var(--text-disabled)" },
      };
    case "link":
      return {
        base: {
          background: "transparent",
          color: "var(--brand-primary)",
          paddingLeft: 0,
          paddingRight: 0,
        },
        hover: { color: "var(--brand-primary-hover)" },
        press: { color: "var(--brand-primary-press)" },
        disabled: { background: "transparent", color: "var(--text-disabled)" },
      };
    default:
      /* primary */
      return {
        base: { background: "var(--brand-primary)", color: "var(--text-on-brand)" },
        hover: { background: "var(--brand-primary-hover)" },
        press: { background: "var(--brand-primary-press)" },
        disabled: { background: "var(--surface-disabled)", color: "var(--text-disabled)" },
      };
  }
}

/**
 * Button — the primary ALE action control.
 * Types: primary | secondary | tertiary | link. Sizes: small | medium | large.
 * Supports leading/trailing icons (Material Symbol names), loading and disabled.
 */
export function Button({
  children,
  type = "primary",
  size = "medium",
  leadingIcon,
  trailingIcon,
  loading = false,
  disabled = false,
  fullWidth = false,
  onClick,
  style,
  ...rest
}: ButtonProps) {
  const [hover, setHover] = useState(false);
  const [press, setPress] = useState(false);
  const s = SIZES[size] || SIZES.medium;
  const p = palette(type);
  const isDisabled = disabled || loading;
  const stateStyle = isDisabled ? p.disabled : press ? { ...p.hover, ...p.press } : hover ? p.hover : null;

  const mergedStyle: CSSProperties = {
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    gap: "var(--space-nano)",
    height: s.height,
    paddingInline: type === "link" ? 0 : s.padX,
    width: fullWidth ? "100%" : undefined,
    border: "none",
    borderRadius: "var(--radius-sm)",
    font: "var(--text-label-lg)",
    fontSize: s.font,
    whiteSpace: "nowrap",
    cursor: isDisabled ? "not-allowed" : "pointer",
    transition: "background .15s ease, color .15s ease, box-shadow .15s ease",
    ...p.base,
    ...stateStyle,
    ...style,
  };

  return (
    <button
      type="button"
      disabled={isDisabled}
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => {
        setHover(false);
        setPress(false);
      }}
      onMouseDown={() => setPress(true)}
      onMouseUp={() => setPress(false)}
      style={mergedStyle}
      {...rest}
    >
      {loading ? (
        <Icon name="progress_activity" size={s.icon} style={{ animation: "ale-spin 1s linear infinite" }} />
      ) : (
        <>
          {leadingIcon && <Icon name={leadingIcon} size={s.icon} />}
          {children}
          {trailingIcon && <Icon name={trailingIcon} size={s.icon} />}
        </>
      )}
      <style>{`@keyframes ale-spin{to{transform:rotate(360deg)}}`}</style>
    </button>
  );
}
