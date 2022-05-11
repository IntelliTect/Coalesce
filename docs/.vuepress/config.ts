import { defaultTheme, SidebarGroup } from '@vuepress/theme-default'
import { shikiPlugin } from '@vuepress/plugin-shiki'
import { registerComponentsPlugin } from '@vuepress/plugin-register-components'
import { importMdPlugin } from './import-md-plugin'
import path from 'path'
import fs from 'fs'
 
export default {
  lang: 'en-US',
  title: 'Hello, VuePress!',
  description: 'This is my first VuePress site',
  plugins: [
    importMdPlugin(),
    shikiPlugin({
      theme: 'dark-plus'
    }),
    registerComponentsPlugin({
      componentsDir: path.resolve(__dirname, './components'),
    }),
  ],
  theme: defaultTheme({
    sidebar: [
      "/",
      {
        text: 'Model Types',
        children: [ 
          '/modeling/model-types/entities',
          '/modeling/model-types/external-types',
          '/modeling/model-types/dtos',
          '/modeling/model-types/services'
         ],
      },
      {
        text: 'Model Components',
        children: [
          '/modeling/model-components/properties',
          {
            text: "Attributes",
            link: '/modeling/model-components/attributes',
            collapsible: true,
            children: fs
              .readdirSync(path.resolve(__dirname, '../modeling/model-components/attributes'))
              .map(f => '/modeling/model-components/attributes/' + f)
          } as SidebarGroup /* https://github.com/vuepress/vuepress-next/issues/883 */,
          '/modeling/model-components/methods',
          '/modeling/model-components/data-sources',
          '/modeling/model-components/behaviors'
         ],
      },
      {
        text: 'Generated Code',
        // collapsible: false,
        children: [
          '/stacks/agnostic/generation',
          '/stacks/agnostic/dtos'
         ],
      },
      {
        text: 'Client - Vue',
        // collapsible: false,
        children: [
          '/stacks/vue/overview',
          '/stacks/vue/getting-started',
          '/stacks/vue/layers/metadata',
          '/stacks/vue/layers/models',
          '/stacks/vue/layers/api-clients',
          '/stacks/vue/layers/viewmodels',
          {
            text: "Vuetify Components",
            link: '/stacks/vue/coalesce-vue-vuetify/overview',
            collapsible: true,
            children: fs
              .readdirSync(path.resolve(__dirname, '../stacks/vue/coalesce-vue-vuetify/components'))
              .map(f => '/stacks/vue/coalesce-vue-vuetify/components/' + f)
          } as SidebarGroup /* https://github.com/vuepress/vuepress-next/issues/883 */,
         ],
      },
      {
        text: 'Client - Knockout',
        collapsible: true,
        children: [
          '/stacks/ko/overview',
          '/stacks/ko/getting-started',
          '/stacks/ko/client/view-model',
          '/stacks/ko/client/list-view-model',
          '/stacks/ko/client/external-view-model',
          '/stacks/ko/client/methods',
          '/stacks/ko/client/model-config',
          '/stacks/ko/client/bindings',
         ],
      },
      {
        text: 'Concepts',
        // collapsible: false,
        children: [
          '/concepts/include-tree',
          '/concepts/includes',
         ],
      },
      {
        text: 'Configuration',
        // collapsible: false,
        children: [
          '/topics/startup',
          '/topics/coalesce-json',
         ],
      }
    ]
  }),
}
