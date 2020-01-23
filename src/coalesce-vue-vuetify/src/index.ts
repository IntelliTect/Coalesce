import { PluginObject } from 'vue';
import { Domain } from 'coalesce-vue';
import ComponentsInstaller from './components';


import type { Route } from 'vue-router';

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

import './shared.scss'
export * from './components';
export default Plugin;