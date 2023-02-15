/** Component name resolver for unplugin-vue-components. */

import { join } from "node:path";
import { existsSync, readFileSync } from "node:fs";

export function CoalesceVuetifyResolver() {
  // Read the actual component names from the source code
  // so that we can never have a false match.
  const componentExports = ["components/index.d.ts", "components/index.ts"]
    .map((f) => join(__dirname, f))
    .filter(existsSync)
    .map((f) => readFileSync(f, { encoding: "utf-8" }))[0];

  const components = new Set(componentExports.match(/C[A-Z][A-Za-z]+/g));

  return {
    type: "component",
    resolve: (name: string) => {
      if (components.has(name)) return { name, from: "coalesce-vue-vuetify3" };
    },
  } as const;
}
