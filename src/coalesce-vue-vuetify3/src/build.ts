import { existsSync, readFileSync } from "node:fs";

/** Component name resolver for unplugin-vue-components that resolves coalesce-vue-vuetify3 components. */
export function CoalesceVuetifyResolver() {
  // Read the actual component names and their file paths from the source code
  // so that we can never have a false match.
  const componentIndex = ["./components/index.d.ts", "./components/index.ts"]
    .map((f) => new URL(f, import.meta.url))
    .filter(existsSync)
    .map((f) => readFileSync(f, { encoding: "utf-8" }))[0];

  // Build a map from component name to its direct subpath,
  // e.g. "CSelect" -> "coalesce-vue-vuetify3/components/input/c-select.js"
  const componentMap = new Map<string, string>();
  const exportRegex =
    /export\s*\{\s*default\s+as\s+(C[A-Z][A-Za-z]+)\s*\}\s*from\s*"\.\/(.+?)\.vue"/g;
  let match: RegExpExecArray | null;
  while ((match = exportRegex.exec(componentIndex)) !== null) {
    const [, name, relPath] = match;
    componentMap.set(name, `coalesce-vue-vuetify3/components/${relPath}.js`);
  }

  return {
    type: "component",
    resolve: (name: string) => {
      const from = componentMap.get(name);
      if (from) return { name: "default", as: name, from };
    },
  } as const;
}
