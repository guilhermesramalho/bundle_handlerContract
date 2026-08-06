import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import path from "node:path";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: ["./vitest.setup.ts"],
    css: true,
    exclude: ["node_modules", ".next", "e2e"],
    coverage: {
      provider: "v8",
      reporter: ["text", "html"],
      include: ["src/**/*.{ts,tsx}"],
      exclude: [
        // Bootstrap/config sem lógica própria — ver DT-024 seção 10.
        "src/app/layout.tsx",
        "src/app/page.tsx",
        "src/app/fonts.ts",
        "src/**/*.test.{ts,tsx}",
        "src/components/ui/index.ts",
      ],
    },
  },
});
