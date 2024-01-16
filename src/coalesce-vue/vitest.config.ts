import { defineConfig, UserConfig } from "vitest/config";
import path from "path";

export default defineConfig({
  define: {
    TEST_EXPECTED_VUE_VERSION: 2,
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["test/global-setup.ts"],
    include: ["**/*.{test,spec}.{ts,js}", "**/*.{test,spec}.vue2.{ts,js}"],
  },
  resolve: {
    alias: [
      // Imports inside the generated test targets:
      { find: "coalesce-vue/lib", replacement: path.resolve(__dirname, "src") },
    ],
  },
}) as UserConfig;
