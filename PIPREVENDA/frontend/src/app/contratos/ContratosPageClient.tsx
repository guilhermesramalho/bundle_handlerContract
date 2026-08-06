"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { Icon, Input, TagStatus, type TagStatusTone } from "@/components/ui";
import {
  useContratos,
  useContratosOpcoesFiltro,
  type Segmento,
  type SituacaoPcr,
  type TipoContrato,
} from "@/hooks/useContratos";
import type { components } from "@/types/api.generated";
import { FilterDropdown } from "./_components/FilterDropdown";
import styles from "./ContratosPage.module.css";

type ContratoItem = components["schemas"]["ListarContratosResponse"];

type SortCol = "contratoId" | "razao" | "grupo" | "segmento" | "galPct" | "fim";
interface SortState {
  col: SortCol;
  dir: "asc" | "desc";
}

interface SingleFilters {
  emDenuncia: string[];
  bandeira: string[];
}

interface MultiFilters {
  segmentos: Segmento[];
  tipos: TipoContrato[];
  ufs: string[];
  situacoesPcr: SituacaoPcr[];
}

/** Os 4 níveis da cascata de hierarquia comercial na Listagem (E1). */
const HIER_LEVELS = [
  { field: "diretoria", label: "Diretor" },
  { field: "regionalVendas", label: "Gerente Regional" },
  { field: "pontoVenda", label: "Coordenador" },
  { field: "consultor", label: "Consultor Comercial" },
] as const;
type HierField = (typeof HIER_LEVELS)[number]["field"];
type HierState = Record<HierField, string[]>;

const EMPTY_HIER: HierState = { diretoria: [], regionalVendas: [], pontoVenda: [], consultor: [] };
const PAGE_SIZE = 20;

const SEG_TONE: Record<Segmento, TagStatusTone> = {
  Rede: "info",
  Grr: "cian",
  Cofa: "violet",
  Cofd: "orange",
  Cofar: "success",
  Spot: "neutral",
  Outros: "neutral",
};

const SITUACAO_LABEL: Record<SituacaoPcr, string> = {
  Vigente: "Vigente",
  VencidoPorGalonagem: "Venc. galonagem",
  VencidoPorData: "Venc. data",
};

function uniq<T extends string>(items: ContratoItem[], selector: (item: ContratoItem) => T): T[] {
  return [...new Set(items.map(selector))];
}

/** Cor da barra de galonagem — mesma regra de negócio do backend (contexto seção 9):
 * vencido por galonagem = barra verde; vencido por data = barra vermelha; vigente = amarela
 * até completar. Calculada aqui porque a API só devolve o percentual bruto, não a cor. */
function corBarraGalonagem(situacaoPcr: SituacaoPcr, percentual: number): string {
  if (situacaoPcr === "Vigente" && percentual <= 0.99) return "var(--helper-color-warning-warning-pure)";
  return situacaoPcr === "VencidoPorData" ? "var(--feedback-error)" : "var(--feedback-success)";
}

export function ContratosPageClient() {
  const searchParams = useSearchParams();

  const [search, setSearch] = useState(() => searchParams.get("q") ?? "");
  const [buscaEfetiva, setBuscaEfetiva] = useState(search);
  const [filters, setFilters] = useState<SingleFilters>({ emDenuncia: [], bandeira: [] });
  const [filtersMulti, setFiltersMulti] = useState<MultiFilters>({ segmentos: [], tipos: [], ufs: [], situacoesPcr: [] });
  const [hier, setHier] = useState<HierState>(EMPTY_HIER);
  const [sort, setSort] = useState<SortState>({ col: "contratoId", dir: "asc" });
  const [pageIndex, setPageIndex] = useState(1);

  // Busca com debounce — evita 1 requisição por tecla digitada.
  useEffect(() => {
    const timer = setTimeout(() => setBuscaEfetiva(search), 400);
    return () => clearTimeout(timer);
  }, [search]);

  const opcoes = useContratosOpcoesFiltro();
  const opcoesItems = opcoes.data?.items ?? [];

  const emDenunciaValor = filters.emDenuncia[0] === "Em denúncia" ? true : filters.emDenuncia[0] === "Sem denúncia" ? false : undefined;

  const { data, isLoading, isError } = useContratos({
    busca: buscaEfetiva || undefined,
    segmentos: filtersMulti.segmentos,
    tipos: filtersMulti.tipos,
    situacoesPcr: filtersMulti.situacoesPcr,
    emDenuncia: emDenunciaValor,
    bandeira: filters.bandeira[0],
    ufs: filtersMulti.ufs,
    diretoria: hier.diretoria[0],
    regionalVendas: hier.regionalVendas[0],
    pontoVenda: hier.pontoVenda[0],
    consultor: hier.consultor[0],
    sortBy: sort.col,
    sortDirection: sort.dir,
    pageIndex,
    pageSize: PAGE_SIZE,
  });

  const rows = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = data?.totalPages ?? 0;

  function resetarPaginaEAplicar<T>(setter: (updater: (prev: T) => T) => void, updater: (prev: T) => T) {
    setPageIndex(1);
    setter(updater);
  }

  function setHierLevel(levelIndex: number, next: string[]) {
    resetarPaginaEAplicar(setHier, (prev) => {
      const updated = { ...prev, [HIER_LEVELS[levelIndex].field]: next };
      for (let j = levelIndex + 1; j < HIER_LEVELS.length; j++) updated[HIER_LEVELS[j].field] = [];
      return updated;
    });
  }

  function hierOptions(levelIndex: number): string[] {
    let pool = opcoesItems;
    for (let j = 0; j < levelIndex; j++) {
      const field = HIER_LEVELS[j].field;
      const sel = hier[field];
      if (sel.length) pool = pool.filter((c) => sel.includes(c[field]));
    }
    const field = HIER_LEVELS[levelIndex].field;
    return [...new Set(pool.map((c) => c[field]))].filter(Boolean).sort((a, b) => a.localeCompare(b, "pt-BR"));
  }

  function clearFilters() {
    setPageIndex(1);
    setFilters({ emDenuncia: [], bandeira: [] });
    setFiltersMulti({ segmentos: [], tipos: [], ufs: [], situacoesPcr: [] });
    setHier(EMPTY_HIER);
    setSearch("");
    setBuscaEfetiva("");
  }

  function toggleSort(col: SortCol) {
    setPageIndex(1);
    setSort((s) => ({ col, dir: s.col === col && s.dir === "asc" ? "desc" : "asc" }));
  }

  const multiActiveCount =
    (filtersMulti.segmentos.length ? 1 : 0) +
    (filtersMulti.tipos.length ? 1 : 0) +
    (filtersMulti.ufs.length ? 1 : 0) +
    (filtersMulti.situacoesPcr.length ? 1 : 0);
  const hierActiveCount = HIER_LEVELS.filter((lv) => hier[lv.field].length > 0).length;
  const singleActiveCount = (filters.emDenuncia.length ? 1 : 0) + (filters.bandeira.length ? 1 : 0);
  const activeFilterCount = multiActiveCount + hierActiveCount + singleActiveCount + (buscaEfetiva.trim() ? 1 : 0);

  function sortArrow(col: SortCol) {
    if (sort.col !== col) return null;
    return <Icon name={sort.dir === "asc" ? "arrow_upward" : "arrow_downward"} size={14} />;
  }

  return (
    <div>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Base consolidada de contratos</h1>
          <p className={styles.subtitle}>
            Visão única Rede + B2B — dados alimentados pelas integrações SAP, PCR e Elaw.{" "}
            <strong className={styles.subtitleStrong}>{totalCount}</strong> contrato{totalCount === 1 ? "" : "s"} encontrado
            {totalCount === 1 ? "" : "s"}.
          </p>
        </div>
      </div>

      <div className={styles.filtersPanel}>
        <div className={styles.filtersPanelHeader}>
          <div className={styles.filtersPanelTitle}>
            <Icon name="tune" size={20} color="var(--brand-primary)" />
            <span>Filtros avançados</span>
            {activeFilterCount > 0 && <span className={styles.filterBadge}>{activeFilterCount}</span>}
          </div>
          <button type="button" className={styles.clearButton} onClick={clearFilters}>
            <Icon name="backspace" size={18} />
            Limpar filtros
          </button>
        </div>

        <Input
          placeholder="Buscar CNPJ, nº SAP, PCR/PCF, grupo econômico..."
          leadingIcon="search"
          value={search}
          onChange={(e) => {
            setPageIndex(1);
            setSearch(e.target.value);
          }}
          aria-label="Buscar contratos"
          style={{ marginBottom: 14 }}
        />

        <div className={styles.filtersGrid}>
          <FilterDropdown
            label="Segmento"
            placeholder="Todos"
            options={uniq(opcoesItems, (c) => c.segmento)}
            selected={filtersMulti.segmentos}
            onChange={(next) => resetarPaginaEAplicar(setFiltersMulti, (f) => ({ ...f, segmentos: next as Segmento[] }))}
          />
          <FilterDropdown
            label="Tipo de contrato"
            placeholder="Todos"
            options={uniq(opcoesItems, (c) => c.tipo)}
            selected={filtersMulti.tipos}
            onChange={(next) => resetarPaginaEAplicar(setFiltersMulti, (f) => ({ ...f, tipos: next as TipoContrato[] }))}
          />
          <FilterDropdown
            label="Situação da PCR"
            placeholder="Todas"
            options={["Vigente", "VencidoPorGalonagem", "VencidoPorData"]}
            selected={filtersMulti.situacoesPcr}
            renderLabel={(v) => SITUACAO_LABEL[v as SituacaoPcr]}
            onChange={(next) => resetarPaginaEAplicar(setFiltersMulti, (f) => ({ ...f, situacoesPcr: next as SituacaoPcr[] }))}
          />
          <FilterDropdown
            label="Denúncia"
            placeholder="Todas"
            multiple={false}
            options={["Em denúncia", "Sem denúncia"]}
            selected={filters.emDenuncia}
            onChange={(next) => resetarPaginaEAplicar(setFilters, (f) => ({ ...f, emDenuncia: next }))}
          />
          <FilterDropdown
            label="Bandeira ANP"
            placeholder="Todas"
            multiple={false}
            options={uniq(opcoesItems, (c) => c.bandeira)}
            selected={filters.bandeira}
            onChange={(next) => resetarPaginaEAplicar(setFilters, (f) => ({ ...f, bandeira: next }))}
          />
          <FilterDropdown
            label="UF"
            placeholder="Todas"
            options={uniq(opcoesItems, (c) => c.clienteUf ?? "").filter(Boolean).sort((a, b) => a.localeCompare(b, "pt-BR"))}
            selected={filtersMulti.ufs}
            onChange={(next) => resetarPaginaEAplicar(setFiltersMulti, (f) => ({ ...f, ufs: next }))}
          />
        </div>

        <div className={styles.hierDivider}>
          <Icon name="account_tree" size={18} color="var(--brand-primary)" />
          <span>Estrutura comercial</span>
          <div className={styles.hierDividerLine} />
        </div>
        <div className={styles.hierGrid}>
          {HIER_LEVELS.map((lv, i) => (
            <FilterDropdown
              key={lv.field}
              label={lv.label}
              placeholder="Todos"
              options={hierOptions(i)}
              selected={hier[lv.field]}
              onChange={(next) => setHierLevel(i, next)}
            />
          ))}
        </div>
      </div>

      <div className={styles.tableCard}>
        <div className={styles.tableScroll}>
          <div className={styles.tableInner}>
            <div className={`${styles.tableGrid} ${styles.tableHead}`}>
              <button type="button" className={styles.th} onClick={() => toggleSort("contratoId")}>
                Contrato {sortArrow("contratoId")}
              </button>
              <button type="button" className={styles.th} onClick={() => toggleSort("razao")}>
                Cliente {sortArrow("razao")}
              </button>
              <button type="button" className={styles.th} onClick={() => toggleSort("grupo")}>
                Grupo econômico {sortArrow("grupo")}
              </button>
              <button type="button" className={styles.th} onClick={() => toggleSort("segmento")}>
                Segmento {sortArrow("segmento")}
              </button>
              <div className={styles.thStatic}>Tipo</div>
              <button type="button" className={styles.th} onClick={() => toggleSort("galPct")}>
                Cumprim. galonagem {sortArrow("galPct")}
              </button>
              <div className={styles.thStatic}>Situação PCR</div>
              <button type="button" className={styles.th} onClick={() => toggleSort("fim")}>
                Vencimento {sortArrow("fim")}
              </button>
            </div>

            {isLoading && <div className={styles.stateMessage}>Carregando contratos…</div>}
            {isError && <div className={styles.stateMessage}>Não foi possível carregar os contratos.</div>}

            {!isLoading &&
              !isError &&
              rows.map((c) => (
                <Link key={c.id} href={`/contratos/${c.id}`} className={`${styles.tableGrid} ${styles.row}`}>
                  <div className={styles.cell}>{c.contratoId}</div>
                  <div className={styles.cell}>
                    <div className={styles.cellPrimaryRow}>
                      <span className={styles.cellPrimary}>{c.clienteNome}</span>
                      {c.papelGuardaChuva && <Icon name="account_tree" size={18} color="var(--brand-primary)" />}
                    </div>
                    <div className={styles.cellSecondary}>
                      {c.clienteCnpj}
                      {c.clienteNrSap ? ` · SAP ${c.clienteNrSap}` : ""}
                    </div>
                  </div>
                  <div className={styles.cell}>
                    <div className={styles.cellPrimary}>{c.grupoEconomicoNome ?? "—"}</div>
                    <div className={styles.cellSecondary}>{c.clienteUf ? `UF ${c.clienteUf}` : ""}</div>
                  </div>
                  <div className={styles.cell}>
                    <TagStatus tone={SEG_TONE[c.segmento]}>{c.segmento}</TagStatus>
                  </div>
                  <div className={styles.cell}>{c.tipo}</div>
                  <div className={styles.cell}>
                    <div className={styles.galWrap}>
                      <div className={styles.galTrack}>
                        <div
                          className={styles.galFill}
                          style={{
                            width: `${Math.min(100, c.galonagemPercentual * 100).toFixed(0)}%`,
                            background: corBarraGalonagem(c.situacaoPcr, c.galonagemPercentual),
                          }}
                        />
                      </div>
                      <span className={styles.galPct}>{(c.galonagemPercentual * 100).toFixed(0)}%</span>
                    </div>
                  </div>
                  <div className={styles.cell}>
                    <TagStatus
                      tone={c.situacaoPcr === "Vigente" ? "success" : "pink"}
                      leadingIcon={c.situacaoPcr === "Vigente" ? "check_circle" : "error"}
                    >
                      {SITUACAO_LABEL[c.situacaoPcr]}
                    </TagStatus>
                  </div>
                  <div className={styles.cell}>{new Date(`${c.fimVigencia}T00:00:00`).toLocaleDateString("pt-BR")}</div>
                </Link>
              ))}
          </div>
        </div>

        {!isLoading && !isError && rows.length === 0 && (
          <div className={styles.emptyState}>
            <Icon name="search_off" size={40} color="var(--text-tertiary)" />
            <p>Nenhum contrato encontrado para os filtros aplicados.</p>
          </div>
        )}

        {!isLoading && !isError && totalPages > 1 && (
          <div className={styles.pagination}>
            <button
              type="button"
              className={styles.pageButton}
              disabled={pageIndex <= 1}
              onClick={() => setPageIndex((p) => Math.max(1, p - 1))}
            >
              <Icon name="chevron_left" size={18} />
              Anterior
            </button>
            <span className={styles.pageInfo}>
              Página {pageIndex} de {totalPages}
            </span>
            <button
              type="button"
              className={styles.pageButton}
              disabled={pageIndex >= totalPages}
              onClick={() => setPageIndex((p) => Math.min(totalPages, p + 1))}
            >
              Próxima
              <Icon name="chevron_right" size={18} />
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
