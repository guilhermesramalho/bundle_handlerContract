import { useId, useState, type CSSProperties, type InputHTMLAttributes } from "react";
import { Icon } from "../core/Icon";

export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "id" | "style"> {
  label?: string;
  optional?: boolean;
  helperText?: string;
  error?: boolean;
  leadingIcon?: string;
  trailingIcon?: string;
  id?: string;
  style?: CSSProperties;
}

/**
 * Input — labelled text field with helper text and error state.
 * States: rest, focus (blue ring), filled, error (red), disabled.
 */
export function Input({
  label,
  optional = false,
  placeholder = "Digite aqui...",
  helperText,
  error = false,
  disabled = false,
  leadingIcon,
  trailingIcon,
  value,
  onChange,
  id,
  style,
  ...rest
}: InputProps) {
  const [focus, setFocus] = useState(false);
  const generatedId = useId();
  const inputId = id || generatedId;

  let borderColor = "var(--border-default)";
  if (disabled) borderColor = "var(--border-subtle)";
  else if (error) borderColor = "var(--feedback-error)";
  else if (focus) borderColor = "var(--border-focus)";

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "var(--space-nano)", ...style }}>
      {label && (
        <label
          htmlFor={inputId}
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
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "var(--space-nano)",
          height: 48,
          paddingInline: "var(--space-xxxs)",
          background: disabled ? "var(--surface-disabled)" : "var(--surface-card)",
          border: `1px solid ${borderColor}`,
          boxShadow: focus && !error && !disabled ? "var(--shadow-focus-ring)" : "none",
          borderRadius: "var(--radius-md)",
          transition: "border-color .15s ease, box-shadow .15s ease",
        }}
      >
        {leadingIcon && <Icon name={leadingIcon} size={20} color="var(--text-tertiary)" />}
        <input
          id={inputId}
          value={value}
          onChange={onChange}
          disabled={disabled}
          placeholder={placeholder}
          onFocus={() => setFocus(true)}
          onBlur={() => setFocus(false)}
          style={{
            flex: 1,
            minWidth: 0,
            border: "none",
            outline: "none",
            background: "transparent",
            font: "var(--text-body-lg)",
            color: "var(--text-primary)",
          }}
          {...rest}
        />
        {trailingIcon && <Icon name={trailingIcon} size={20} color={error ? "var(--feedback-error)" : "var(--text-tertiary)"} />}
      </div>
      {helperText && (
        <span style={{ font: "var(--text-caption)", color: error ? "var(--feedback-error)" : "var(--text-secondary)" }}>
          {helperText}
        </span>
      )}
    </div>
  );
}
