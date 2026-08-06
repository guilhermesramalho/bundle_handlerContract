import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { server } from "@/mocks/server";
import type { components } from "@/types/api.generated";
import { ContratosPageClient } from "./ContratosPageClient";

type Item = components["schemas"]["ListarContratosResponse"];

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

let initialQuery = "";

vi.mock("next/navigation", () => ({
  useSearchParams: () => new URLSearchParams(initialQuery),
}));

function contratoFake(overrides: Partial<Item>): Item {
  return {
    id: 1,
    contratoId: "PCR 700.123",
    pcr: "700.123",
    clienteNome: "Auto Posto Aurora Ltda",
    clienteCnpj: "12345678000195",
    clienteNrSap: "1000000001",
    clienteUf: "SP",
    grupoEconomicoNome: "Grupo Solaris",
    segmento: "Rede",
    tipo: "Pcvm",
    galonagemContratada: 10080,
    galonagemFaturada: 0,
    galonagemPercentual: 0,
    situacaoPcr: "Vigente",
    fimVigencia: "2029-02-28",
    denuncia: false,
    bandeira: "Bandeira ALE",
    diretoria: "Sudeste",
    regionalVendas: "GR São Paulo Interior",
    pontoVenda: "RN Campinas",
    consultor: "Marcos Vidal",
    papelGuardaChuva: null,
    ...overrides,
  };
}

const CONTRATO_COFA = contratoFake({
  id: 2,
  contratoId: "PCF 700.999",
  pcr: "700.999",
  clienteNome: "Transportes Bandeirante S.A.",
  segmento: "Cofa",
  tipo: "Comodato",
  bandeira: "Bandeira Branca",
  diretoria: "Sul",
  regionalVendas: "GR Sul",
  pontoVenda: "RN Curitiba",
  consultor: "Helena Rocha",
  clienteUf: "PR",
  situacaoPcr: "VencidoPorData",
});

const DATASET = [contratoFake({}), CONTRATO_COFA];

function paginaFake(items: Item[], overrides?: Partial<{ pageIndex: number; totalPages: number }>) {
  return {
    items,
    pageIndex: overrides?.pageIndex ?? 1,
    pageSize: 20,
    totalCount: items.length,
    totalPages: overrides?.totalPages ?? 1,
  };
}

/** Registra o handler compartilhado: pageSize=100 é a chamada de opções de filtro (sempre o dataset completo, sem filtro); qualquer outro pageSize é a listagem principal (recebe o callback para customizar por teste). */
function mockContratosEndpoint(onListagem: (url: URL) => Item[] | { items: Item[]; totalPages: number }) {
  server.use(
    http.get(`${API_URL}/api/v1/contratos`, ({ request }) => {
      const url = new URL(request.url);
      if (url.searchParams.get("PageSize") === "100") {
        return HttpResponse.json(paginaFake(DATASET));
      }
      const resultado = onListagem(url);
      if (Array.isArray(resultado)) return HttpResponse.json(paginaFake(resultado));
      return HttpResponse.json(paginaFake(resultado.items, { totalPages: resultado.totalPages }));
    }),
  );
}

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <ContratosPageClient />
    </QueryClientProvider>,
  );
}

async function waitForRowsLoaded() {
  await waitFor(() => expect(screen.queryByText("Carregando contratos…")).not.toBeInTheDocument());
}

describe("ContratosPageClient", () => {
  beforeEach(() => {
    initialQuery = "";
  });

  it("CarregamentoInicial_ExibeAsLinhasRetornadasPelaApi", async () => {
    // Arrange
    mockContratosEndpoint(() => DATASET);

    // Act
    renderPage();
    await waitForRowsLoaded();

    // Assert
    expect(screen.getAllByRole("link")).toHaveLength(2);
    expect(screen.getByText("Auto Posto Aurora Ltda")).toBeInTheDocument();
    expect(screen.getByText("Transportes Bandeirante S.A.")).toBeInTheDocument();
  });

  it("ParametroQNaUrl_PreenchendoCampoDeBuscaEEnviaBuscaParaApi", async () => {
    // Arrange
    initialQuery = "q=Aurora";
    let buscaRecebida: string | null = null;
    mockContratosEndpoint((url) => {
      buscaRecebida = url.searchParams.get("Busca");
      return [DATASET[0]];
    });

    // Act
    renderPage();
    await waitForRowsLoaded();

    // Assert
    expect(screen.getByLabelText("Buscar contratos")).toHaveValue("Aurora");
    await waitFor(() => expect(buscaRecebida).toBe("Aurora"));
  });

  it("FiltroDeSegmentoCofa_EnviaSegmentosNaQueryStringDaApi", async () => {
    // Arrange
    const user = userEvent.setup();
    let segmentosRecebidos: string[] = [];
    mockContratosEndpoint((url) => {
      segmentosRecebidos = url.searchParams.getAll("Segmentos");
      return segmentosRecebidos.includes("Cofa") ? [CONTRATO_COFA] : DATASET;
    });
    renderPage();
    await waitForRowsLoaded();

    // Act
    await user.click(screen.getByRole("button", { name: "Filtro Segmento" }));
    await user.click(screen.getByRole("option", { name: "Cofa" }));

    // Assert
    await waitFor(() => expect(segmentosRecebidos).toEqual(["Cofa"]));
    await waitFor(() => expect(screen.getAllByRole("link")).toHaveLength(1));
  });

  it("OrdenacaoPorClienteAscEDesc_EnviaSortByESortDirectionParaApi", async () => {
    // Arrange
    const user = userEvent.setup();
    const sorts: { sortBy: string | null; sortDirection: string | null }[] = [];
    mockContratosEndpoint((url) => {
      sorts.push({ sortBy: url.searchParams.get("SortBy"), sortDirection: url.searchParams.get("SortDirection") });
      return DATASET;
    });
    renderPage();
    await waitForRowsLoaded();

    // Act
    await user.click(screen.getByRole("button", { name: /^Cliente/ }));
    await waitFor(() => expect(sorts.at(-1)).toEqual({ sortBy: "razao", sortDirection: "asc" }));
    await user.click(screen.getByRole("button", { name: /^Cliente/ }));

    // Assert
    await waitFor(() => expect(sorts.at(-1)).toEqual({ sortBy: "razao", sortDirection: "desc" }));
  });

  it("SemResultados_ExibeEstadoVazio", async () => {
    // Arrange
    mockContratosEndpoint(() => []);

    // Act
    renderPage();
    await waitForRowsLoaded();

    // Assert
    expect(await screen.findByText("Nenhum contrato encontrado para os filtros aplicados.")).toBeInTheDocument();
    expect(screen.queryAllByRole("link")).toHaveLength(0);
  });

  it("ComErroDaApi_ExibeMensagemDeErro", async () => {
    // Arrange
    server.use(http.get(`${API_URL}/api/v1/contratos`, () => HttpResponse.json({}, { status: 500 })));

    // Act
    renderPage();

    // Assert
    expect(await screen.findByText("Não foi possível carregar os contratos.")).toBeInTheDocument();
  });

  it("MaisDeUmaPagina_ExibePaginacaoENavegaAoClicarProxima", async () => {
    // Arrange
    const user = userEvent.setup();
    const pageIndexes: string[] = [];
    mockContratosEndpoint((url) => {
      pageIndexes.push(url.searchParams.get("PageIndex") ?? "");
      return { items: DATASET, totalPages: 3 };
    });
    renderPage();
    await waitForRowsLoaded();

    // Act
    expect(screen.getByText("Página 1 de 3")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: /Próxima/ }));

    // Assert
    await waitFor(() => expect(pageIndexes.at(-1)).toBe("2"));
  });

  it("BotaoLimparFiltros_RemoveFiltroAtivoERecarregaSemFiltro", async () => {
    // Arrange
    const user = userEvent.setup();
    let ultimaChamadaTinhaSegmento = false;
    mockContratosEndpoint((url) => {
      ultimaChamadaTinhaSegmento = url.searchParams.getAll("Segmentos").length > 0;
      return DATASET;
    });
    renderPage();
    await waitForRowsLoaded();
    await user.click(screen.getByRole("button", { name: "Filtro Segmento" }));
    await user.click(screen.getByRole("option", { name: "Cofa" }));
    await waitFor(() => expect(ultimaChamadaTinhaSegmento).toBe(true));

    // Act
    await user.click(screen.getByRole("button", { name: /Limpar filtros/ }));

    // Assert
    await waitFor(() => expect(ultimaChamadaTinhaSegmento).toBe(false));
  });
});
