/// <reference types="vite/client" />
/// <reference types="coalesce-vue-vuetify3" />

declare module "*.vue" {
  import type { DefineComponent } from "vue";
  // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/ban-types
  const component: DefineComponent<{}, {}, any>;
  export default component;
}

// declare module 'vuetify'
// declare module 'vuetify/lib/components'
// declare module 'vuetify/lib/directives'
