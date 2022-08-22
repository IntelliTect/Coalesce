# Vuetify Components

<!-- MARKER:summary -->

[![](https://img.shields.io/npm/v/coalesce-vue-vuetify2/dev?color=42b883&label=coalesce-vue-vuetify2%40dev)](https://www.npmjs.com/package/coalesce-vue-vuetify2)

The [Vue](https://vuejs.org/) stack for Coalesce provides [a set of components](/stacks/vue/coalesce-vue-vuetify/overview.md) based on [Vuetify](https://vuetifyjs.com/), packaged up in an NPM package [coalesce-vue-vuetify](https://www.npmjs.com/package/coalesce-vue-vuetify). These components are driven primarily by the [Metadata Layer](/stacks/vue/layers/metadata.md), and include both low level input and display components like [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) and [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) that are highly reusable in the custom pages you'll build in your application, as well as high-level components like [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md) and [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md) that constitute entire pages. 

<!-- MARKER:summary-end -->

[[toc]]

## Setup

::: tip
The template described in [Getting Started with Vue](/stacks/vue/getting-started.md) already includes all the necessary setup. You can skip this section if you started from the template.
:::

First, ensure that NPM package [coalesce-vue-vuetify2](https://www.npmjs.com/package/coalesce-vue-vuetify2) is installed in your project. The examples below assume it has been aliased to `coalesce-vue-vuetify` - in your package.json: `"coalesce-vue-vuetify": "npm:coalesce-vue-vuetify2@some.version"`. If you do not alias it, adjust the import/require statements accordingly.

### Setup with Vuetify A-la-carte
If you're using [Vuetify](https://vuetifyjs.com/)'s [A-la-carte builds](https://vuetifyjs.com/en/customization/a-la-carte/), then similar to importing Vuetify from `'vuetify/lib'` rather than `'vuetify'`, you should import `'coalesce-vue-vuetify'` from `'coalesce-vue-vuetify/lib'`. 

This is an alternate build that itself imports Vuetify components from `'vuetify/lib'`, therefore preventing duplication of components. Similar to Vuetify, this build also does not register the Coalesce components globally, allowing them to also be treeshaken.

<CodeTabs name="vue-bundler">
<template #vue-cli>

Install `unplugin-vue-components`, and add the following configuration to `vue.config.js`:

``` ts
// vue.config.js
configureWebpack: {
  plugins: [
    require('unplugin-vue-components/webpack')({
      dts: false,
      resolvers: [
        // If VuetifyResolver is used, `vuetify-loader` + `vue-cli-plugin-vuetify` can be uninstalled.
        require('unplugin-vue-components/resolvers').VuetifyResolver(),
        require('coalesce-vue-vuetify/lib/build').CoalesceVuetifyResolver(),
      ],
    }),
  ],
}
```

</template>
<template #vite>

Install `unplugin-vue-components`, and add it to your `vite.config.ts`:

``` ts
// vite.config.js
import Components from "unplugin-vue-components/vite";
import { VuetifyResolver } from "unplugin-vue-components/resolvers";
import { CoalesceVuetifyResolver } from "coalesce-vue-vuetify/lib/build";

// defineConfig
plugins: [
  // createVuePlugin(), etc...
  Components({
    dts: false,
    resolvers: [VuetifyResolver(), CoalesceVuetifyResolver()],
  }),
]
```

</template>
</CodeTabs>

Then, in your [Vue](https://vuejs.org/) application's ``main.ts`` file, you need to add the ``coalesce-vue-vuetify`` [plugin](https://vuejs.org/v2/guide/plugins.html) to your application, like so:

``` ts
import $metadata from '@/metadata.g';
// viewmodels.g has side-effects - it populates the global lookup on ViewModel and ListViewModel. 
// It must be imported for c-admin-editor-page and c-admin-table-page to work correctly.
import '@/viewmodels.g';

import CoalesceVuetify from 'coalesce-vue-vuetify/lib';
Vue.use(CoalesceVuetify, { metadata: $metadata, });
```

If you have routes to [c-admin-editor-page](./components/c-admin-editor-page.md) or [c-admin-table-page](./components/c-admin-table-page.md), make sure that those components are also imported from `coalesce-vue-vuetify/lib`.


### Setup without Vuetify A-la-carte

In your [Vue](https://vuejs.org/) application's ``main.ts`` file, you need to add the ``coalesce-vue-vuetify`` [plugin](https://vuejs.org/v2/guide/plugins.html) to your application, like so:

``` ts
import $metadata from '@/metadata.g';
// viewmodels.g has side-effects - it populates the global lookup on ViewModel and ListViewModel. 
// It must be imported for c-admin-editor-page and c-admin-table-page to work correctly.
import '@/viewmodels.g';

import CoalesceVuetify from 'coalesce-vue-vuetify';
Vue.use(CoalesceVuetify, { metadata: $metadata, });
```

Also ensure that you have setup [Vuetify](https://vuetifyjs.com/) correctly in your application as [described in Vuetify's documentation](https://vuetifyjs.com/en/getting-started/quick-start/).




## Display Components

<table>
<thead><tr><th width="170px">Component</th><th>Description</th></tr></thead>
<tr><td>

[c-display](./components/c-display.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-display.md) 
</td></tr>
<tr><td>

[c-loader-status](./components/c-loader-status.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-loader-status.md) 
</td></tr>
<tr><td>

[c-list-range-display](./components/c-list-range-display.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-list-range-display.md) 
</td></tr>
<tr><td>

[c-table](./components/c-table.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-table.md) 
</td></tr>
</table>


## Input Components

<table>
<thead><tr><th width="170px">Component</th><th>Description</th></tr></thead>
<tr><td>

[c-input](./components/c-input.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-input.md) 
</td></tr>
<tr><td>

[c-select](./components/c-select.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-select.md) 
</td></tr>
<tr><td>

[c-datetime-picker](./components/c-datetime-picker.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-datetime-picker.md) 
</td></tr>
<tr><td>

[c-select-many-to-many](./components/c-select-many-to-many.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-select-many-to-many.md) 
</td></tr>
<tr><td>

[c-select-string-value](./components/c-select-string-value.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-select-string-value.md) 
</td></tr>
<tr><td>

[c-select-values](./components/c-select-values.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-select-values.md) 
</td></tr>
<tr><td>

[c-list-filters](./components/c-list-filters.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-list-filters.md) 
</td></tr>
<tr><td>

[c-list-pagination](./components/c-list-pagination.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-list-pagination.md) 
</td></tr>
<tr><td>

[c-list-page-size](./components/c-list-page-size.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-list-page-size.md) 
</td></tr>
<tr><td>

[c-list-page](./components/c-list-page.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-list-page.md) 
</td></tr>

</table>


## Admin Components

<table>
<thead><tr><th width="170px">Component</th><th>Description</th></tr></thead>
<tr><td>

[c-admin-method](./components/c-admin-method.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-method.md) 
</td></tr>
<tr><td>

[c-admin-methods](./components/c-admin-methods.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-methods.md) 
</td></tr>
<tr><td>

[c-admin-display](./components/c-admin-display.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-display.md) 
</td></tr>
<tr><td>

[c-admin-editor](./components/c-admin-editor.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-editor.md) 
</td></tr>
<tr><td>

[c-admin-editor-page](./components/c-admin-editor-page.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-editor-page.md) 
</td></tr>
<tr><td>

[c-admin-table](./components/c-admin-table.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-table.md) 
</td></tr>
<tr><td>

[c-admin-table-toolbar](./components/c-admin-table-toolbar.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-table-toolbar.md) 
</td></tr>
<tr><td>

[c-admin-table-page](./components/c-admin-table-page.md)
</td>
<td> 

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./components/c-admin-table-page.md) 
</td></tr>
</table>