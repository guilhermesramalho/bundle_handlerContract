import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";
import { server } from "@/mocks/server";
import { useContratos, useContratosOpcoesFiltro } from "./useContratos";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

function paginaFake(overrides?: Partial<{ totalCount: number; items: unknown[] }>) {
  return {
    items: overrides?.items ?? [],
    pageIndex: 1,
    pageSize: 20,
    totalCount: overrides?.totalCount ?? 0,
    totalPages: 1,
  };
}

describe("useContratos", () => {
  it("ComSucesso_RetornaAPaginaDeContratosDaApi", async () => {
    // Arrange
    server.use(
      http.get(`${API_URL}/api/v1/contratos`, () =>
        HttpResponse.json(paginaFake({ totalCount: 1, items: [{ id: 1, pcr: "700.123" }] })),
      ),
    );

    // Act
    const { result } = renderHook(() => useContratos({}), { wrapper });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.totalCount).toBe(1);
    expect(result.current.data?.items).toHaveLength(1);
  });

  it("ComFalhaDaApi_RetornaEstadoDeErro", async () => {
    // Arrange
    server.use(http.get(`${API_URL}/api/v1/contratos`, () => HttpResponse.json({}, { status: 500 })));

    // Act
    const { result } = renderHook(() => useContratos({}), { wrapper });
    await waitFor(() => expect(result.current.isError).toBe(true));

    // Assert
    expect(result.current.isError).toBe(true);
  });

  it("ComFiltros_EnviaOsParametrosCorretosNaQueryString", async () => {
    // Arrange
    let urlCapturada: URL | null = null;
    server.use(
      http.get(`${API_URL}/api/v1/contratos`, ({ request }) => {
        urlCapturada = new URL(request.url);
        return HttpResponse.json(paginaFake());
      }),
    );

    // Act
    const { result } = renderHook(
      () => useContratos({ busca: "Aurora", segmentos: ["Rede", "Grr"], pageIndex: 2, pageSize: 10 }),
      { wrapper },
    );
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(urlCapturada).not.toBeNull();
    expect(urlCapturada!.searchParams.get("Busca")).toBe("Aurora");
    expect(urlCapturada!.searchParams.getAll("Segmentos")).toEqual(["Rede", "Grr"]);
    expect(urlCapturada!.searchParams.get("PageIndex")).toBe("2");
    expect(urlCapturada!.searchParams.get("PageSize")).toBe("10");
  });
});

describe("useContratosOpcoesFiltro", () => {
  it("SemFiltro_BuscaComPageSizeNoMaximoPermitidoPelaApi", async () => {
    // Arrange
    let urlCapturada: URL | null = null;
    server.use(
      http.get(`${API_URL}/api/v1/contratos`, ({ request }) => {
        urlCapturada = new URL(request.url);
        return HttpResponse.json(paginaFake({ totalCount: 14 }));
      }),
    );

    // Act
    const { result } = renderHook(() => useContratosOpcoesFiltro(), { wrapper });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(urlCapturada!.searchParams.get("PageSize")).toBe("100");
  });
});
