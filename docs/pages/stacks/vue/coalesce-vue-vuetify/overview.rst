.. _VuetifyOverview:

Vuetify Components
==================

.. MARKER:summary

.. image:: https://img.shields.io/npm/v/coalesce-vue-vuetify/dev?color=42b883&label=coalesce-vue-vuetify@dev
   :target: https://www.npmjs.com/package/coalesce-vue-vuetify

The Vue_ stack for Coalesce provides :ref:`a set of components <VuetifyOverview>` based on Vuetify_, packaged up in an NPM package coalesce-vue-vuetify_. These components are driven primarily by the :ref:`VueMetadata`, and include both low level input and display components like :ref:`c-input` and :ref:`c-display` that are highly reusable in the custom pages you'll build in your application, as well as high-level components like :ref:`c-admin-table-page` and :ref:`c-admin-editor-page` that constitute entire pages. 

.. MARKER:summary-end

.. contents:: Contents
    :local:

.. toctree::
    :hidden:
    :maxdepth: 1
    :titlesonly:
    :glob:

    ./components/*

Setup
-----

.. tip::
    The template described in :ref:`VueGettingStarted` already includes all the necessary setup. You can skip this section if you started from the template.

First, ensure that NPM package coalesce-vue-vuetify_ is installed in your project.

Then, in your Vue_ application's ``main.ts`` file, you need to add the ``coalesce-vue-vuetify`` `plugin <https://vuejs.org/v2/guide/plugins.html>`_ to your application, like so:

.. code-block:: vue

    import $metadata from '@/metadata.g';
    // viewmodels.g has side-effects - it populates the global lookup on ViewModel and ListViewModel. 
    // It must be imported for c-admin-editor-page and c-admin-table-page to work correctly.
    import '@/viewmodels.g';

    import CoalesceVuetify from 'coalesce-vue-vuetify';
    Vue.use(CoalesceVuetify, { metadata: $metadata, });

Also ensure that you have setup Vuetify_ correctly in your application as `described in Vuetify's documentation <https://vuetifyjs.com/en/getting-started/quick-start/>`_.


.. note::
    An important note if you're using Vuetify_'s `A-la-carte builds <https://vuetifyjs.com/en/customization/a-la-carte/>`_:
 
    Similar to importing Vuetify from :ts:`'vuetify/lib'` rather than :ts:`'vuetify'`, you should import :ts:`'coalesce-vue-vuetify'` from :ts:`'coalesce-vue-vuetify/lib'`. This is an alternate build that itself imports Vuetify components from :ts:`'vuetify/lib'`, therefore preventing duplication of components. Similar to Vuetify, this build also does not register the Coalesce components globally, allowing them to also be treeshaken.

    For Vue CLI-based projects:

    Install ``unplugin-vue-components``, and add the following configuration to ``vue.config.js``:

    .. code-block:: ts
        
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

    For Vite projects:

    Install ``unplugin-vue-components``, and add it to your ``vite.config.ts``:

    .. code-block:: ts

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


You're now ready to start using the components that coalesce-vue-vuetify_ provides! See the list below for a summary of each component and links to dive deeper into each component.

Display Components
------------------

:ref:`c-display`
................
    .. include:: ./components/c-display.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-display`.

:ref:`c-loader-status`
......................
    .. include:: ./components/c-loader-status.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-loader-status`.


:ref:`c-list-range-display`
...........................
    .. include:: ./components/c-list-range-display.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-list-range-display`.

:ref:`c-table`
..............
    .. include:: ./components/c-table.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-table`. 


Input Components
----------------


:ref:`c-input`
..............
    .. include:: ./components/c-input.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-input`.

:ref:`c-select`
...............
    .. include:: ./components/c-select.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-select`.

:ref:`c-datetime-picker`
........................
    .. include:: ./components/c-datetime-picker.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-datetime-picker`.

:ref:`c-select-many-to-many`
............................
    .. include:: ./components/c-select-many-to-many.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-select-many-to-many`.

:ref:`c-select-string-value`
............................
    .. include:: ./components/c-select-string-value.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-select-string-value`.

:ref:`c-select-values`
......................
    .. include:: ./components/c-select-values.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-select-values`.

:ref:`c-list-filters`
.....................
    .. include:: ./components/c-list-filters.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-list-filters`.

:ref:`c-list-pagination`
........................
    .. include:: ./components/c-list-pagination.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-list-pagination`.

:ref:`c-list-page-size`
.......................
    .. include:: ./components/c-list-page-size.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-list-page-size`.

:ref:`c-list-page`
..................
    .. include:: ./components/c-list-page.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-list-page`.

Admin Components
----------------


:ref:`c-admin-method`
.....................
    .. include:: ./components/c-admin-method.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-method`.

:ref:`c-admin-methods`
......................
    .. include:: ./components/c-admin-methods.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-methods`.

:ref:`c-admin-display`
......................
    .. include:: ./components/c-admin-display.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-display`.

:ref:`c-admin-editor`
.....................
    .. include:: ./components/c-admin-editor.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-editor`.

:ref:`c-admin-editor-page`
..........................
    .. include:: ./components/c-admin-editor-page.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-editor-page`.

:ref:`c-admin-table`
....................
    .. include:: ./components/c-admin-table.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-table`.

:ref:`c-admin-table-toolbar`
............................
    .. include:: ./components/c-admin-table-toolbar.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-table-toolbar`.

:ref:`c-admin-table-page`
.........................
    .. include:: ./components/c-admin-table-page.rst
        :start-after: MARKER:summary
        :end-before: MARKER:summary-end
    Full Documentation: :ref:`c-admin-table-page`.
