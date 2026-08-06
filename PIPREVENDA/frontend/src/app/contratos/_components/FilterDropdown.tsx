"use client";

import { useEffect, useRef, useState } from "react";
import { Icon } from "@/components/ui";
import styles from "./FilterDropdown.module.css";

export interface FilterDropdownProps {
  label: string;
  placeholder: string;
  options: string[];
  selected: string[];
  /** `false` = seleção única (com opção explícita para limpar). `true` = seleção múltipla (checkbox). */
  multiple?: boolean;
  onChange: (next: string[]) => void;
  /** Rótulo amigável opcional — o valor real (`options`/`selected`) continua sendo a string crua (ex.: enum da API); só o texto exibido muda. */
  renderLabel?: (value: string) => string;
}

/**
 * Dropdown de filtro fiel ao padrão visual/interação do protótipo
 * (trigger + lista posicionada absolutamente, fecha ao clicar fora).
 * Usado tanto para os filtros simples/múltiplos quanto para os 4 níveis da
 * cascata de hierarquia comercial na Listagem de Contratos.
 */
export function FilterDropdown({
  label,
  placeholder,
  options,
  selected,
  multiple = true,
  onChange,
  renderLabel = (value) => value,
}: FilterDropdownProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function onDocClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", onDocClick);
    return () => document.removeEventListener("mousedown", onDocClick);
  }, []);

  const active = selected.length > 0;
  const currentLabel = !active ? placeholder : selected.length === 1 ? renderLabel(selected[0]) : `${selected.length} selecionadas`;

  function toggleValue(value: string) {
    if (multiple) {
      const has = selected.includes(value);
      onChange(has ? selected.filter((v) => v !== value) : [...selected, value]);
    } else {
      onChange([value]);
      setOpen(false);
    }
  }

  function clear() {
    onChange([]);
    setOpen(false);
  }

  return (
    <div className={styles.wrap} ref={ref}>
      <label className={styles.label}>{label}</label>
      <button
        type="button"
        className={`${styles.trigger} ${active ? styles.triggerActive : ""}`}
        onClick={() => setOpen((o) => !o)}
        aria-expanded={open}
        aria-label={`Filtro ${label}`}
      >
        <span className={styles.triggerText}>{currentLabel}</span>
        <Icon name={open ? "expand_less" : "expand_more"} size={20} color="var(--text-secondary)" />
      </button>
      {open && (
        <ul className={styles.menu} role="listbox" aria-label={label} aria-multiselectable={multiple}>
          {!multiple && (
            <li>
              <button type="button" className={styles.option} onClick={clear}>
                <span className={styles.optionLabel}>{placeholder}</span>
                {!active && <Icon name="check" size={18} color="var(--brand-primary)" />}
              </button>
            </li>
          )}
          {options.map((opt) => {
            const isSel = selected.includes(opt);
            return (
              <li key={opt}>
                <button
                  type="button"
                  role="option"
                  aria-selected={isSel}
                  className={`${styles.option} ${isSel && !multiple ? styles.optionSelected : ""}`}
                  onClick={() => toggleValue(opt)}
                >
                  <span className={styles.optionLabel}>
                    {multiple && (
                      <Icon
                        name={isSel ? "check_box" : "check_box_outline_blank"}
                        size={19}
                        color={isSel ? "var(--brand-primary)" : "var(--text-tertiary)"}
                      />
                    )}
                    {renderLabel(opt)}
                  </span>
                  {!multiple && isSel && <Icon name="check" size={18} color="var(--brand-primary)" />}
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
