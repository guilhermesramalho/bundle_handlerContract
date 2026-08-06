import { useId, type CSSProperties } from "react";
import { Icon } from "../core/Icon";

export interface CheckboxProps {
  checked?: boolean;
  indeterminate?: boolean;
  onChange?: (checked: boolean) => void;
  label?: string;
  disabled?: boolean;
  id?: string;
  style?: CSSProperties;
}

/**
 * Checkbox — boolean control with optional label. Checked fills with
 * primary-dark and a white check; supports indeterminate and disabled.
 */
export function Checkbox({ checked = false, indeterminate = false, onChange, label, disabled = false, id, style }: CheckboxProps) {
  const generatedId = useId();
  const cbId = id || generatedId;
  const on = checked || indeterminate;

  return (
    <label
      htmlFor={cbId}
      style={{
        display: "inline-flex",
        alignItems: "center",
        gap: "var(--space-nano)",
        cursor: disabled ? "not-allowed" : "pointer",
        opacity: disabled ? 0.5 : 1,
        ...style,
      }}
    >
      <input
        id={cbId}
        type="checkbox"
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange && onChange(e.target.checked)}
        style={{ position: "absolute", opacity: 0, width: 0, height: 0 }}
      />
      <span
        style={{
          display: "inline-flex",
          alignItems: "center",
          justifyContent: "center",
          width: 20,
          height: 20,
          borderRadius: "var(--radius-sm)",
          flexShrink: 0,
          background: on ? "var(--brand-primary-press)" : "var(--surface-card)",
          boxShadow: on ? "none" : "inset 0 0 0 2px var(--border-strong)",
          transition: "background .15s ease",
        }}
      >
        {indeterminate ? (
          <Icon name="remove" size={16} color="var(--text-on-brand)" />
        ) : (
          checked && <Icon name="check" size={16} color="var(--text-on-brand)" />
        )}
      </span>
      {label && <span style={{ font: "var(--text-body-lg)", color: "var(--text-primary)" }}>{label}</span>}
    </label>
  );
}
