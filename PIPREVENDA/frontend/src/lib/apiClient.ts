import createClient from "openapi-fetch";
import type { paths } from "@/types/api.generated";

/**
 * Cliente HTTP tipado contra o OpenAPI real da API .NET (PortalAle.Api) —
 * `src/types/api.generated.ts` é gerado via `npm run generate:api-types`
 * (skill arquitetura-frontend-nextjs: "nunca escrever tipo de resposta de
 * API manualmente"). Rodar o script de novo sempre que o contrato da API
 * mudar (novo endpoint, campo novo, etc.).
 */
export const apiClient = createClient<paths>({
  baseUrl: process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080",
  // Não passar `fetch: globalThis.fetch` direto — isso capturaria a referência
  // no momento da criação do client (import time), antes do MSW substituir
  // `globalThis.fetch` no `beforeAll` dos testes, fazendo toda chamada furar o
  // mock e cair na rede de verdade. O wrapper abaixo resolve `globalThis.fetch`
  // a cada chamada, pegando sempre a versão (real ou interceptada) vigente.
  fetch: (...args: Parameters<typeof fetch>) => globalThis.fetch(...args),
});
