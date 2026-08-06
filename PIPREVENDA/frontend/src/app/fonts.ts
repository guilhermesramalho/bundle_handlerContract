import localFont from "next/font/local";
import { Inter } from "next/font/google";

// Neo Sans Pro (display) e Lato (corpo/UI) — arquivos extraídos do bundle do
// protótipo (docs/modelo-frontend/prototipo.html), auto-hospedados em
// public/fonts/. Ver PLANO-IMPLEMENTACAO-FRONTEND.md seção 2: licença de
// redistribuição da Neo Sans Pro (Monotype) ainda não confirmada — revisar
// antes de publicar em produção.
export const neoSansPro = localFont({
  src: [
    { path: "../../public/fonts/neo-sans-pro/NeoSansPro-Light.otf", weight: "300", style: "normal" },
    { path: "../../public/fonts/neo-sans-pro/NeoSansPro-Regular.otf", weight: "400", style: "normal" },
    { path: "../../public/fonts/neo-sans-pro/NeoSansPro-Medium.otf", weight: "500", style: "normal" },
    { path: "../../public/fonts/neo-sans-pro/NeoSansPro-Bold.otf", weight: "700", style: "normal" },
  ],
  variable: "--font-neo-sans-pro-raw",
  display: "swap",
});

export const lato = localFont({
  src: [
    { path: "../../public/fonts/lato/Lato-Light.ttf", weight: "300", style: "normal" },
    { path: "../../public/fonts/lato/Lato-Regular.ttf", weight: "400", style: "normal" },
    { path: "../../public/fonts/lato/Lato-Italic.ttf", weight: "400", style: "italic" },
    { path: "../../public/fonts/lato/Lato-SemiBold.ttf", weight: "600", style: "normal" },
    { path: "../../public/fonts/lato/Lato-SemiBoldItalic.ttf", weight: "600", style: "italic" },
    { path: "../../public/fonts/lato/Lato-Bold.ttf", weight: "700", style: "normal" },
    { path: "../../public/fonts/lato/Lato-BoldItalic.ttf", weight: "700", style: "italic" },
    { path: "../../public/fonts/lato/Lato-Black.ttf", weight: "900", style: "normal" },
  ],
  variable: "--font-lato-raw",
  display: "swap",
});

// Inter (dados densos/micro) — via next/font/google, mais simples que
// replicar os unicode-range fragmentados do export original do protótipo.
export const inter = Inter({
  subsets: ["latin"],
  variable: "--font-inter-raw",
  display: "swap",
});
