import { Suspense } from "react";
import { ContratosPageClient } from "./ContratosPageClient";

export const metadata = {
  title: "Contratos — PIPREVENDA",
};

/**
 * Tela puramente CSR (skill arquitetura-frontend-nextjs: SSR não se justifica
 * para listagem paginada com +20 mil contratos). O Suspense aqui é só o
 * boundary exigido pelo Next para `useSearchParams` no client component.
 */
export default function ContratosPage() {
  return (
    <Suspense fallback={null}>
      <ContratosPageClient />
    </Suspense>
  );
}
