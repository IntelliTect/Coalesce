.. _VuetifyOverview:

Vuetify Components
==================

.. MARKER:summary

.. image:: https://img.shields.io/npm/v/coalesce-vue-vuetify/dev?color=42b883&label=coalesce-vue-vuetify@dev
   :target: https://www.npmjs.com/package/coalesce-vue-vuetify

The [Vue](https://vuejs.org/) stack for Coalesce provides [a set of components](/stacks/vue/coalesce-vue-vuetify/overview.md) based on [Vuetify](https://vuetifyjs.com/), packaged up in an NPM package [coalesce-vue-vuetify](https://www.npmjs.com/package/coalesce-vue-vuetify). These components are driven primarily by the [Metadata Layer](/stacks/vue/layers/metadata.md), and include both low level input and display components like [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) and [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) that are highly reusable in the custom pages you'll build in your application, as well as high-level components like [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md) and [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md) that constitute entire pages. 

.. MARKER:summary-end

[[toc]]

.. toctree::
    :hidden:
    :maxdepth: 1
    :titlesonly:
    :glob:

    ./components/*

Setup
-----

.. tip::
    The template described in [Getting Started with Vue](/stacks/vue/getting-started.md) already includes all the necessary setup. You can skip this section if you started from the template.

First, ensure that NPM package [coalesce-vue-vuetify](https://www.npmjs.com/package/coalesce-vue-vuetify) is installed in your project.

Then, in your [Vue](https://vuejs.org/) application's ``main.ts`` file, you need to add the ``coalesce-vue-vuetify`` [plugin](https://vuejs.org/v2/guide/plugins.html) to your application, like so:

``` ts

    import $metadata from '@/metadata.g';
    // viewmodels.g has side-effects - it populates the global lookup on ViewModel and ListViewModel. 
    // It must be imported for c-admin-editor-page and c-admin-table-page to work correctly.
    import '@/viewmodels.g';

    import CoalesceVuetify from 'coalesce-vue-vuetify';
    Vue.use(CoalesceVuetify, { metadata: $metadata, });


```

Also ensure that you have setup [Vuetify](https://vuetifyjs.com/) correctly in your application as [described in Vuetify's documentation](https://vuetifyjs.com/en/getting-started/quick-start/).


.. note::
    An important note if you're using [Vuetify](https://vuetifyjs.com/)'s [A-la-carte builds](https://vuetifyjs.com/en/customization/a-la-carte/):
 
    Similar to importing Vuetify from `'vuetify/lib'` rather than `'vuetify'`, you should import `'coalesce-vue-vuetify'` from `'coalesce-vue-vuetify/lib'`. This is an alternate build that itself imports Vuetify components from `'vuetify/lib'`, therefore preventing duplication of components. Similar to Vuetify, this build also does not register the Coalesce components globally, allowing them to also be treeshaken.

    For Vue CLI-based projects:

    Install ``unplugin-vue-components``, and add the following configuration to ``vue.config.js``:

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

    For Vite projects:

    Install ``unplugin-vue-components``, and add it to your ``vite.config.ts``:

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

You're now ready to start using the components that [coalesce-vue-vuetify](https://www.npmjs.com/package/coalesce-vue-vuetify) provides! See the list below for a summary of each component and links to dive deeper into each component.

Display Components
------------------

[c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md)
................
    .. include:: ./components/c-display.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md).

[c-loader-status](/stacks/vue/coalesce-vue-vuetify/components/c-loader-status.md)
......................
    .. include:: ./components/c-loader-status.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-loader-status](/stacks/vue/coalesce-vue-vuetify/components/c-loader-status.md).


[c-list-range-display](/stacks/vue/coalesce-vue-vuetify/components/c-list-range-display.md)
...........................
    .. include:: ./components/c-list-range-display.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-list-range-display](/stacks/vue/coalesce-vue-vuetify/components/c-list-range-display.md).

[c-table](/stacks/vue/coalesce-vue-vuetify/components/c-table.md)
..............
    .. include:: ./components/c-table.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-table](/stacks/vue/coalesce-vue-vuetify/components/c-table.md). 


Input Components
----------------


[c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md)
..............
    .. include:: ./components/c-input.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md).

[c-select](/stacks/vue/coalesce-vue-vuetify/components/c-select.md)
...............
    .. include:: ./components/c-select.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-select](/stacks/vue/coalesce-vue-vuetify/components/c-select.md).

[c-datetime-picker](/stacks/vue/coalesce-vue-vuetify/components/c-datetime-picker.md)
........................
    .. include:: ./components/c-datetime-picker.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-datetime-picker](/stacks/vue/coalesce-vue-vuetify/components/c-datetime-picker.md).

[c-select-many-to-many](/stacks/vue/coalesce-vue-vuetify/components/c-select-many-to-many.md)
............................
    .. include:: ./components/c-select-many-to-many.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-select-many-to-many](/stacks/vue/coalesce-vue-vuetify/components/c-select-many-to-many.md).

[c-select-string-value](/stacks/vue/coalesce-vue-vuetify/components/c-select-string-value.md)
............................
    .. include:: ./components/c-select-string-value.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-select-string-value](/stacks/vue/coalesce-vue-vuetify/components/c-select-string-value.md).

[c-select-values](/stacks/vue/coalesce-vue-vuetify/components/c-select-values.md)
......................
    .. include:: ./components/c-select-values.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-select-values](/stacks/vue/coalesce-vue-vuetify/components/c-select-values.md).

[c-list-filters](/stacks/vue/coalesce-vue-vuetify/components/c-list-filters.md)
.....................
    .. include:: ./components/c-list-filters.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-list-filters](/stacks/vue/coalesce-vue-vuetify/components/c-list-filters.md).

[c-list-pagination](/stacks/vue/coalesce-vue-vuetify/components/c-list-pagination.md)
........................
    .. include:: ./components/c-list-pagination.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-list-pagination](/stacks/vue/coalesce-vue-vuetify/components/c-list-pagination.md).

[c-list-page-size](/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.md)
.......................
    .. include:: ./components/c-list-page-size.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-list-page-size](/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.md).

[c-list-page](/stacks/vue/coalesce-vue-vuetify/components/c-list-page.md)
..................
    .. include:: ./components/c-list-page.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-list-page](/stacks/vue/coalesce-vue-vuetify/components/c-list-page.md).

Admin Components
----------------


[c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md)
.....................
    .. include:: ./components/c-admin-method.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md).

[c-admin-methods](/stacks/vue/coalesce-vue-vuetify/components/c-admin-methods.md)
......................
    .. include:: ./components/c-admin-methods.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-methods](/stacks/vue/coalesce-vue-vuetify/components/c-admin-methods.md).

[c-admin-display](/stacks/vue/coalesce-vue-vuetify/components/c-admin-display.md)
......................
    .. include:: ./components/c-admin-display.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-display](/stacks/vue/coalesce-vue-vuetify/components/c-admin-display.md).

[c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md)
.....................
    .. include:: ./components/c-admin-editor.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md).

[c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md)
..........................
    .. include:: ./components/c-admin-editor-page.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md).

[c-admin-table](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table.md)
....................
    .. include:: ./components/c-admin-table.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-table](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table.md).

[c-admin-table-toolbar](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-toolbar.md)
............................
    .. include:: ./components/c-admin-table-toolbar.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-table-toolbar](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-toolbar.md).

[c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md)
.........................
    .. include:: ./components/c-admin-table-page.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md).
