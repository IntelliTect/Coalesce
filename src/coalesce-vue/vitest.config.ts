import { defineConfig, UserConfig } from "vitest/config";
import path from "path";

export default defineConfig({
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["test/global-setup.ts"],
    include: ["**/*.{test,spec}.{ts,js}"],
  },
  resolve: {
    alias: [
      // Imports inside the generated test targets:
      { find: "coalesce-vue/lib", replacement: path.resolve(__dirname, "src") },
      {
        find: "@test-targets",
        replacement: path.resolve(__dirname, "../test-targets"),
      },
    ],
  },
}) as UserConfig;
