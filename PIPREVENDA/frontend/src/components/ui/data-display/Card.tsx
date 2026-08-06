import { useState, type CSSProperties, type MouseEventHandler, type ReactNode } from "react";

export interface CardProps {
  children?: ReactNode;
  outlined?: boolean;
  padding?: string;
  interactive?: boolean;
  onClick?: MouseEventHandler<HTMLDivElement>;
  style?: CSSProperties;
}

/**
 * Card — the ALE surface container. Subtle elevation by default; `outlined`
 * swaps shadow for a hairline border. Use as the base for list rows, panels
 * and dashboard tiles.
 */
export function Card({ children, outlined = false, padding = "var(--space-xxs)", interactive = false, onClick, style }: CardProps) {
  const [hover, setHover] = useState(false);

  return (
    <div
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={{
        background: "var(--surface-card)",
        borderRadius: "var(--radius-lg)",
        border: outlined ? "1px solid var(--border-subtle)" : "none",
        boxShadow: outlined ? "none" : interactive && hover ? "var(--shadow-md)" : "var(--shadow-sm)",
        padding,
        cursor: interactive ? "pointer" : "default",
        transition: "box-shadow .15s ease, transform .15s ease",
        transform: interactive && hover ? "translateY(-2px)" : "none",
        ...style,
      }}
    >
      {children}
    </div>
  );
}
