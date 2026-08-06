import { useId, type CSSProperties } from "react";

export interface RadioProps {
  checked?: boolean;
  onChange?: (value: string) => void;
  label?: string;
  name?: string;
  value?: string;
  disabled?: boolean;
  id?: string;
  style?: CSSProperties;
}

/**
 * Radio — single option within a group. Selected shows a primary ring with
 * a filled centre dot. Use one shared `name` per group.
 */
export function Radio({ checked = false, onChange, label, name, value, disabled = false, id, style }: RadioProps) {
  const generatedId = useId();
  const rId = id || generatedId;

  return (
    <label
      htmlFor={rId}
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
        id={rId}
        type="radio"
        name={name}
        value={value}
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange && onChange(e.target.value)}
        style={{ position: "absolute", opacity: 0, width: 0, height: 0 }}
      />
      <span
        style={{
          display: "inline-flex",
          alignItems: "center",
          justifyContent: "center",
          width: 20,
          height: 20,
          borderRadius: "50%",
          flexShrink: 0,
          background: "var(--surface-card)",
          boxShadow: `inset 0 0 0 2px ${checked ? "var(--brand-primary)" : "var(--border-strong)"}`,
          transition: "box-shadow .15s ease",
        }}
      >
        {checked && <span style={{ width: 10, height: 10, borderRadius: "50%", background: "var(--brand-primary)" }} />}
      </span>
      {label && <span style={{ font: "var(--text-body-lg)", color: "var(--text-primary)" }}>{label}</span>}
    </label>
  );
}
