import { defaultTheme } from '@vuepress/theme-default'

export default {
  lang: 'en-US',
  title: 'Hello, VuePress!',
  description: 'This is my first VuePress site',
  theme: defaultTheme({
    sidebar: [
      "/",
      {
        text: 'Model Types',
        // collapsible: false,
        children: [ 
          '/modeling/model-types/entities',
          '/modeling/model-types/external-types',
          '/modeling/model-types/dtos',
          '/modeling/model-types/services'
         ],
      },
      {
        text: 'Model Components',
        // collapsible: false,
        children: [
          '/modeling/model-components/properties',
          '/modeling/model-components/attributes',
          '/modeling/model-components/methods',
          '/modeling/model-components/data-sources',
          '/modeling/model-components/behaviors'
         ],
      },
      {
        text: 'Server-side Generated Code',
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
          '/stacks/vue/coalesce-vue-vuetify/overview'
         ],
      },
      {
        text: 'Client - Knockout',
        // collapsible: false,
        children: [
          'stacks/ko/overview',
          'stacks/ko/getting-started',
          'stacks/ko/client/view-model',
          'stacks/ko/client/list-view-model',
          'stacks/ko/client/external-view-model',
          'stacks/ko/client/methods',
          'stacks/ko/client/model-config',
          'stacks/ko/client/bindings',
         ],
      },
      {
        text: 'Concepts',
        // collapsible: false,
        children: [
          'concepts/include-tree',
          'concepts/includes',
         ],
      },
      {
        text: 'Configuration',
        // collapsible: false,
        children: [
          'topics/startup',
          'topics/coalesce-json',
         ],
      }
    ]
  }),
}
