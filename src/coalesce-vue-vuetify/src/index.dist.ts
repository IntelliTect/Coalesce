/** index.dist.ts is a non-treeshakable version of coalesce-vue-vuetify, 
 * intended for use with the non-treeshakable version of vuetify (e.g. non `/lib` import.) */

import { PluginObject } from 'vue';

import BasePlugin, { CoalesceVuetifyOptions } from './index';
import * as components from './components';

const Plugin = <PluginObject<CoalesceVuetifyOptions>>{
  install(Vue, options) {
    Vue.use(BasePlugin, options);

    for (const key in components) {
      const component = (components as any)[key]
      Vue.component(key, component as typeof Vue)
    }
  }
}

import './shared.scss'
export * from './components';
export * from './util';
export default Plugin;