import { useEffect, useId, useRef, useState, type CSSProperties } from "react";
import { Icon } from "../core/Icon";

export interface SelectOption {
  value: string;
  label?: string;
}

export interface SelectProps {
  label?: string;
  optional?: boolean;
  placeholder?: string;
  options?: (SelectOption | string)[];
  value?: string;
  onChange?: (value: string) => void;
  helperText?: string;
  error?: boolean;
  disabled?: boolean;
  id?: string;
  style?: CSSProperties;
}

function optionValue(o: SelectOption | string): string {
  return typeof o === "string" ? o : o.value;
}

function optionLabel(o: SelectOption | string): string {
  return typeof o === "string" ? o : (o.label ?? o.value);
}

/**
 * Select — labelled dropdown field. Mirrors Input styling; the menu is a
 * native-feel popover of options. Controlled via `value` / `onChange(value)`.
 */
export function Select({
  label,
  optional = false,
  placeholder = "Selecione",
  options = [],
  value,
  onChange,
  helperText,
  error = false,
  disabled = false,
  id,
  style,
}: SelectProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const generatedId = useId();
  const selectId = id || generatedId;
  const selected = options.find((o) => optionValue(o) === value);

  useEffect(() => {
    function onDoc(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", onDoc);
    return () => document.removeEventListener("mousedown", onDoc);
  }, []);

  let borderColor = "var(--border-default)";
  if (disabled) borderColor = "var(--border-subtle)";
  else if (error) borderColor = "var(--feedback-error)";
  else if (open) borderColor = "var(--border-focus)";

  return (
    <div ref={ref} style={{ display: "flex", flexDirection: "column", gap: "var(--space-nano)", position: "relative", ...style }}>
      {label && (
        <label
          htmlFor={selectId}
          style={{
            font: "var(--text-label-md)",
            color: error ? "var(--feedback-error)" : "var(--text-primary)",
            display: "inline-flex",
            gap: 6,
            alignItems: "baseline",
          }}
        >
          {label}
          {optional && <span style={{ font: "var(--text-caption)", color: "var(--text-tertiary)" }}>(Opcional)</span>}
        </label>
      )}
      <button
        id={selectId}
        type="button"
        disabled={disabled}
        onClick={() => setOpen((o) => !o)}
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: "var(--space-nano)",
          height: 48,
          paddingInline: "var(--space-xxxs)",
          width: "100%",
          background: disabled ? "var(--surface-disabled)" : "var(--surface-card)",
          border: `1px solid ${borderColor}`,
          boxShadow: open && !error && !disabled ? "var(--shadow-focus-ring)" : "none",
          borderRadius: "var(--radius-md)",
          cursor: disabled ? "not-allowed" : "pointer",
          font: "var(--text-body-lg)",
          color: selected ? "var(--text-primary)" : "var(--text-tertiary)",
          textAlign: "left",
          transition: "border-color .15s ease, box-shadow .15s ease",
        }}
      >
        <span style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
          {selected ? optionLabel(selected) : placeholder}
        </span>
        <Icon name={open ? "expand_less" : "expand_more"} size={20} color="var(--text-secondary)" />
      </button>
      {open && !disabled && (
        <ul
          style={{
            listStyle: "none",
            margin: 0,
            padding: "var(--space-quarck)",
            position: "absolute",
            top: "calc(100% + 4px)",
            left: 0,
            right: 0,
            zIndex: 20,
            background: "var(--surface-card)",
            border: "1px solid var(--border-subtle)",
            borderRadius: "var(--radius-md)",
            boxShadow: "var(--shadow-lg)",
            maxHeight: 240,
            overflowY: "auto",
          }}
        >
          {options.map((o, i) => {
            const val = optionValue(o);
            const lbl = optionLabel(o);
            const isSel = val === value;
            return (
              <li key={i}>
                <button
                  type="button"
                  onClick={() => {
                    onChange?.(val);
                    setOpen(false);
                  }}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                    width: "100%",
                    padding: "10px 12px",
                    border: "none",
                    borderRadius: "var(--radius-sm)",
                    cursor: "pointer",
                    background: isSel ? "var(--brand-primary-soft)" : "transparent",
                    font: "var(--text-body-lg)",
                    color: "var(--text-primary)",
                    textAlign: "left",
                  }}
                  onMouseEnter={(e) => {
                    if (!isSel) e.currentTarget.style.background = "var(--surface-page)";
                  }}
                  onMouseLeave={(e) => {
                    if (!isSel) e.currentTarget.style.background = "transparent";
                  }}
                >
                  {lbl}
                  {isSel && <Icon name="check" size={18} color="var(--brand-primary)" />}
                </button>
              </li>
            );
          })}
        </ul>
      )}
      {helperText && (
        <span style={{ font: "var(--text-caption)", color: error ? "var(--feedback-error)" : "var(--text-secondary)" }}>
          {helperText}
        </span>
      )}
    </div>
  );
}
