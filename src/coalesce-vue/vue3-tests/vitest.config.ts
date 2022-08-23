import { defineConfig } from "vitest/config";
import path from "path";
import packageJson from "./package.json";

import config from '../vitest.config';
export default defineConfig({
  ...config,
  root: "../",
  define: {
    ...config.define,
    TEST_EXPECTED_VUE_VERSION: 3
  },
  resolve: {
    alias: Object.keys(packageJson.devDependencies).map((pkgName) => ({
      find: pkgName,
      replacement: path.resolve(__dirname, "node_modules", pkgName),
    })),
  },
  test: {
    ...config.test,
    include: ['**/*.{test,spec}.{ts,js}', '**/*.{test,spec}.vue3.{ts,js}']
  }
});
