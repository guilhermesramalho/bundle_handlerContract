import { useState, type CSSProperties, type MouseEventHandler, type ReactNode } from "react";
import { Icon } from "../core/Icon";

export interface ChipProps {
  children?: ReactNode;
  selected?: boolean;
  disabled?: boolean;
  leadingIcon?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  style?: CSSProperties;
}

/**
 * Chip — a compact, toggleable filter/choice. Rest = outlined; selected =
 * primary fill with a check. Use in filter bars and multi-select groups.
 */
export function Chip({ children, selected = false, disabled = false, leadingIcon, onClick, style }: ChipProps) {
  const [hover, setHover] = useState(false);

  let bg = "transparent";
  let fg = "var(--brand-primary)";
  let ring = "inset 0 0 0 1px var(--brand-primary)";
  if (selected) {
    bg = "var(--brand-primary)";
    fg = "var(--text-on-brand)";
    ring = "none";
  } else if (hover && !disabled) {
    bg = "var(--brand-primary-soft)";
  }
  if (disabled) {
    bg = "var(--surface-disabled)";
    fg = "var(--text-disabled)";
    ring = "none";
  }

  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={{
        display: "inline-flex",
        alignItems: "center",
        gap: 6,
        height: 36,
        paddingInline: "var(--space-xxxs)",
        border: "none",
        borderRadius: "var(--radius-md)",
        boxShadow: ring,
        background: bg,
        color: fg,
        font: "var(--text-label-lg)",
        cursor: disabled ? "not-allowed" : "pointer",
        transition: "background .15s ease",
        whiteSpace: "nowrap",
        ...style,
      }}
    >
      {leadingIcon && <Icon name={leadingIcon} size={18} />}
      {children}
      {selected && <Icon name="check_circle" size={18} fill />}
    </button>
  );
}
