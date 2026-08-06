import type { CSSProperties } from "react";
import { Icon } from "../core/Icon";

export interface TabItem {
  value: string;
  label?: string;
  icon?: string;
  badge?: string | number;
}

export interface TabsProps {
  tabs?: (TabItem | string)[];
  value?: string;
  onChange?: (value: string) => void;
  style?: CSSProperties;
}

function tabValue(t: TabItem | string): string {
  return typeof t === "string" ? t : t.value;
}

function tabLabel(t: TabItem | string): string {
  return typeof t === "string" ? t : (t.label ?? t.value);
}

/**
 * Tabs — horizontal section switcher with an underline indicator.
 * Active tab is primary with a 2px underline; others are muted.
 */
export function Tabs({ tabs = [], value, onChange, style }: TabsProps) {
  return (
    <div role="tablist" style={{ display: "flex", gap: "var(--space-xs)", borderBottom: "1px solid var(--border-subtle)", ...style }}>
      {tabs.map((t, i) => {
        const val = tabValue(t);
        const label = tabLabel(t);
        const active = val === value;
        const icon = typeof t === "string" ? undefined : t.icon;
        const badge = typeof t === "string" ? undefined : t.badge;

        return (
          <button
            key={i}
            role="tab"
            aria-selected={active}
            onClick={() => onChange && onChange(val)}
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: 6,
              border: "none",
              background: "none",
              cursor: "pointer",
              padding: "0 0 12px",
              marginBottom: -1,
              font: active ? "var(--text-label-lg)" : "var(--text-body-lg)",
              color: active ? "var(--brand-primary)" : "var(--text-secondary)",
              borderBottom: `2px solid ${active ? "var(--brand-primary)" : "transparent"}`,
              transition: "color .15s ease, border-color .15s ease",
            }}
          >
            {icon && <Icon name={icon} size={20} />}
            {label}
            {badge != null && (
              <span
                style={{
                  minWidth: 18,
                  height: 18,
                  paddingInline: 5,
                  borderRadius: "var(--radius-pill)",
                  display: "inline-flex",
                  alignItems: "center",
                  justifyContent: "center",
                  background: active ? "var(--brand-primary)" : "var(--neutral-color-high-medium)",
                  color: active ? "var(--text-on-brand)" : "var(--text-secondary)",
                  font: "var(--text-micro)",
                  fontSize: 11,
                  fontWeight: 700,
                }}
              >
                {badge}
              </span>
            )}
          </button>
        );
      })}
    </div>
  );
}
