import { defineConfig } from "vitest/config";
import path from "path";
import packageJson from "./package.json";

import config from "../vitest.config";
export default defineConfig({
  ...config,
  root: "../",
  define: {
    ...config.define,
    TEST_EXPECTED_VUE_VERSION: 3,
  },
  resolve: {
    alias: [
      ...(config.resolve!.alias as []),

      // Override vue3-specific dependencies
      ...Object.keys(packageJson.devDependencies).map((pkgName) => ({
        find: pkgName,
        replacement: path.resolve(__dirname, "node_modules", pkgName),
      })),
    ],
  },
  test: {
    ...config.test,
    include: ["**/*.{test,spec}.{ts,js}", "**/*.{test,spec}.vue3.{ts,js}"],
  },
  server: {
    fs: {
      // Since recent-ish versions of vite or maybe vitest (vite 5+)?,
      // the test global setup follows the filesystem serving rules,
      // and these rules don't (no longer?) account for the configured `root`.
      // The default allow is always relative to the location of the vite config file,
      // but we need to allow reads from the parent dir for global-setup to be found.
      // Strangely, relative allows here ARE relative to the root, even though the defaults are not.
      allow: ["./"],
    },
  },
});
