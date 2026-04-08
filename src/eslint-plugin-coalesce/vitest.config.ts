import { defineConfig, UserConfig } from "vitest/config";

export default defineConfig({
  test: {
    globals: true,
    environment: "node",
    include: ["**/*.{test,spec}.{ts,js}"],
    exclude: ["**/node_modules/**", "**/lib/**"],
  },
}) as UserConfig;
