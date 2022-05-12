import { defineUserConfig } from 'vuepress'

import { defaultTheme, DefaultThemeOptions, SidebarGroup } from '@vuepress/theme-default'
import { shikiPlugin } from '@vuepress/plugin-shiki'
import { registerComponentsPlugin } from '@vuepress/plugin-register-components'
import { searchPlugin } from '@vuepress/plugin-search'

import { importMdPlugin } from './importMdPlugin'

import path from 'path'
import fs from 'fs'

const baseUrl = "/Coalesce" // for GH pages

export default defineUserConfig({
  base: `${baseUrl}/`,
  lang: 'en-US',
  title: 'Coalesce',
  description: 'Documentation for IntelliTect.Coalesce',
  plugins: [
    importMdPlugin(),
    {
      // Workaround for https://github.com/vuepress/vuepress-next/issues/653
      // Prepend absolute URLs with our GH pages base URL.
      name: "baseify-link",
      extendsMarkdown(md) {
        const old = md.normalizeLink;
        md.normalizeLink = (url) => {
          url = old(url);
          if (url.startsWith("/") && !url.startsWith(baseUrl)) {
            url = baseUrl + url
          }
          return url;
        }
      }
    },
    shikiPlugin({
      theme: 'dark-plus'
    }),
    registerComponentsPlugin({
      componentsDir: path.resolve(__dirname, './components'),
    }),
    // Temporary until we deploy out on GH and can then apply for algolia
    searchPlugin({
      // options
      getExtraFields: (page) => [...page.contentRendered.matchAll(/def="(.*?)"/g)].map(result => result[1]),
    }),
  ],
  theme: defaultTheme({
    contributors: false,
    repo: 'intellitect/coalesce',
    docsDir: 'docs',
    docsBranch: 'dev',
    sidebar: [
      {
        text: "Introduction",
        link: "/",
        children: [
          '/stacks/vue/getting-started',
          '/stacks/ko/getting-started',
        ]
      },
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
            link: '/modeling/model-components/attributes.html',
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
          '/stacks/vue/layers/metadata',
          '/stacks/vue/layers/models',
          '/stacks/vue/layers/api-clients',
          '/stacks/vue/layers/viewmodels',
          {
            text: "Vuetify Components",
            link: '/stacks/vue/coalesce-vue-vuetify/overview.html',
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
})
