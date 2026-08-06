import type { CSSProperties } from "react";

export type AvatarSize = "sm" | "md" | "lg" | "xl";
export type AvatarStatus = "online" | "busy" | "away";

export interface AvatarProps {
  src?: string;
  name?: string;
  initials?: string;
  size?: AvatarSize;
  status?: AvatarStatus;
  style?: CSSProperties;
}

const SIZES: Record<AvatarSize, number> = { sm: 32, md: 40, lg: 48, xl: 64 };

/**
 * Avatar — circular user mark. Shows an image when `src` is given, otherwise
 * the `initials`/`name` on a primary fill. Optional status dot.
 */
export function Avatar({ src, name, initials, size = "md", status, style }: AvatarProps) {
  const px = SIZES[size] || SIZES.md;
  const label =
    initials ||
    (name
      ? name
          .trim()
          .split(/\s+/)
          .slice(0, 2)
          .map((w) => w[0])
          .join("")
          .toUpperCase()
      : "");
  const statusColor =
    status === "online"
      ? "var(--feedback-success)"
      : status === "busy"
        ? "var(--brand-secondary)"
        : status === "away"
          ? "var(--brand-tertiary)"
          : null;

  return (
    <span style={{ position: "relative", display: "inline-flex", flexShrink: 0, ...style }}>
      <span
        style={{
          display: "inline-flex",
          alignItems: "center",
          justifyContent: "center",
          width: px,
          height: px,
          borderRadius: "50%",
          overflow: "hidden",
          background: "var(--brand-primary)",
          color: "var(--text-on-brand)",
          font: "var(--text-label-lg)",
          fontSize: px * 0.4,
        }}
      >
        {src ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={src} alt={name || ""} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
        ) : (
          label
        )}
      </span>
      {statusColor && (
        <span
          style={{
            position: "absolute",
            right: 0,
            bottom: 0,
            width: px * 0.28,
            height: px * 0.28,
            borderRadius: "50%",
            background: statusColor,
            boxShadow: "0 0 0 2px var(--surface-card)",
          }}
        />
      )}
    </span>
  );
}
