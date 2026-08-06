import type { Metadata } from "next";
import { neoSansPro, lato, inter } from "./fonts";
import { AppShell } from "@/components/shell/AppShell";
import { Providers } from "./providers";
import "./globals.css";

export const metadata: Metadata = {
  title: "PIPREVENDA — Gestão de Contratos",
  description: "Sistema de Gestão de Contratos ALE Combustíveis",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="pt-BR" className={`${neoSansPro.variable} ${lato.variable} ${inter.variable}`}>
      <head>
        {/* Material Symbols Rounded — usado pelo componente Icon do design
            system. Não embutido no bundle (carregado via CDN no protótipo). */}
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
        <link
          href="https://fonts.googleapis.com/css2?family=Material+Symbols+Rounded:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200"
          rel="stylesheet"
        />
      </head>
      <body>
        <Providers>
          <AppShell>{children}</AppShell>
        </Providers>
      </body>
    </html>
  );
}
