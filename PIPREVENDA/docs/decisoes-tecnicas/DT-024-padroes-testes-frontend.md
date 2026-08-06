# DT-024: Estratégia de Testes para Frontend (React / Next.js)

> **Metadados do Documento**
> **Componente:** `Frontend`
> **Tipo:** Decisão Técnica
>
> **Propósito:** Definir estratégia de testes de frontend como abordagem em pirâmide (unitário → integração → E2E), ferramentas obrigatórias (Vitest, React Testing Library, MSW, Playwright) e cobertura mínima por camada acima de (70%)
>
> **Quando usar:** Ao implementar testes em componentes, hooks, rotas/páginas ou fluxos do frontend, configurar novos projetos Next.js ou revisar decisões sobre escopo e ferramental de testes
>
> **Palavras-chave:** `vitest` `testes-unitarios` `react-testing-library` `msw` `playwright` `cobertura` `estrategia-testes` `testes-integração` `testes-e2e` `nextjs` `react`

---

## Contexto

O frontend do PIPREVENDA-1680 é construído com **React** e **Next.js** (App Router), utilizando componentes funcionais com Hooks, gerenciamento de estado via Server/Client Components, chamadas a APIs REST e integração com o backend PortalAle. A equipe identificou os seguintes desafios relacionados à estratégia de testes:

1. **Complexidade de testes end-to-end (E2E)**: Configuração de ambiente completo (browser, backend real, dados de seed) aumenta significativamente o tempo de implementação e manutenção
2. **Dependências externas**: Testes que dependem de chamadas HTTP reais a APIs (backend, AIDA, APIM) são frágeis, lentos e instáveis (*flaky*)
3. **Baixa cobertura atual**: Ausência de testes em componentes, hooks customizados e lógica de apresentação
4. **Custo vs. benefício**: Testes E2E completos oferecem retorno marginal quando comparado à cobertura unitária/integração de componentes
5. **Renderização híbrida do Next.js**: Server Components, Client Components e Route Handlers exigem estratégias de teste distintas

**Necessidade**: Simplificar a implementação de testes, reduzir dependências externas de infraestrutura e maximizar a cobertura de código com menor esforço, priorizando testes unitários e de integração de componentes (com mocks de rede), reservando E2E para os fluxos críticos de negócio.

---

## Decisão

**Adotar uma estratégia em pirâmide de testes**, priorizando testes unitários e de integração de componentes como base principal, com uso obrigatório de **Vitest** (test runner), **React Testing Library** (testes de componentes), **MSW - Mock Service Worker** (mock de chamadas HTTP) e **Playwright** (testes E2E dos fluxos críticos).

### Ferramentas Obrigatórias

| Ferramenta                  | Finalidade                              | Uso Obrigatório                                      |
| ---------------------------- | ---------------------------------------- | ----------------------------------------------------- |
| **Vitest**                   | Test runner / framework de testes        | Todos os projetos de teste unitário e integração       |
| **React Testing Library**    | Testes de componentes React              | Componentes, páginas e hooks                           |
| **@testing-library/user-event** | Simulação de interações do usuário    | Testes de componentes com interação (clique, digitação) |
| **MSW (Mock Service Worker)**| Mock de requisições HTTP/API             | Isolar chamadas de rede em testes unitários e integração |
| **Playwright**               | Testes E2E (ponta a ponta)               | Fluxos críticos de negócio                              |
| **@faker-js/faker**          | Geração de dados falsos                  | Criar objetos de teste realistas                        |

### Justificativa da Estratégia

- **Simplicidade**: Testes unitários e de componentes não exigem browser real nem backend em execução
- **Velocidade**: Vitest executa em milissegundos (ambiente `jsdom`/`happy-dom`), enquanto E2E leva segundos/minutos
- **Fidelidade ao usuário**: React Testing Library incentiva testar o componente pela perspectiva do usuário (o que é renderizado na tela), não detalhes de implementação
- **Rede realista sem instabilidade**: MSW intercepta requisições no nível de rede, permitindo simular respostas reais da API sem depender de servidor ativo
- **E2E reservado ao essencial**: Playwright cobre apenas os fluxos críticos (login, submissão de projeto, etc.), evitando suíte E2E lenta e frágil
- **Cobertura eficiente**: 70-75% de cobertura em componentes/hooks/utilitários oferece retorno superior ao investimento em uma suíte E2E extensa

---

## Estratégia de Testes por Camada

### 1. **Componentes de UI (Presentational / "Dumb" Components)**

**Escopo:**

- ✅ Componentes reutilizáveis (`Button`, `Input`, `Modal`, `Table`, etc.)
- ✅ Componentes de formulário e validação
- ✅ Componentes com lógica condicional de renderização

**Tipo de Teste:** Unitário (Component Testing)
**Framework:** Vitest + React Testing Library
**Cobertura Mínima:** 70%

**Observações:**

- Testar o componente pelo comportamento visível (texto renderizado, roles, estados), nunca por detalhes internos (`state`, nomes de variáveis)
- Usar queries acessíveis (`getByRole`, `getByLabelText`) em vez de `getByTestId` sempre que possível
- Validar estados de *loading*, erro e vazio (*empty state*)

---

### 2. **Componentes Conectados (Client Components com chamadas de API)**

**Escopo:**

- ✅ Componentes que consomem dados via `fetch`, `axios` ou hooks de data-fetching (ex: React Query/SWR)
- ✅ Formulários com submissão para API
- ✅ Componentes com efeitos colaterais (`useEffect`) dependentes de rede

**Tipo de Teste:** Integração (Component + Rede mockada)
**Framework:** Vitest + React Testing Library + MSW
**Cobertura Mínima:** 70%

**Observações:**

- **Mockar todas as chamadas HTTP** usando MSW (nunca mockar o `fetch`/`axios` diretamente)
- Validar cenários de sucesso, erro (4xx/5xx) e *timeout*/estado de carregamento
- Usar `findBy*` (assíncrono) para aguardar atualizações de UI pós-requisição

---

### 3. **Hooks Customizados (`useX`)**

**Escopo:**

- ✅ Hooks de lógica de negócio (`useProjeto`, `useFiltro`, `usePermissoes`, etc.)
- ✅ Hooks de formulário e validação
- ❌ Hooks triviais que apenas encapsulam `useState`/`useContext` sem lógica adicional

**Tipo de Teste:** Unitário
**Framework:** Vitest + React Testing Library (`renderHook`)
**Cobertura Mínima:** 70%

**Observações:**

- Focar em regras de negócio e transformação de dados dentro do hook
- Mockar dependências externas (serviços de API, contexto) injetadas ou importadas
- Hooks que somente repassam valores de contexto são excluídos, análogos aos QueryHandlers do backend

---

### 4. **Funções Utilitárias e Lógica Pura**

**Escopo:**

- ✅ Formatadores (datas, moeda, texto)
- ✅ Validadores (schemas Zod/Yup, funções de validação)
- ✅ Funções de transformação/mapeamento de dados

**Tipo de Teste:** Unitário
**Framework:** Vitest
**Cobertura Mínima:** 80%

**Observações:**

- Funções puras devem ter a maior cobertura da aplicação, por serem simples e de baixo custo de manutenção
- Testar *edge cases* (valores nulos, vazios, limites)

---

### 5. **Rotas e Páginas (Next.js App Router)**

#### 5.1. **Server Components**

**Escopo:**

- ✅ Lógica de *data fetching* server-side e composição de página

**Tipo de Teste:** Unitário (com mocks de funções de fetch de dados)
**Framework:** Vitest + React Testing Library
**Cobertura Mínima:** 60%

**Observações:**

- Extrair a lógica de busca de dados em funções isoladas (ex: `getProjetos()`), permitindo mock direto sem depender de infraestrutura do Next.js
- Testes de Server Components têm suporte experimental nas ferramentas atuais; priorizar testar a função de *data fetching* separadamente da renderização

#### 5.2. **Client Components de Página**

**Escopo:**

- ✅ Interatividade da página (filtros, paginação, formulários)

**Tipo de Teste:** Integração
**Framework:** Vitest + React Testing Library + MSW
**Cobertura Mínima:** 70%

---

### 6. **Route Handlers / API Routes (BFF do Next.js)**

**Escopo:**

- ✅ Handlers em `app/api/**/route.ts` que implementam lógica (transformação, validação, orquestração)
- ❌ Handlers que apenas fazem *proxy* direto para o backend sem lógica adicional

**Tipo de Teste:** Unitário (com mocks de serviços externos)
**Framework:** Vitest
**Cobertura Mínima:** 70%

**Observações:**

- Mockar `fetch`/clientes HTTP usados para chamar o backend
- Validar tratamento de erros e códigos de status retornados

---

### 7. **Estado Global (Context API / Store)**

**Escopo:**

- ✅ Reducers, actions e seletores de estado global
- ✅ Providers com lógica de inicialização

**Tipo de Teste:** Unitário
**Framework:** Vitest
**Cobertura Mínima:** 70%

---

### 8. **Estilos e Snapshots Visuais**

**Escopo:**

- ❌ Testes de *snapshot* de renderização (`toMatchSnapshot`)

**Tipo de Teste:** **NÃO TERÁ TESTES**

**Justificativa:**

- Snapshots tendem a quebrar com qualquer mudança visual, gerando aprovações automáticas sem revisão real (*snapshot fatigue*)
- Regressões visuais devem ser tratadas por revisão de Pull Request e, futuramente, por ferramentas de *visual regression testing* dedicadas (fora do escopo deste documento)

---

### 9. **Fluxos Críticos de Negócio (End-to-End)**

**Escopo:**

- ✅ Login e autenticação
- ✅ Criação, edição e exclusão de Projeto (fluxo completo)
- ✅ Fluxos de aprovação/mudança de status
- ❌ Todas as variações e *edge cases* (cobertos em testes de integração)

**Tipo de Teste:** E2E
**Framework:** Playwright
**Cobertura:** N/A (cobertura por fluxo, não por linha de código)

**Observações:**

- Executar contra ambiente de homologação ou backend com banco de dados de teste dedicado
- Priorizar poucos fluxos, porém críticos ("happy path" + 1-2 cenários de erro relevantes por fluxo)
- Testes E2E devem rodar em pipeline separado do unitário/integração (mais lento, executado antes de deploy)

---

### 10. **Configuração e Bootstrap**

**Escopo:**

- ❌ `next.config.js`, `middleware.ts` (configuração de roteamento), arquivos de layout raiz sem lógica

**Tipo de Teste:** **NÃO TERÁ TESTES**

**Justificativa:**

- Arquivos de configuração de infraestrutura da aplicação
- Validado indiretamente pelos testes E2E e pela própria inicialização da aplicação

---

## Resumo de Cobertura por Tipo

| Camada / Tipo                          | Componentes Testados                        | Tipo de Teste        | Framework                          | Cobertura Mínima | Status                                 |
| --------------------------------------- | -------------------------------------------- | --------------------- | ----------------------------------- | ----------------- | --------------------------------------- |
| **Componentes de UI**                   | Componentes reutilizáveis, formulários       | Unitário               | Vitest, RTL                         | 70%                | ✅ Implementar                           |
| **Componentes Conectados**              | Componentes com chamadas de API              | Integração             | Vitest, RTL, MSW                    | 70%                | ✅ Implementar                           |
| **Hooks Customizados**                  | Hooks com lógica de negócio                  | Unitário               | Vitest, RTL (`renderHook`)          | 70%                | ✅ Implementar                           |
| **Funções Utilitárias**                 | Formatadores, validadores, mapeadores        | Unitário               | Vitest                              | 80%                | ✅ Implementar                           |
| **Server Components**                   | Funções de data fetching                     | Unitário               | Vitest, RTL                         | 60%                | ✅ Implementar                           |
| **Client Components de Página**         | Páginas interativas                          | Integração             | Vitest, RTL, MSW                    | 70%                | ✅ Implementar                           |
| **Route Handlers (API Routes)**         | Handlers com lógica própria                  | Unitário               | Vitest                              | 70%                | ✅ Implementar                           |
| **Estado Global**                       | Reducers, actions, seletores                 | Unitário               | Vitest                              | 70%                | ✅ Implementar                           |
| **Snapshots visuais**                   | -                                             | -                      | -                                    | N/A                | ❌ Não testado                           |
| **Configuração/Bootstrap**              | -                                             | -                      | -                                    | N/A                | ❌ Não testado                           |
| **Fluxos Críticos de Negócio**          | Login, CRUD de Projeto, Aprovação            | E2E                    | Playwright                          | N/A (por fluxo)    | ✅ Implementar (fluxos essenciais)       |

---

## Alternativas Consideradas

### 1. Cypress ao invés de Playwright (E2E)

**Descrição**: Usar Cypress como ferramenta de testes E2E.

**Vantagens:**

- Ferramenta consolidada, grande comunidade
- Interface de depuração (Cypress Runner) amigável

**Desvantagens:**

- Suporte a múltiplas abas/domínios mais limitado
- Execução mais lenta em suítes grandes comparado ao Playwright
- Suporte a múltiplos browsers (WebKit) historicamente mais restrito

**Por que foi rejeitada**: Playwright oferece paralelização nativa mais eficiente, suporte first-class a Chromium/Firefox/WebKit e melhor integração com pipelines CI modernos, com esforço de aprendizado equivalente.

---

### 2. Jest ao invés de Vitest

**Descrição**: Usar Jest como test runner, ferramenta historicamente padrão no ecossistema React.

**Vantagens:**

- Extremamente maduro e amplamente documentado
- Grande base de exemplos e integrações prontas

**Desvantagens:**

- Configuração mais pesada para projetos com ESM/TypeScript/Next.js
- Execução mais lenta que Vitest (sem aproveitamento nativo do Vite/ESBuild)
- Watch mode e feedback de desenvolvimento mais lentos

**Por que foi rejeitada**: Vitest é compatível com a API do Jest (baixo custo de migração/aprendizado), porém com execução significativamente mais rápida e configuração mais simples em projetos TypeScript modernos.

---

### 3. Testes de Snapshot como Estratégia Principal

**Descrição**: Priorizar `toMatchSnapshot()` para validar renderização de componentes.

**Vantagens:**

- Implementação inicial rápida (gerado automaticamente)
- Detecta qualquer mudança na árvore renderizada

**Desvantagens:**

- Alta taxa de falsos positivos em qualquer alteração visual legítima
- Tendência da equipe a aprovar (`--update`) snapshots sem revisão criteriosa
- Não valida comportamento, apenas estrutura de saída

**Por que foi rejeitada**: Testes baseados em comportamento (React Testing Library) fornecem maior confiança e menor manutenção do que snapshots, que tendem a se tornar ruído com o tempo.

---

### 4. Enzyme ao invés de React Testing Library

**Descrição**: Usar Enzyme para testes de componentes, permitindo inspeção de estado interno e *shallow rendering*.

**Vantagens:**

- Permite acesso direto a estado e props internos do componente
- Suporte histórico consolidado em projetos legados React

**Desvantagens:**

- Sem suporte oficial ativo para versões recentes do React (18+)
- Incentiva testar detalhes de implementação em vez de comportamento observável pelo usuário
- Comunidade e manutenção em declínio

**Por que foi rejeitada**: Enzyme está descontinuado para as versões atuais do React. React Testing Library é a recomendação oficial da equipe do React e reduz o acoplamento entre testes e implementação interna.

---

## Consequências

### Positivas

- ✅ **Simplicidade de implementação**: Testes unitários e de integração não exigem browser real nem backend em execução, acelerando desenvolvimento
- ✅ **Velocidade de execução**: Vitest roda em milissegundos, fornecendo feedback instantâneo durante desenvolvimento (*watch mode*)
- ✅ **Testes resilientes a refatoração**: React Testing Library incentiva testes baseados em comportamento, não em detalhes de implementação
- ✅ **Rede realista e isolada**: MSW permite simular cenários reais de API (sucesso, erro, latência) sem depender de servidor ativo
- ✅ **Confiança nos fluxos críticos**: Playwright garante que os fluxos de negócio mais importantes funcionem ponta a ponta
- ✅ **CI/CD ágil**: Pipeline de testes unitários/integração executa em segundos; E2E roda em estágio separado, sem bloquear feedback rápido

### Negativas

- ⚠️ **Risco em Server Components não cobertos por E2E**: Bugs de composição/streaming podem não ser detectados apenas por testes unitários
- ⚠️ **Necessidade de disciplina**: Desenvolvedores precisam configurar handlers do MSW corretamente (risco de mocks que não refletem a API real)
- ⚠️ **Curva de aprendizado**: Equipe precisa dominar RTL (queries, `waitFor`, `userEvent`), MSW (handlers, servidor de mock) e Playwright
- ⚠️ **Manutenção de mocks de contrato**: Mudanças na API do backend exigem atualização dos handlers MSW em múltiplos testes
- ⚠️ **Cobertura limitada de fluxos E2E**: Apenas os fluxos críticos são cobertos; regressões em fluxos secundários dependem de testes de integração e revisão de PR

---

## Implementação

### 1. Estrutura de Projetos de Teste

Os testes devem ser estruturados de forma a espelhar a estrutura de pastas do projeto principal (co-localização), com testes E2E mantidos em pasta própria na raiz.

```
frontend/
├── src/
│   ├── components/
│   │   ├── ui/
│   │   │   ├── Button.tsx
│   │   │   └── Button.test.tsx           # Teste unitário do componente
│   │   ├── projetos/
│   │   │   ├── ProjetoForm.tsx
│   │   │   └── ProjetoForm.test.tsx      # Teste de integração (com MSW)
│   │
│   ├── hooks/
│   │   ├── useProjeto.ts
│   │   └── useProjeto.test.ts            # Teste unitário do hook
│   │
│   ├── lib/
│   │   ├── utils/
│   │   │   ├── formatarData.ts
│   │   │   └── formatarData.test.ts      # Teste unitário de função pura
│   │   └── validators/
│   │       ├── projetoSchema.ts
│   │       └── projetoSchema.test.ts
│   │
│   ├── app/
│   │   ├── projetos/
│   │   │   ├── page.tsx
│   │   │   ├── page.test.tsx             # Teste de página (Server Component)
│   │   │   └── ProjetosClient.tsx
│   │   │   └── ProjetosClient.test.tsx   # Teste de integração (Client Component)
│   │   └── api/
│   │       └── projetos/
│   │           ├── route.ts
│   │           └── route.test.ts         # Teste unitário do Route Handler
│   │
│   ├── store/
│   │   ├── projetoSlice.ts
│   │   └── projetoSlice.test.ts
│   │
│   └── mocks/
│       ├── handlers.ts                   # Handlers MSW compartilhados
│       ├── server.ts                     # Setup MSW para testes (node)
│       └── browser.ts                    # Setup MSW para desenvolvimento
│
├── e2e/
│   ├── auth.spec.ts
│   ├── projeto-crud.spec.ts
│   └── projeto-aprovacao.spec.ts
│
├── vitest.config.ts
├── vitest.setup.ts
└── playwright.config.ts
```

**Observações:**

- **IMPORTANTE**: Testes unitários e de integração ficam **co-localizados** junto ao arquivo testado (`Componente.tsx` + `Componente.test.tsx`), facilitando manutenção
- Testes E2E ficam isolados em `e2e/`, na raiz do projeto, por rodarem em pipeline e ambiente distintos
- Handlers MSW compartilhados ficam centralizados em `src/mocks/`, reutilizáveis entre testes e ambiente de desenvolvimento

---

### 2. Padrões de Implementação

#### 2.1. Estrutura AAA (Arrange-Act-Assert)

Todos os testes DEVEM seguir o padrão AAA com comentários explicativos:

```tsx
import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { faker } from "@faker-js/faker";
import { ProjetoForm } from "./ProjetoForm";

describe("ProjetoForm", () => {
  it("ComDadosValidos_DeveChamarOnSubmitComOsValoresPreenchidos", async () => {
    // ============ ARRANGE ============
    // Preparar dados de entrada usando Faker
    const nomeProjeto = faker.company.name();
    const onSubmitMock = vi.fn();
    const user = userEvent.setup();

    render(<ProjetoForm onSubmit={onSubmitMock} />);

    // ============ ACT ============
    // Simular interação do usuário
    await user.type(screen.getByLabelText(/nome do projeto/i), nomeProjeto);
    await user.click(screen.getByRole("button", { name: /salvar/i }));

    // ============ ASSERT ============
    // Verificar resultado
    expect(onSubmitMock).toHaveBeenCalledTimes(1);
    expect(onSubmitMock).toHaveBeenCalledWith(
      expect.objectContaining({ nome: nomeProjeto })
    );
  });
});
```

---

#### 2.2. Nomenclatura de Testes

**Formato obrigatório**: `[Cenário]_[ResultadoEsperado]` dentro de um bloco `describe` nomeado com a unidade testada.

**Exemplos:**

```tsx
// ✅ CORRETO
describe("ProjetoForm", () => {
  it("ComNomeVazio_DeveExibirMensagemDeErro", () => {});
  it("ComDadosValidos_DeveChamarOnSubmit", () => {});
});

describe("useProjeto", () => {
  it("QuandoIdInvalido_DeveRetornarErro", () => {});
  it("QuandoRequisicaoComSucesso_DeveRetornarProjeto", () => {});
});

// ❌ INCORRETO
it("test1", () => {});          // Não descritivo
it("funciona", () => {});       // Vago
it("testa o formulário", () => {}); // Não indica cenário nem resultado esperado
```

---

#### 2.3. Uso de Faker

**Gerar dados realistas para testes:**

```tsx
import { faker } from "@faker-js/faker";

// Configurar locale (uma vez, em setup global ou por teste)
faker.locale = "pt_BR";

// Gerar dados
const nome = faker.person.fullName();          // "João Silva"
const email = faker.internet.email();          // "joao.silva@exemplo.com"
const dataInicio = faker.date.recent({ days: 30 });
const descricao = faker.lorem.sentence();

// Gerar objeto complexo
const projeto = {
  id: faker.number.int({ min: 1, max: 1000 }),
  nome: faker.company.name(),
  descricao: faker.lorem.paragraph(),
  dataInicio: faker.date.recent({ days: 10 }),
  statusId: faker.number.int({ min: 1, max: 5 }),
};

// Gerar lista de objetos
const projetos = Array.from({ length: 10 }, () => ({
  id: faker.number.int(),
  nome: faker.company.name(),
  descricao: faker.lorem.paragraph(),
}));
```

---

#### 2.4. Uso de MSW (Mock Service Worker)

**Mockar chamadas HTTP em testes de integração:**

```tsx
// src/mocks/handlers.ts
import { http, HttpResponse } from "msw";

export const handlers = [
  http.get("/api/projetos/:id", ({ params }) => {
    return HttpResponse.json({
      id: params.id,
      nome: "Projeto Teste",
      statusId: 1,
    });
  }),

  http.post("/api/projetos", async ({ request }) => {
    const body = await request.json();
    return HttpResponse.json({ id: 123, ...body }, { status: 201 });
  }),
];
```

```tsx
// src/mocks/server.ts (usado nos testes com Vitest)
import { setupServer } from "msw/node";
import { handlers } from "./handlers";

export const server = setupServer(...handlers);
```

```tsx
// vitest.setup.ts
import { beforeAll, afterEach, afterAll } from "vitest";
import { server } from "./src/mocks/server";

beforeAll(() => server.listen({ onUnhandledRequest: "error" }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

```tsx
// ProjetosClient.test.tsx — sobrescrevendo handler para cenário específico
import { http, HttpResponse } from "msw";
import { server } from "@/mocks/server";
import { render, screen } from "@testing-library/react";
import { ProjetosClient } from "./ProjetosClient";

it("QuandoApiRetornaErro_DeveExibirMensagemDeErro", async () => {
  // Arrange: sobrescreve o handler padrão para simular erro 500
  server.use(
    http.get("/api/projetos", () => {
      return HttpResponse.json({ message: "Erro interno" }, { status: 500 });
    })
  );

  // Act
  render(<ProjetosClient />);

  // Assert
  expect(await screen.findByText(/erro ao carregar projetos/i)).toBeInTheDocument();
});
```

---

#### 2.5. Teste de Hooks Customizados

```tsx
import { renderHook, waitFor } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { useProjeto } from "./useProjeto";

describe("useProjeto", () => {
  it("QuandoIdValido_DeveRetornarProjetoCarregado", async () => {
    // Arrange & Act
    const { result } = renderHook(() => useProjeto(1));

    // Assert
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.projeto?.id).toBe(1);
  });
});
```

---

#### 2.6. Teste de Route Handler (API Route do Next.js)

```tsx
// app/api/projetos/route.test.ts
import { describe, it, expect, vi } from "vitest";
import { GET } from "./route";

vi.mock("@/lib/services/projetoService", () => ({
  buscarProjetos: vi.fn().mockResolvedValue([{ id: 1, nome: "Teste" }]),
}));

describe("GET /api/projetos", () => {
  it("QuandoSucesso_DeveRetornarStatus200ComListaDeProjetos", async () => {
    // Act
    const response = await GET(new Request("http://localhost/api/projetos"));
    const body = await response.json();

    // Assert
    expect(response.status).toBe(200);
    expect(body).toHaveLength(1);
  });
});
```

---

#### 2.7. Teste E2E com Playwright

```tsx
// e2e/projeto-crud.spec.ts
import { test, expect } from "@playwright/test";

test.describe("CRUD de Projeto", () => {
  test("Usuário_DeveCriarNovoProjetoComSucesso", async ({ page }) => {
    // Arrange
    await page.goto("/projetos/novo");

    // Act
    await page.getByLabel("Nome do Projeto").fill("Projeto E2E Teste");
    await page.getByRole("button", { name: "Salvar" }).click();

    // Assert
    await expect(page.getByText("Projeto criado com sucesso")).toBeVisible();
    await expect(page).toHaveURL(/\/projetos\/\d+/);
  });
});
```

---

### 3. Execução de Testes

**Executar testes unitários e de integração:**

```bash
# Executar todos os testes (unitário + integração)
npx vitest run

# Executar em modo watch (desenvolvimento)
npx vitest

# Executar com cobertura
npx vitest run --coverage

# Executar testes de um arquivo/pasta específica
npx vitest run src/components/projetos

# Filtrar por nome do teste
npx vitest run -t "ComDadosValidos"
```

**Executar testes E2E:**

```bash
# Executar todos os testes E2E
npx playwright test

# Executar em modo UI (depuração visual)
npx playwright test --ui

# Executar um arquivo específico
npx playwright test e2e/projeto-crud.spec.ts

# Gerar relatório HTML
npx playwright show-report
```

---

## Verificação de Conformidade

Use este checklist para auditar se a estratégia de testes está sendo seguida corretamente:

### Padrões de Código

- [ ] Testes unitários/integração estão co-localizados junto ao arquivo testado (`Componente.tsx` + `Componente.test.tsx`)
- [ ] Testes E2E estão centralizados na pasta `e2e/`
- [ ] Todos os testes seguem nomenclatura `[Cenário]_[ResultadoEsperado]` dentro de um `describe`
- [ ] Todos os testes seguem padrão AAA (Arrange-Act-Assert) com comentários
- [ ] Chamadas de rede são mockadas usando **MSW** (nunca mock direto de `fetch`/`axios`)
- [ ] Componentes são testados por comportamento observável (queries de `getByRole`/`getByLabelText`), não por `getByTestId` como primeira opção

### Cobertura

- [ ] **Componentes de UI**: Cobertura ≥ 70%
- [ ] **Componentes Conectados**: Cobertura ≥ 70% (com MSW)
- [ ] **Hooks Customizados**: Cobertura ≥ 70%
- [ ] **Funções Utilitárias**: Cobertura ≥ 80%
- [ ] **Route Handlers**: Cobertura ≥ 70%
- [ ] **Fluxos Críticos**: 100% dos fluxos definidos cobertos por E2E

### Qualidade

- [ ] Todos os testes passam localmente antes de commit
- [ ] Não existem testes ignorados (`.skip`) sem justificativa documentada
- [ ] Testes são independentes (não dependem de ordem de execução nem de estado compartilhado)
- [ ] Testes assíncronos usam `findBy*`/`waitFor` corretamente (sem `setTimeout` arbitrário)
- [ ] Suíte unitária/integração executa em < 2 minutos localmente
- [ ] Suíte E2E executa em pipeline separado, sem bloquear feedback rápido de desenvolvimento

### Implementação por Camada

- [ ] **Componentes de UI**: possuem testes de renderização, estados e interações
- [ ] **Componentes Conectados**: possuem testes de integração com MSW cobrindo sucesso e erro
- [ ] **Hooks**: possuem testes unitários com `renderHook`
- [ ] **Route Handlers com lógica**: possuem testes unitários; *proxies* simples NÃO possuem (conforme decisão)
- [ ] **Snapshots visuais**: NÃO são utilizados como estratégia principal (conforme decisão)
- [ ] **Fluxos críticos**: possuem testes E2E com Playwright

---

## Referências

- **DT-005**: Padrões de Estilo de Código Backend
- **DT-008**: Estratégia de Testes Unitários para Backend
- **Vitest Documentation**: https://vitest.dev/
- **React Testing Library Documentation**: https://testing-library.com/docs/react-testing-library/intro/
- **MSW (Mock Service Worker) Documentation**: https://mswjs.io/
- **Playwright Documentation**: https://playwright.dev/
- **Faker.js Repository**: https://github.com/faker-js/faker
- **Next.js Testing Documentation**: https://nextjs.org/docs/app/building-your-application/testing