import type { ESLint, Linter } from "eslint";
import noLoadBeforeAutoLoad from "./rules/no-load-before-auto-load.js";
import noSortInComputed from "./rules/no-sort-in-computed.js";
import noStaticRouterImport from "./rules/no-static-router-import.js";

const rules = {
  "no-load-before-auto-load": noLoadBeforeAutoLoad,
  "no-sort-in-computed": noSortInComputed,
  "no-static-router-import": noStaticRouterImport,
} satisfies ESLint.Plugin["rules"];

const plugin: ESLint.Plugin = {
  rules,
};

/**
 * A flat config that enables all recommended rules as warnings.
 * Usage: `import coalesce from "eslint-plugin-coalesce";`
 * Then include `coalesce.configs.recommended` in your eslint config array.
 */
const recommended: Linter.Config = {
  name: "coalesce/recommended",
  plugins: { coalesce: plugin },
  rules: {
    "coalesce/no-load-before-auto-load": "warn",
    "coalesce/no-sort-in-computed": "warn",
    "coalesce/no-static-router-import": "warn",
  },
};

plugin.configs = { recommended };

export default plugin;
