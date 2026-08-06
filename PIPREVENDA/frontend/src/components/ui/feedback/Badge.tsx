import type { CSSProperties, ReactNode } from "react";

export type BadgeTone = "tertiary" | "primary" | "error" | "success" | "neutral";

export interface BadgeProps {
  count?: number;
  max?: number;
  dot?: boolean;
  tone?: BadgeTone;
  style?: CSSProperties;
  children?: ReactNode;
}

/* Tone → {bg, fg}. Badge uses solid, saturated fills (number badges). */
const TONES: Record<BadgeTone, { bg: string; fg: string }> = {
  tertiary: { bg: "var(--terciary-color-terciary-pure)", fg: "var(--neutral-color-low-dark)" },
  primary: { bg: "var(--brand-primary)", fg: "var(--text-on-brand)" },
  error: { bg: "var(--feedback-error)", fg: "var(--text-on-brand)" },
  success: { bg: "var(--feedback-success)", fg: "var(--text-on-brand)" },
  neutral: { bg: "var(--neutral-color-low-dark)", fg: "var(--text-on-brand)" },
};

/**
 * Badge — a small count or status dot, typically anchored to an icon/avatar.
 * `dot` renders a 8px indicator; otherwise shows `count` (capped by `max`).
 */
export function Badge({ count, max = 99, dot = false, tone = "tertiary", style, children }: BadgeProps) {
  const t = TONES[tone] || TONES.tertiary;

  if (dot) {
    return (
      <span
        style={{
          display: "inline-block",
          width: 8,
          height: 8,
          borderRadius: "50%",
          background: t.bg,
          ...style,
        }}
      />
    );
  }

  const display = children ?? (typeof count === "number" && count > max ? `${max}+` : count);

  return (
    <span
      style={{
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "center",
        minWidth: 20,
        height: 20,
        paddingInline: 6,
        borderRadius: "var(--radius-pill)",
        background: t.bg,
        color: t.fg,
        font: "var(--text-micro)",
        fontWeight: 700,
        fontSize: 11,
        ...style,
      }}
    >
      {display}
    </span>
  );
}
