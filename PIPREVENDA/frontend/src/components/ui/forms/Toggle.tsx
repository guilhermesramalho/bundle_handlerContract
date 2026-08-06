import { useId, type CSSProperties } from "react";

export interface ToggleProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  label?: string;
  disabled?: boolean;
  id?: string;
  style?: CSSProperties;
}

/**
 * Toggle — on/off switch. On = primary blue track; off = neutral grey.
 * The knob slides; the whole control is clickable with its label.
 */
export function Toggle({ checked = false, onChange, label, disabled = false, id, style }: ToggleProps) {
  const generatedId = useId();
  const tId = id || generatedId;

  return (
    <label
      htmlFor={tId}
      style={{
        display: "inline-flex",
        alignItems: "center",
        gap: "var(--space-xxxs)",
        cursor: disabled ? "not-allowed" : "pointer",
        opacity: disabled ? 0.5 : 1,
        ...style,
      }}
    >
      <input
        id={tId}
        type="checkbox"
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange && onChange(e.target.checked)}
        style={{ position: "absolute", opacity: 0, width: 0, height: 0 }}
      />
      <span
        style={{
          position: "relative",
          width: 48,
          height: 28,
          borderRadius: "var(--radius-pill)",
          flexShrink: 0,
          background: checked ? "var(--brand-primary)" : "var(--neutral-color-low-medium)",
          transition: "background .2s ease",
        }}
      >
        <span
          style={{
            position: "absolute",
            top: 4,
            left: checked ? 24 : 4,
            width: 20,
            height: 20,
            borderRadius: "50%",
            background: "var(--surface-card)",
            boxShadow: "var(--shadow-xs)",
            transition: "left .2s ease",
          }}
        />
      </span>
      {label && <span style={{ font: "var(--text-body-lg)", color: "var(--text-primary)" }}>{label}</span>}
    </label>
  );
}
