# DT-023: Estratégia de Estilo no Frontend Next.js

> **Metadados do Documento**
> **Componente:** `Frontend`
> **Tipo:** Decisão Técnica
>
> **Propósito:** Definir como os componentes e telas do frontend Next.js aplicam estilo, encerrando a decisão em aberto registrada em `PLANO-IMPLEMENTACAO-FRONTEND.md` seção 2.
>
> **Quando usar:** Ao criar ou portar qualquer componente/página no frontend — antes de escolher entre CSS Modules, Tailwind, styled-components ou inline styles.
>
> **Palavras-chave:** `frontend` `nextjs` `css` `css-modules` `design-tokens` `custom-properties`

### Contexto

O protótipo (`docs/modelo-frontend/prototipo.html`) implementa os 16
componentes do Design System (`ALEDesignSystem_e2bae5`) inteiramente com
`style` inline em React, consumindo tokens via `var(--token)` (CSS custom
properties definidas em `tokens.css`). Não há Tailwind nem CSS Modules no
protótipo. A skill `arquitetura-frontend-nextjs` ainda não definia essa
escolha para o app Next.js real — ficou registrada como decisão em aberto no
plano de reconstrução do frontend (Fase F0).

### Decisão

Adotar **CSS custom properties (tokens de `src/styles/tokens.css`) + CSS
Modules**, sem Tailwind:

- **Componentes primitivos do Design System** (`src/components/ui/**`):
  seguem o padrão do protótipo — `style` inline referenciando
  `var(--token)`. É transcrição mecânica de `React.createElement` para JSX
  idiomático, sem introduzir uma camada de estilo nova.
- **Composições de tela/layout** (shell, páginas, seções que agrupam
  primitivos — ex. `src/components/shell/AppShell.tsx`): usam **CSS Modules**
  (`*.module.css`) para layout estrutural (grid, flex, posicionamento),
  também consumindo os mesmos tokens via `var(--token)`.
- Tokens continuam centralizados em `src/styles/tokens.css` (extraído do
  protótipo) e `src/styles/base.css` — nenhum dos dois deve ser reescrito à
  mão sem atualizar a origem.

### Alternativas Consideradas

1. **Tailwind CSS** — rejeitado: exigiria mapear todos os tokens do
   protótipo (`--brand-primary`, `--space-*`, `--radius-*`, etc.) para
   `tailwind.config`, além de reescrever os 16 componentes do zero em vez de
   portá-los mecanicamente. Maior esforço sem ganho de fidelidade visual.
2. **styled-components / CSS-in-JS com runtime** — rejeitado: adiciona
   dependência e custo de bundle não presentes no protótipo nem exigidos
   pelo volume/UX do projeto (ver `arquitetura-frontend-nextjs`: CSR para
   telas de dados, sem necessidade de theming dinâmico via JS).
3. **Só inline style, sem CSS Modules** — rejeitado para composições de
   tela: layout estrutural (grid de página, media queries) fica ilegível e
   não reutilizável só com objetos de style inline.

### Consequências

- (+) Porte dos 16 componentes do Design System é fiel ao protótipo e de
  baixo risco (mesma técnica, só troca de `React.createElement` por JSX).
- (+) Tokens seguem como única fonte de verdade visual — trocar tema
  (`data-theme`) no futuro não exige tocar componentes.
- (–) CSS Modules e inline `var(--token)` convivem no mesmo código; times
  acostumados só com Tailwind têm uma curva de adaptação.
- (–) Sem verificação estática de "token inexistente" (um `var(--typo)`
  digitado errado só falha silenciosamente em runtime) — mitigar com a
  página `/design-system` como checagem visual de fidelidade.

### Implementação

- `src/styles/tokens.css` e `src/styles/base.css`: importados globalmente
  em `src/app/globals.css`.
- Componentes do Design System: `src/components/ui/{core,data-display,feedback,forms,navigation}/*.tsx`,
  com `style` inline.
- Composições de tela: `*.module.css` ao lado do componente/página
  (ex. `src/app/design-system/page.module.css`, `src/components/shell/AppShell.module.css`).

### Verificação de Conformidade

- [ ] Componentes novos em `src/components/ui/**` usam `style` inline com `var(--token)`, sem classes Tailwind.
- [ ] Nenhuma dependência de Tailwind (`tailwindcss`, `@tailwindcss/*`) foi adicionada ao `package.json`.
- [ ] Layout estrutural de página/composição usa `*.module.css`, não `style` inline extenso.
- [ ] Nenhum valor de cor/espaçamento/raio é hardcoded fora de `tokens.css` — sempre via `var(--token)`.

---
