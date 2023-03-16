/** Component name resolver for unplugin-vue-components. */

let moduleName: Promise<string>;
export function CoalesceVuetifyResolver() {
  moduleName = (async () => {
    // See if coalesce-vue-vuetify2 was aliased in package.json as coalesce-vue-vuetify.
    // We have to do so in a way that will work in both ESM and CJS.

    if ("require" in global) {
      try {
        require.resolve("coalesce-vue-vuetify");
        return "coalesce-vue-vuetify";
      } catch {
        /* not cjs */
      }
    } else {
      try {
        if (await import.meta.resolve?.("coalesce-vue-vuetify"))
          return "coalesce-vue-vuetify";
      } catch {
        /* not esm */
      }
    }
    return "coalesce-vue-vuetify2";
  })();

  return {
    type: "component",
    resolve: async (name: string) => {
      if (name.match(/^C[A-Z]/))
        return { name, from: (await moduleName) + "/lib" };
    },
  } as const;
}
