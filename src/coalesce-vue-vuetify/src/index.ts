/** index.ts is a treeshakable version of coalesce-vue-vuetify,
 * intended for use with the treeshakable version of vuetify (e.g. `vuetify/lib` import).
 * It does not register its own components globally, requiring use of techniques like
 * unplugin-vue-components (or tedious manual imports, or manual global imports) in consuming projects.
 *
 */

import { PluginObject } from "vue";
import { Domain } from "coalesce-vue";

// Import of a type from vue-router is needed for Vue.$router component instance prop to be typed during build.
import type { Route } from "vue-router";

export interface CoalesceVuetifyOptions {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  readonly metadata?: Domain;
}

declare module "vue/types/vue" {
  export interface Vue {
    readonly $coalesce: CoalesceVuetifyOptions;
  }
}

const Plugin = <PluginObject<CoalesceVuetifyOptions>>{
  install(Vue, options) {
    Vue.prototype.$coalesce = options ?? {};
  },
};

import "./shared.scss";
export * from "./components";
export * from "./util";
export default Plugin;
