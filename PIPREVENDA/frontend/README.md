# PIPREVENDA — Frontend

Frontend do Sistema de Gestão de Contratos da ALE Combustíveis
(PIPREVENDA-1680): telas de listagem, detalhe e dashboard executivo de
contratos comerciais (PCR/PCF, guarda-chuva, galonagem, jurídico), consumindo
a API .NET do backend deste mesmo bundle.

> Projeto em construção incremental — ver
> `../PLANO-IMPLEMENTACAO-FRONTEND.md` para o plano de fases (F0–F5) e o
> que já foi implementado. Hoje estão prontas a Fase F0 (fundação visual —
> tokens, fontes e os 16 componentes do Design System em
> `src/components/ui/`) e a Fase F1 (shell da aplicação — navegação, busca
> global e notificações, sem autenticação). As telas de negócio
> (Contratos, Dashboard, Detalhe) ainda não existem.

## Requisitos técnicos

- **Node.js 20.9 ou superior** (recomendado usar a versão LTS atual —
  desenvolvido e testado com Node 24)
- **npm** (gerenciador de pacotes usado no projeto; `package-lock.json` é
  versionado)

## Stack

- [Next.js 16](https://nextjs.org/) (App Router) + React 19 + TypeScript
- Estilo: CSS custom properties (design tokens) + CSS Modules — sem
  Tailwind (decisão registrada em
  `../docs/decisoes-tecnicas/DT-023-estrategia-estilo-frontend.md`)
- Fontes auto-hospedadas via `next/font/local` (Neo Sans Pro, Lato) e
  `next/font/google` (Inter); ícones via Material Symbols Rounded (Google
  Fonts)
- ESLint (`eslint-config-next`) para lint
- Testes: Vitest + React Testing Library + MSW (unitário/integração) e
  Playwright (E2E) — estratégia definida em
  `../docs/decisoes-tecnicas/DT-024-padroes-testes-frontend.md`

## Como rodar localmente

```bash
# 1. Instalar dependências
npm install

# 2. Subir o servidor de desenvolvimento
npm run dev
```

Acesse `http://localhost:3000`. Páginas disponíveis hoje:

- `/` — placeholder da aplicação
- `/design-system` — vitrine dos 16 componentes do Design System (página de
  conferência visual da Fase F0)

## Como compilar para produção

```bash
# Build otimizado
npm run build

# Servir o build de produção
npm run start
```

## Como rodar os testes

```bash
# Unitários e de integração (Vitest + React Testing Library + MSW)
npm run test

# Com relatório de cobertura (saída em coverage/)
npm run test:coverage

# E2E (Playwright — sobe o servidor de dev automaticamente).
# Na primeira vez, instale o browser: npx playwright install chromium
npm run test:e2e
```

Testes unitários/integração ficam co-localizados junto ao arquivo testado
(`Componente.tsx` + `Componente.test.tsx`); os E2E ficam em `e2e/`. Ver
`../docs/decisoes-tecnicas/DT-024-padroes-testes-frontend.md` para a
estratégia completa (o que é testado em cada camada, cobertura mínima,
padrões de nomenclatura).

## Scripts disponíveis

| Comando               | Descrição                                          |
|------------------------|-----------------------------------------------------|
| `npm run dev`          | Servidor de desenvolvimento (hot reload)             |
| `npm run build`        | Build de produção                                    |
| `npm run start`        | Sobe o build de produção (requer `build` antes)      |
| `npm run lint`         | Roda o ESLint sobre o projeto                        |
| `npm run test`         | Testes unitários/integração (Vitest)                 |
| `npm run test:watch`   | Testes unitários/integração em modo watch            |
| `npm run test:coverage`| Testes unitários/integração com relatório de cobertura |
| `npm run test:e2e`     | Testes E2E (Playwright)                              |

## Estrutura de pastas

```
src/
  app/                 # Rotas (App Router) — layout raiz, fontes, páginas
    design-system/     # Página de conferência dos componentes
  components/
    ui/                # Design System portado do protótipo (core,
                        # data-display, feedback, forms, navigation) —
                        # cada componente com seu *.test.tsx ao lado
    shell/              # Shell da aplicação (navegação, busca, notificações)
  styles/
    tokens.css          # Design tokens (cores, tipografia, espaçamento) —
                         # extraído do protótipo, não editar à mão
    base.css             # Reset e defaults globais de página
  mocks/                 # Handlers MSW (server.ts para testes, browser.ts
                          # para mock local de API em desenvolvimento)
public/
  fonts/                # Neo Sans Pro e Lato (auto-hospedadas)
e2e/                     # Testes E2E (Playwright)
```

## Observações importantes

- **Sem autenticação**: a integração com Azure AD B2C ainda não foi
  implementada (depende da Fase 4 do `../PLANO-IMPLEMENTACAO.md`). O bloco
  de usuário no shell é um placeholder explícito — não simula login.
- **Licença de fonte**: os arquivos da Neo Sans Pro (Monotype) estão em
  `public/fonts/neo-sans-pro/` extraídos do protótipo de design. Confirmar
  direito de redistribuição antes de publicar este repositório ou seu build
  externamente (ver seção 2 do plano de implementação do frontend).
- Antes de criar página, componente ou hook de dados novo, consulte
  `../.claude/skills/arquitetura-frontend-nextjs/SKILL.md`.
