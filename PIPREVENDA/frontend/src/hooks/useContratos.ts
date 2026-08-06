import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import type { components } from "@/types/api.generated";

export type Segmento = components["schemas"]["Segmento"];
export type TipoContrato = components["schemas"]["TipoContrato"];
export type SituacaoPcr = components["schemas"]["SituacaoPcr"];
export type ListarContratosResponse = components["schemas"]["ListarContratosResponse"];
export type PaginationResponse = components["schemas"]["PaginationResponseOfListarContratosResponse"];

export interface ListarContratosParams {
  busca?: string;
  segmentos?: Segmento[];
  tipos?: TipoContrato[];
  situacoesPcr?: SituacaoPcr[];
  emDenuncia?: boolean;
  bandeira?: string;
  ufs?: string[];
  diretoria?: string;
  regionalVendas?: string;
  pontoVenda?: string;
  consultor?: string;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
  pageIndex?: number;
  pageSize?: number;
}

async function fetchContratos(params: ListarContratosParams): Promise<PaginationResponse> {
  const { data, error } = await apiClient.GET("/api/v1/contratos", {
    params: {
      query: {
        Busca: params.busca || undefined,
        Segmentos: params.segmentos?.length ? params.segmentos : undefined,
        Tipos: params.tipos?.length ? params.tipos : undefined,
        SituacoesPcr: params.situacoesPcr?.length ? params.situacoesPcr : undefined,
        EmDenuncia: params.emDenuncia,
        Bandeira: params.bandeira || undefined,
        Ufs: params.ufs?.length ? params.ufs : undefined,
        Diretoria: params.diretoria || undefined,
        RegionalVendas: params.regionalVendas || undefined,
        PontoVenda: params.pontoVenda || undefined,
        Consultor: params.consultor || undefined,
        SortBy: params.sortBy || undefined,
        SortDirection: params.sortDirection,
        PageIndex: params.pageIndex,
        PageSize: params.pageSize,
      },
    },
  });

  if (error) throw new Error("Não foi possível carregar os contratos.");
  return data;
}

/**
 * Listagem paginada/filtrada/ordenada — server-side (GET /api/v1/contratos),
 * substitui o mock da Fase F2 (ver PLANO-IMPLEMENTACAO-FRONTEND.md Fase F5).
 * `keepPreviousData`: mantém a página atual visível durante o refetch (evita
 * a tabela "piscar" vazia a cada filtro/ordenação/paginação).
 */
export function useContratos(params: ListarContratosParams) {
  return useQuery({
    queryKey: ["contratos", params],
    queryFn: () => fetchContratos(params),
    placeholderData: keepPreviousData,
  });
}

/**
 * Snapshot "todos os contratos" (sem filtro, `pageSize` no máximo permitido
 * pela API — DT-019, 100) usado só para popular as opções dos filtros
 * (segmento/tipo/bandeira/UF/hierarquia comercial), não para exibir na
 * tabela. Funciona bem no volume atual (~14 contratos seedados); em
 * produção (+20 mil contratos, ver contexto seção 12) essa abordagem não
 * escala — precisaria de um endpoint dedicado de valores distintos. Ver
 * PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 2 para outros gaps documentados
 * dessa mesma natureza.
 */
export function useContratosOpcoesFiltro() {
  return useQuery({
    queryKey: ["contratos", "opcoes-filtro"],
    queryFn: () => fetchContratos({ pageSize: 100 }),
    staleTime: 5 * 60_000,
  });
}
