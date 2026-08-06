import { Fragment, type CSSProperties } from "react";
import { Icon } from "../core/Icon";

export interface BreadcrumbItem {
  label: string;
}

export interface BreadcrumbsProps {
  items?: (BreadcrumbItem | string)[];
  onNavigate?: (item: BreadcrumbItem | string, index: number) => void;
  style?: CSSProperties;
}

function itemLabel(it: BreadcrumbItem | string): string {
  return typeof it === "string" ? it : it.label;
}

/**
 * Breadcrumbs — hierarchical path. Ancestors are muted links; the last
 * item is the current page (primary, bold, non-interactive).
 */
export function Breadcrumbs({ items = [], onNavigate, style }: BreadcrumbsProps) {
  return (
    <nav aria-label="breadcrumb" style={{ display: "flex", alignItems: "center", flexWrap: "wrap", gap: "var(--space-nano)", ...style }}>
      {items.map((it, i) => {
        const isLast = i === items.length - 1;
        const label = itemLabel(it);
        return (
          <Fragment key={i}>
            {isLast ? (
              <span aria-current="page" style={{ font: "var(--text-label-lg)", color: "var(--brand-primary)" }}>
                {label}
              </span>
            ) : (
              <button
                type="button"
                onClick={() => onNavigate && onNavigate(it, i)}
                style={{
                  border: "none",
                  background: "none",
                  padding: 0,
                  cursor: "pointer",
                  font: "var(--text-body-lg)",
                  color: "var(--text-secondary)",
                }}
              >
                {label}
              </button>
            )}
            {!isLast && <Icon name="chevron_right" size={18} color="var(--text-tertiary)" />}
          </Fragment>
        );
      })}
    </nav>
  );
}
