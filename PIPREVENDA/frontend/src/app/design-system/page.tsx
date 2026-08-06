"use client";

import { useState, type ReactNode } from "react";
import {
  Avatar,
  Badge,
  Breadcrumbs,
  Button,
  Card,
  Checkbox,
  Chip,
  FabButton,
  Icon,
  IconButton,
  Input,
  Radio,
  Select,
  Tabs,
  TagStatus,
  Toggle,
} from "@/components/ui";
import styles from "./page.module.css";

function ComponentBlock({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className={styles.componentBlock}>
      <span className={styles.componentTitle}>{title}</span>
      <div className={styles.row}>{children}</div>
    </div>
  );
}

/**
 * Página de conferência do Design System (Fase F0). Mostra os 16
 * componentes portados de ALEDesignSystem_e2bae5 lado a lado, com as
 * principais variações de props, para validar fidelidade visual ao
 * protótipo antes de partir para as telas de negócio (F2+).
 */
export default function DesignSystemPage() {
  const [checked, setChecked] = useState(true);
  const [radioValue, setRadioValue] = useState("a");
  const [toggleOn, setToggleOn] = useState(true);
  const [inputValue, setInputValue] = useState("");
  const [selectValue, setSelectValue] = useState<string | undefined>();
  const [chipSelected, setChipSelected] = useState(true);
  const [tab, setTab] = useState("overview");

  return (
    <main className={styles.page}>
      <div className={styles.header}>
        <h1 style={{ font: "var(--text-display-md)" }}>Design System — ALEDesignSystem_e2bae5</h1>
        <p style={{ font: "var(--text-body-lg)", color: "var(--text-secondary)" }}>
          Página de conferência (Fase F0) — os 16 componentes portados do protótipo, com variações de props.
        </p>
      </div>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Core</h2>

        <ComponentBlock title="Button — types">
          <Button type="primary">Primary</Button>
          <Button type="secondary">Secondary</Button>
          <Button type="tertiary">Tertiary</Button>
          <Button type="link">Link</Button>
          <Button type="primary" disabled>
            Disabled
          </Button>
          <Button type="primary" loading>
            Loading
          </Button>
        </ComponentBlock>

        <ComponentBlock title="Button — sizes & ícones">
          <Button size="small" leadingIcon="add">
            Small
          </Button>
          <Button size="medium" leadingIcon="add">
            Medium
          </Button>
          <Button size="large" trailingIcon="arrow_forward">
            Large
          </Button>
        </ComponentBlock>

        <ComponentBlock title="IconButton — types & sizes">
          <IconButton icon="edit" type="primary" aria-label="Editar" />
          <IconButton icon="edit" type="secondary" aria-label="Editar" />
          <IconButton icon="edit" type="tertiary" aria-label="Editar" />
          <IconButton icon="delete" type="ghost" aria-label="Excluir" />
          <IconButton icon="more_vert" type="ghost" rounded aria-label="Mais opções" />
        </ComponentBlock>

        <ComponentBlock title="FabButton">
          <FabButton icon="add" type="primary" aria-label="Adicionar" />
          <FabButton icon="edit" type="secondary" aria-label="Editar" />
          <FabButton icon="filter_alt" type="tertiary" size="small" aria-label="Filtrar" />
        </ComponentBlock>

        <ComponentBlock title="Icon — pesos">
          <Icon name="home" />
          <Icon name="home" fill />
          <Icon name="home" size={32} color="var(--brand-primary)" />
        </ComponentBlock>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Data display</h2>

        <ComponentBlock title="Avatar — tamanhos e status">
          <Avatar name="Marina Alves" size="sm" />
          <Avatar name="Marina Alves" size="md" status="online" />
          <Avatar name="Marina Alves" size="lg" status="busy" />
          <Avatar name="Marina Alves" size="xl" status="away" />
        </ComponentBlock>

        <ComponentBlock title="Card — elevated / outlined / interactive">
          <Card style={{ width: 200 }}>Elevated</Card>
          <Card outlined style={{ width: 200 }}>
            Outlined
          </Card>
          <Card interactive style={{ width: 200 }}>
            Interactive (hover)
          </Card>
        </ComponentBlock>

        <ComponentBlock title="Chip — selected / disabled">
          <Chip selected={chipSelected} onClick={() => setChipSelected((v) => !v)} leadingIcon="local_gas_station">
            Rede
          </Chip>
          <Chip>B2B</Chip>
          <Chip disabled>Indisponível</Chip>
        </ComponentBlock>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Feedback</h2>

        <ComponentBlock title="Badge — tons e dot">
          <Badge tone="tertiary" count={3} />
          <Badge tone="primary" count={12} />
          <Badge tone="error" count={128} max={99} />
          <Badge tone="success" dot />
          <Badge tone="neutral" dot />
        </ComponentBlock>

        <ComponentBlock title="TagStatus — situação PCR">
          <TagStatus tone="success" leadingIcon="check_circle">
            Vigente
          </TagStatus>
          <TagStatus tone="warning" leadingIcon="warning">
            Vencido por galonagem
          </TagStatus>
          <TagStatus tone="error" leadingIcon="event_busy">
            Vencido por data
          </TagStatus>
          <TagStatus tone="info">Rede</TagStatus>
          <TagStatus tone="neutral">B2B</TagStatus>
        </ComponentBlock>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Forms</h2>

        <div className={styles.col}>
          <Checkbox label="Selecionado" checked={checked} onChange={setChecked} />
          <Checkbox label="Indeterminado" indeterminate />
          <Checkbox label="Desabilitado" disabled />

          <Radio name="demo-radio" value="a" label="Opção A" checked={radioValue === "a"} onChange={setRadioValue} />
          <Radio name="demo-radio" value="b" label="Opção B" checked={radioValue === "b"} onChange={setRadioValue} />

          <Toggle label="Ativo" checked={toggleOn} onChange={setToggleOn} />

          <Input
            label="CNPJ / Nº SAP / Grupo econômico"
            placeholder="Buscar contrato..."
            leadingIcon="search"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
          />
          <Input label="Campo com erro" error helperText="Campo obrigatório" />
          <Input label="Campo desabilitado" disabled value="Não editável" onChange={() => {}} />

          <Select
            label="Segmento"
            placeholder="Selecione o segmento"
            value={selectValue}
            onChange={setSelectValue}
            options={["Rede", "GRR", "COFA", "COFD", "COFAR"]}
          />
        </div>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Navigation</h2>

        <ComponentBlock title="Breadcrumbs">
          <Breadcrumbs items={["Contratos", "PCR 000123", "Visão geral"]} />
        </ComponentBlock>

        <ComponentBlock title="Tabs — com badge (aba Jurídico)">
          <Tabs
            value={tab}
            onChange={setTab}
            tabs={[
              { value: "overview", label: "Visão geral" },
              { value: "galonagem", label: "Galonagem & produto" },
              { value: "performance", label: "Performance" },
              { value: "juridico", label: "Jurídico", badge: 3 },
              { value: "pir", label: "PIR" },
            ]}
          />
        </ComponentBlock>
      </section>
    </main>
  );
}
