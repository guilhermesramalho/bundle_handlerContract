# Plano de Implementação — Frontend (reconstrução do protótipo)

> Detalha como reconstruir, em Next.js real, tudo que está em
> `docs/modelo-frontend/prototipo.html`. Substitui/expande a Fase 2 (genérica)
> de `PLANO-IMPLEMENTACAO.md`. Fonte de verdade de negócio continua sendo
> `context/CONTEXTO-COMPLETO-PROJETO.md`; este documento é só sobre **como**
> portar a UI já prototipada.

---

## 0. O que é `prototipo.html` (leia antes de tentar abrir o arquivo)

`prototipo.html` **não é uma página HTML normal** — é um "Bundled Page"
(formato de artifact auto-contido, gerado por uma ferramenta de design da
Claude). Ele empacota, em base64 dentro de `<script type="__bundler/...">`,
quatro coisas distintas:

| Camada | O que é | Reaproveitável direto? |
|---|---|---|
| **Design System** (`ALEDesignSystem_e2bae5`, bundle `@ds-bundle` formato 3) | 16 componentes React genuínos (JSX compilado para `React.createElement`) | ✅ Sim — é React de verdade, só precisa reconverter para `.tsx` idiomático |
| **Tokens de design** | Cores, tipografia, espaçamento, raio, sombra — como CSS custom properties (`:root { --brand-primary: ... }`) | ✅ Sim — copiar como `tokens.css` |
| **Fontes** | Neo Sans Pro (OTF, display), Lato (TTF, corpo/UI), Inter (WOFF2, dados densos) + Material Symbols (Google CDN) | ⚠️ Verificar licença antes (seção 2) |
| **App do protótipo** (telas, dados mock, regras) | Escrito numa **DSL proprietária de templating** (`<x-dc>`, `sc-if`, `sc-for`, `{{ }}`, classe `DCLogic`) | ❌ Não roda em React/Next.js as-is — é especificação funcional a **reimplementar**, não código a colar |

**Por isso "reconstruir o protótipo" = duas tarefas diferentes:**
1. **Portar** o Design System + tokens + fontes (mecânico, baixo risco).
2. **Reimplementar** as telas/lógica de negócio da DSL proprietária em React/Next.js real (é aqui que mora o trabalho).

Este arquivo não abre/renderiza normalmente num editor por ser ~9MB numa
única linha de payload — a extração usada para escrever este plano foi feita
com um script Python (anexado na seção 6) que decodifica o manifest e separa
cada asset em um arquivo próprio. Rode-o de novo sempre que precisar
inspecionar o protótipo diretamente.

---

## 1. Inventário do que existe no protótipo

### 1.1 Tokens de design (já prontos para copiar)
- **Paleta primitiva**: `--auxiliar-color-*` (blue/cian/green/olive/orange/violet, 4 tons cada), `--neutral-color-*`, `--helper-color-{success,warning,error}-*`, `--primary-color-*` (azul ALE, `#0054A4`), `--secondary-color-*` (vermelho, `#EE2E21`), `--terciary-color-*` (dourado, `#FBC422`).
- **Tokens semânticos** (o que os componentes de fato consomem): `--surface-*`, `--text-*`, `--brand-primary(-hover/-press/-soft)`, `--border-*`, `--feedback-{success,warning,error}(-soft)`, `--radius-{sm,md,lg,xl,pill,circular}`, `--space-{quarck…xxxl}`, `--shadow-{xs,sm,md,lg,focus-ring}`.
- Suporte a tema claro/escuro já esboçado (`:root[data-theme="light|dark"]`), mas só com uma variável (`--schemes-on-surface-variant`) — o resto do sistema parece ainda não ter sido pensado para dark mode.

### 1.2 Tipografia e fontes
- **Display** (`--font-display`): Neo Sans Pro — pesos 300/400/500/700, arquivos `.otf` **auto-hospedados no bundle** (licença Monotype).
- **Corpo/UI** (`--font-sans`): Lato — pesos 300/400/600/700/900 (+ itálicos), `.ttf` auto-hospedados.
- **Dados densos/micro** (`--font-ui`): Inter — via `@font-face` com `unicode-range` fragmentado (padrão de export do Google Fonts), `.woff2`.
- **Ícones**: Material Symbols Rounded, carregado do Google Fonts por link `<link rel="preconnect" href="https://fonts.googleapis.com">` (não embutido).

### 1.3 Componentes do Design System (`ALEDesignSystem_e2bae5`, 16 componentes)
| Componente | Categoria |
|---|---|
| Button, FabButton, Icon, IconButton | `core` |
| Avatar, Card, Chip | `data-display` |
| Badge, TagStatus | `feedback` |
| Checkbox, Input, Radio, Select, Toggle | `forms` |
| Breadcrumbs, Tabs | `navigation` |

Todos usam **props + `style` inline com `var(--token)`** (nenhum Tailwind, nenhum CSS Modules) e um helper interno `Icon` para o Material Symbols. É a base de tudo que as telas usam.

> Há também um bundle secundário `ui_kits/ale-app/screens.jsx` dentro do mesmo arquivo de design system — é uma vitrine de componentes em formato mobile (tem um `StatusBar` mockando barra de status de celular). **Não é a aplicação real** (que está na DSL, ver 1.4) — ignorar para fins de reconstrução, é material de demonstração do design system.

### 1.4 As telas reais do protótipo (a aplicação em si, via `DCLogic`/`x-dc`)
Três `screen`s controlados por estado (`list` / `detail` / `dashboard`):

**A. Listagem de Contratos** (`screen: 'list'`)
- Busca por texto (CNPJ, nº SAP, grupo econômico).
- Filtros combináveis simples: `denuncia`, `bandeira`.
- Filtros múltiplos (`filtersMulti`): `segmento`, `tipo`, `uf`, `situacaoPcr`.
- Filtro hierárquico em cascata com múltipla seleção (`hier`): Diretor → Gerente Regional → Coordenador → Consultor Comercial — já reflete a hierarquia simplificada decidida em 03/07 (seção 8 do contexto: Gerente Executivo/Regional/Supervisor removidos).
- Ordenação por coluna (`sort: { col, dir }`).
- Indicador colorido de galonagem (verde/amarelo/vermelho, regra da seção 9 do contexto) e chip de situação PCR (Vigente / Vencido por galonagem / Vencido por data).

**B. Detalhe do Contrato** (`screen: 'detail'`) — 5 abas:
1. **Visão geral**
2. **Galonagem & produto** — trata separadamente guarda-chuva Rede (individual) vs. B2B (global do grupo, CNPJs solidários) — regra da seção 9 já implementada em `process()`.
3. **Performance** — 4 indicadores/gráficos (volume, margem, investimento aprovado vs. realizado, contribuição marginal) + bloco de TIR (Contratada/Realizada/Projetada) com seção colapsável "Ver racional" (tabela de 3 colunas: PCR Contratada / Realizado / Negociação mais recente — exatamente a tabela da seção 10 do contexto).
4. **Jurídico** — ações ativas (tipo, status, fase, resumo) + notificações, com badge de contagem na aba.
5. **PIR** — placeholder (correto: seção 10 do contexto confirma que PIR é fase futura, não implementar cálculo real).
- Toggle **Variante A / Variante B** de layout — usado para comparação com o cliente durante a prototipação, **não é uma feature real** (ver decisão pendente na seção 2).

**C. Dashboard Executivo** (`screen: 'dashboard'`, E6)
- Cards de indicadores agregados (ex.: galonagem vigente vs. vencida).
- Funil de vencimentos (`funilMode`: por prazo ou por galonagem, `funilRange` com atalhos 1/3/6/12 meses).
- Agrupamento de carteira (`carteiraGroup`, ex. por regional).
- Mesmo toggle de Variante A/B do detalhe.

**Transversais**: dropdown de notificações, tooltip flutuante sobre gráficos (`data-tip` + listener de `mousemove`), seletor de datas customizado (range preset ou custom), toasts.

### 1.5 Modelo de dados mock (referência para o front, não é a fonte de verdade)
Campos do objeto `Contrato` usados no protótipo — útil como checklist ao montar o DTO real de `GET /api/v1/contratos` quando o backend chegar na Fase 1.3 do `PLANO-IMPLEMENTACAO.md`:

```
id, pcr, pcf, cnpj, razao, segmento, grupo, diretoria, gr, rn, consultor,
situacaoMes, tipo, bandeira, registradoALE, dataANP, inicioBR, fimBR,
volMensal, frac, perf, margemBase, greenfield, denuncia, umbrella{role,group},
obs, clausula, sucedidoCnpj, sucedidoRazao,
juridico{acoesAtivas, notificacoes, ultimaNotif}, acoes[],
tir{proj, real}, eventos[]{tipo, data, desc}, garantia, sublocado, encerrado
```
A função `process()` deriva em runtime: `galContratada`, `galFaturada`, `saldo`, `galPct`, `situacaoPcr`, `pcrTone`, `barVermelha` — **essa derivação é lógica de apresentação, não deveria ser recalculada no front contra dados reais**; o ideal é que a API já devolva esses campos calculados (mover a regra de negócio para o backend, ver DT-016/019). Tratar isso como decisão de arquitetura ao desenhar o endpoint real, não replicar o cálculo no cliente.

---

## 2. Decisões a tomar ANTES de portar (não assumir, registrar resposta)

| Decisão | Por quê importa | Quem decide |
|---|---|---|
| Licença de redistribuição da fonte Neo Sans Pro (Monotype) | Arquivos `.otf` estão embutidos no protótipo; comitar no repo do frontend sem confirmar licença é risco legal | PM / quem contratou a licença original do design |
| Estratégia de estilo no Next.js: CSS custom properties + CSS Modules (maior fidelidade ao protótipo) vs. Tailwind | A skill `arquitetura-frontend-nextjs` ainda não define isso — hoje é decisão em aberto | Time técnico (Yuri/frontend) — registrar como DT de frontend nova |
| Qual variante (A/B) do Detalhe e do Dashboard vira produção | Protótipo mantém as duas de propósito, para comparação com o cliente | UX (André/Vitor) + Suzana |
| Origem dos dados de hierarquia comercial (`HIER_REG`/`HIER_GR`, hoje hardcoded) | Deve vir do SAP (integração bloqueada, Fase 5) ou ser cadastro manual na Fase 1? | Yuri Najar / TI ALE |
| Biblioteca de gráficos para a aba Performance | Protótipo desenha os gráficos "na mão" com SVG/tooltip customizado — não há decisão de lib (Recharts, visx, etc.) | Time técnico — registrar como DT nova |
| Onde calcular `situacaoPcr`/cores de galonagem: backend ou frontend | Ver nota da seção 1.5 — hoje é 100% client-side no protótipo | Arquitetura (Yuri) |

Nenhum destes bloqueia o início da Fase F0 (fundação visual) — mas bloqueiam F2/F3 se não resolvidos antes.

---

## 3. Ordem de reconstrução

### Fase F0 — Fundação visual (sem lógica de app ainda) ✅ concluída (2026-08-04)
- [x] Extrair os assets do bundle (script da seção 6) e organizar em `frontend/`:
  - Fontes → `public/fonts/`
  - Tokens (`:root {...}`) → `src/styles/tokens.css`
  - Reset/defaults de tipografia → `src/styles/base.css`
- [x] Configurar `next/font/local` para Neo Sans Pro e Lato; Inter pode continuar via Google Fonts ou `next/font/google` (mais simples que replicar os `unicode-range` manualmente). **Bloqueado por decisão de licença (seção 2) antes de comitar os `.otf`.** — *fontes locais commitadas sem confirmação formal da licença; risco sinalizado, não resolvido (ver seção 2).*
- [x] Portar os 16 componentes (`src/components/ui/*.tsx`), convertendo `React.createElement(...)` de volta para JSX — é transcrição mecânica (o bundle já é React real), não reescrita de comportamento.
- [x] Página de conferência (`/design-system` ou Storybook, o que for mais rápido de montar) mostrando todos os componentes lado a lado.
- **Critério de pronto:** os 16 componentes renderizam com fidelidade visual ao protótipo (fontes, cores, espaçamento). ✅

### Fase F1 — Shell da aplicação ✅ concluída (2026-08-04)
- [x] Layout raiz: navegação (Contratos / Dashboard), dropdown de notificações, busca global.
- [x] Sem autenticação ainda (depende da Fase 4 do `PLANO-IMPLEMENTACAO.md`, Azure AD B2C não implementado) — deixar isso explícito no código (comentário/placeholder), não simular login falso.

### Fase F2 — Listagem de Contratos (`/contratos`, E1) ✅ concluída (2026-08-05)
- [~] Camada de dados mock (`src/mocks/contratos.ts`) portando o dataset de 14 contratos + gerador de encerrados do protótipo (`build()`/`buildEncerrados()`) — objetivo: destravar a UI sem depender do endpoint real de `Contrato`, que está bloqueado no backend até as decisões de negócio da Fase 1.2/1.3 do `PLANO-IMPLEMENTACAO.md`. — *parcial: os 14 contratos ativos (`build()`/`process()`) foram portados; `buildEncerrados()` (histórico de contratos encerrados) **não** foi portado — confirmado no protótipo que a Listagem só lê `allContracts`, encerrados são exclusivos da tela de Detalhe. Pendente para a Fase F3.*
- [x] Isolar essa camada atrás de uma interface só de leitura (ex. um hook `useContratos()`), para que trocar mock por `useQuery` real na Fase F5 não exija tocar nos componentes de tela. — `src/hooks/useContratos.ts`, via React Query (dependência nova, instalada nesta fase).
- [x] Portar: busca texto, filtros combináveis, filtros múltiplos, filtro hierárquico em cascata, ordenação por coluna, cor da barra/chip de galonagem.
- **Critério de pronto:** tela navegável com todos os filtros do protótipo operando sobre o mock. ✅ verificado em `next build`, suíte de testes (85 testes) e navegação real no Chrome (Claude in Chrome). Botões de exportar Excel/PDF do protótipo não foram portados (fora do critério de pronto, seriam UI morta sem função real).

### Fase F3 — Detalhe do Contrato (`/contratos/[id]`)
- [ ] Resolver a decisão de variante única (seção 2) antes de portar — não implementar A e B em produção.
- [ ] Portar as 5 abas. Aba Performance depende da decisão de lib de gráficos (seção 2).
- [ ] Aba Jurídico e aba PIR: portar como estão (dados mock; PIR continua placeholder — não implementar cálculo, conforme seção 10 do contexto).
- **Critério de pronto:** navegação lista → detalhe → abas, fiel ao protótipo, sobre dados mock.

### Fase F4 — Dashboard Executivo (`/dashboard`, E6)
- [ ] Cards de indicadores, funil de vencimentos, agrupamento de carteira — reaproveitando o mesmo mock e a mesma decisão de variante da F3.

### Fase F5 — Troca progressiva de mock por API real
- [x] `Contrato` (a Listagem/F2, que é quem consome `useContratos()`) — feito em 2026-08-06,
      junto com a implementação da API (`PLANO-IMPLEMENTACAO-API-CONTRATO.md`). `GrupoEconomico`
      já existe no backend (pré-requisito da API de Contrato) mas o frontend ainda não tem tela
      própria para ele — só é consumido indiretamente (nome do grupo econômico na listagem).
- [x] Tipos gerados do OpenAPI real (`npm run generate:api-types` → `src/types/api.generated.ts`,
      via `openapi-typescript`) + cliente tipado (`openapi-fetch`, `src/lib/apiClient.ts`) —
      `useContratos()`/`useContratosOpcoesFiltro()` reescritos para consumir `GET
      /api/v1/contratos` de verdade, com paginação/filtro/ordenação **server-side** (antes era
      tudo client-side sobre o array mock completo — mudança de arquitetura, não só de fonte de
      dado, porque a API real pagina). Colunas "Venc. projetado" e "Jurídico" foram removidas da
      tabela (a API não expõe esses dados — Elaw e série histórica de faturamento são gaps
      documentados em `PLANO-IMPLEMENTACAO-API-CONTRATO.md` seção 2).
- **Achados de infraestrutura durante a integração (não óbvios, guardados na memória do
  projeto):** backend precisou de política CORS (nenhuma API .NET libera cross-origin por
  padrão — sem isso o browser bloqueia com "Failed to fetch" mesmo com a API saudável);
  `openapi-fetch` captura a referência de `fetch` na criação do client, então testes com MSW
  furavam o mock silenciosamente até o client passar a resolver `globalThis.fetch` a cada
  chamada em vez de uma vez só.

---

## 4. Riscos e observações

- **Licença de fonte**: não commitar os `.otf`/`.ttf` do Neo Sans Pro/Lato sem confirmar direito de redistribuição.
- **Dataset mock é fictício** (gerado por hash determinístico `rnd()` no protótipo) — não usar para validar regra de negócio com o cliente como se fosse dado real.
- **Toggle de variante A/B** existe só para fins de prototipação — não deve vazar para produção sem decisão explícita registrada.
- **Cálculo de `situacaoPcr`/cores no client vs. server**: replicar cegamente do protótipo (tudo no front) contraria o padrão do resto do projeto (regra de negócio no backend/domínio, ver DT-022) — decidir conscientemente, não por inércia.
- Nenhuma DT de frontend existe ainda (a skill já avisa isso) — cada decisão da seção 2 que for tomada deveria virar uma DT nova, no mesmo padrão do backend, para não se perder.

---

## 5. Por onde começar amanhã

1. Rodar a extração de assets (script da seção 6) e gerar `tokens.css`.
2. Levar a pergunta de licenciamento de fonte para quem gerou o protótipo — não bloqueia o resto, mas bloqueia comitar fontes.
3. Inicializar o Next.js (se ainda não feito) e já criar `src/styles/tokens.css` + `next/font/local`.
4. Portar 3 componentes primeiro (Button, Card, TagStatus) para validar o pipeline de tokens de ponta a ponta antes de portar os 16.
5. Só depois disso partir para F2 (Listagem) — é a tela que mais destrava valor (paralela ao que já existe no backend).

---

## 6. Script de extração do bundle (referência/reprodução)

O `prototipo.html` é grande demais para abrir/gregar diretamente (assets em
base64 dentro de poucas linhas gigantes). Este script (Python 3, sem
dependências externas) decodifica o manifest e separa cada asset em um
arquivo, permitindo inspecionar tokens/componentes/telas normalmente.

```python
import json, base64, gzip, os

SRC = "docs/modelo-frontend/prototipo.html"
OUT = "docs/modelo-frontend/_extraido"  # pasta de trabalho, não versionar
os.makedirs(OUT, exist_ok=True)

with open(SRC, "r", encoding="utf-8", errors="replace") as f:
    data = f.read()

def extract_block(type_name):
    marker = f'<script type="{type_name}">'
    start = data.index(marker) + len(marker)
    end = data.index("</script>", start)
    return data[start:end]

manifest = json.loads(extract_block("__bundler/manifest"))
template = json.loads(extract_block("__bundler/template"))

with open(os.path.join(OUT, "template.html"), "w", encoding="utf-8") as f:
    f.write(template)  # HTML com os <style> de tokens/fontes e o <script type="text/x-dc"> com a lógica das telas

for uuid, entry in manifest.items():
    raw = base64.b64decode(entry["data"])
    if entry.get("compressed"):
        raw = gzip.decompress(raw)
    mime = entry.get("mime", "")
    ext = {"application/javascript": ".js", "text/javascript": ".js",
           "font/otf": ".otf", "font/ttf": ".ttf", "font/woff2": ".woff2"}.get(mime, ".bin")
    with open(os.path.join(OUT, f"{uuid}{ext}"), "wb") as f:
        f.write(raw)

print("Extraído em", OUT)
```

Depois de rodar: os dois arquivos `.js` grandes no manifest são, respectivamente,
o runtime interno do bundler (`dc-runtime`, ignorar) e o **Design System**
(`@ds-bundle`, começa com um comentário `/* @ds-bundle: {...} */` listando os
16 componentes — este é o que importa). `template.html` contém os tokens
(`:root {...}`), os `@font-face`, e a lógica das telas dentro de
`<script type="text/x-dc">` (classe `Component extends DCLogic`).
