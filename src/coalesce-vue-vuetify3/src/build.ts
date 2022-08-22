/** Component name resolver for unplugin-vue-components. */

export function CoalesceVuetifyResolver() {
  return {
    type: "component",
    resolve: (name: string) => {
      if (name.match(/^C[A-Z]/)) return { name, from: "coalesce-vue-vuetify" };
    },
  } as const;
}
