"use client";

import { useEffect, useRef, useState, type FormEvent, type ReactNode } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Avatar, Icon, IconButton, Input } from "@/components/ui";
import styles from "./AppShell.module.css";

const NAV_ITEMS = [
  { href: "/contratos", label: "Contratos", icon: "description" },
  { href: "/dashboard", label: "Dashboard", icon: "dashboard" },
];

function NotificationsDropdown() {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function onDoc(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", onDoc);
    return () => document.removeEventListener("mousedown", onDoc);
  }, []);

  return (
    <div className={styles.notifWrap} ref={ref}>
      <IconButton icon="notifications" type="ghost" aria-label="Notificações" onClick={() => setOpen((o) => !o)} />
      {open && (
        <div className={styles.notifPanel} role="menu">
          <span className={styles.notifTitle}>Notificações</span>
          {/* Notificações reais dependem da integração com o Elaw (aba
              Jurídico) — ver skill integracao-elaw. Sem dado mock aqui para
              não sugerir uma fonte que ainda não existe. */}
          <span className={styles.notifEmpty}>Nenhuma notificação no momento.</span>
        </div>
      )}
    </div>
  );
}

function GlobalSearch() {
  const router = useRouter();
  const [query, setQuery] = useState("");

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!query.trim()) return;
    router.push(`/contratos?q=${encodeURIComponent(query.trim())}`);
  }

  return (
    <form className={styles.search} onSubmit={handleSubmit}>
      <Input
        placeholder="Buscar CNPJ, nº SAP, grupo econômico..."
        leadingIcon="search"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        aria-label="Busca global de contratos"
      />
    </form>
  );
}

/**
 * Shell da aplicação (Fase F1): navegação, busca global e notificações.
 * Sem autenticação — Azure AD B2C ainda não implementado (Fase 4 do
 * PLANO-IMPLEMENTACAO.md). O bloco de usuário abaixo é um placeholder
 * explícito, não um login simulado.
 */
export function AppShell({ children }: { children: ReactNode }) {
  const pathname = usePathname();

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <Link href="/" className={styles.logo}>
          PIPREVENDA
        </Link>

        <nav className={styles.nav}>
          {NAV_ITEMS.map((item) => {
            const active = pathname === item.href || pathname.startsWith(`${item.href}/`);
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`${styles.navLink} ${active ? styles.navLinkActive : ""}`}
              >
                <Icon name={item.icon} size={20} />
                {item.label}
              </Link>
            );
          })}
        </nav>

        <GlobalSearch />

        <div className={styles.actions}>
          <NotificationsDropdown />

          {/* Placeholder de usuário — sem autenticação implementada ainda. */}
          <div className={styles.userPlaceholder}>
            <Avatar initials="—" size="sm" />
          </div>
        </div>
      </header>

      <main className={styles.main}>{children}</main>
    </div>
  );
}
