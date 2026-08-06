import type { CSSProperties, HTMLAttributes } from "react";

export interface IconProps extends HTMLAttributes<HTMLSpanElement> {
  name: string;
  size?: number;
  weight?: number;
  fill?: boolean;
  color?: string;
}

/**
 * Icon — thin wrapper over Google Material Symbols (Rounded).
 * The ALE design system uses Material Symbols as its icon language.
 * Requires the Material Symbols Rounded font to be loaded on the page
 * (linked globally in src/app/layout.tsx).
 */
export function Icon({
  name,
  size = 24,
  weight = 400,
  fill = false,
  color = "currentColor",
  style,
  ...rest
}: IconProps) {
  const mergedStyle: CSSProperties = {
    fontSize: size,
    lineHeight: 1,
    color,
    fontVariationSettings: `'FILL' ${fill ? 1 : 0}, 'wght' ${weight}, 'GRAD' 0, 'opsz' 24`,
    userSelect: "none",
    flexShrink: 0,
    ...style,
  };

  return (
    <span className="material-symbols-rounded" aria-hidden="true" style={mergedStyle} {...rest}>
      {name}
    </span>
  );
}
