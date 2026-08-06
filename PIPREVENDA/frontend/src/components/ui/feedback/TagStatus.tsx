import type { CSSProperties, ReactNode } from "react";
import { Icon } from "../core/Icon";

export type TagStatusTone =
  | "info"
  | "error"
  | "warning"
  | "success"
  | "pink"
  | "orange"
  | "violet"
  | "cian"
  | "neutral";

export interface TagStatusProps {
  children?: ReactNode;
  tone?: TagStatusTone;
  leadingIcon?: string;
  trailingIcon?: string;
  style?: CSSProperties;
}

/* Soft pill: light tint background + saturated text/icon. Maps to the
   ALE semantic + auxiliary palettes. */
const TONES: Record<TagStatusTone, { bg: string; fg: string }> = {
  info: { bg: "var(--primary-color-primary-lighter)", fg: "var(--primary-color-primary-pure)" },
  error: { bg: "var(--secondary-color-secondary-lighter)", fg: "var(--secondary-color-secondary-pure)" },
  warning: { bg: "var(--helper-color-warning-warning-light)", fg: "var(--terciary-color-terciary-dark)" },
  success: { bg: "var(--helper-color-success-success-light)", fg: "var(--helper-color-success-success-pure)" },
  pink: { bg: "var(--helper-color-error-error-light)", fg: "var(--helper-color-error-error-dark)" },
  orange: { bg: "var(--auxiliar-color-orange-auxiliar-orange-light)", fg: "var(--auxiliar-color-orange-auxiliar-orange-dark)" },
  violet: { bg: "var(--auxiliar-color-violet-auxiliar-violet-light)", fg: "var(--auxiliar-color-violet-auxiliar-violet-medium)" },
  cian: { bg: "var(--auxiliar-color-cian-auxiliar-cian-light)", fg: "var(--auxiliar-color-cian-auxiliar-cian-dark)" },
  neutral: { bg: "var(--neutral-color-high-medium)", fg: "var(--neutral-color-low-medium)" },
};

/**
 * TagStatus — a soft, pill-shaped status label with an optional leading
 * and/or trailing Material Symbol. Tones map to the semantic + auxiliary palette.
 */
export function TagStatus({ children, tone = "info", leadingIcon, trailingIcon, style }: TagStatusProps) {
  const t = TONES[tone] || TONES.info;

  return (
    <span
      style={{
        display: "inline-flex",
        alignItems: "center",
        gap: 6,
        height: 28,
        paddingInline: "var(--space-xxxxs)",
        borderRadius: "var(--radius-pill)",
        background: t.bg,
        color: t.fg,
        font: "var(--text-label-md)",
        whiteSpace: "nowrap",
        ...style,
      }}
    >
      {leadingIcon && <Icon name={leadingIcon} size={16} />}
      {children}
      {trailingIcon && <Icon name={trailingIcon} size={16} />}
    </span>
  );
}
