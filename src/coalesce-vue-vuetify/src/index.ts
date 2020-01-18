import { PluginObject } from 'vue';
import ComponentsInstaller from './components';
export * from './components';

import { Domain } from 'coalesce-vue';

export interface CoalesceVuetifyOptions {
  /** A reference to the whole set of Coalesce-generated metadata for the application,
   * as exported from `metadata.g.ts`, e.g. `import metadata from '@/metadata.g'`. */
  readonly metadata?: Domain;
}

declare module 'vue/types/vue' {
  export interface Vue {
    readonly $coalesce: CoalesceVuetifyOptions
  }
}

const Plugin = <PluginObject<CoalesceVuetifyOptions>>{
  install(Vue, options) {
    Vue.prototype.$coalesce = options ?? {};
    Vue.use(ComponentsInstaller)
  }
}

export default Plugin;