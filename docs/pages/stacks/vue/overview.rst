
.. _VueOverview:

Vue Overview
=================

The Vue stack for Coalesce has been designed from the ground up to be used to build modern web applications using current technologies like Webpack, Vue CLI, ES Modules, and more. It enables you to use all of the features of Vue.js available, including building a SPA, and the ability to use modern component frameworks like Vuetify.

Layers
------

The Vue stack for Coalesce is based on a four-layer hierarchy of generated code. From the bottom, these layers are:

:ref:`Metadata <VueMetadata>`
    The metadata layer, generated as `metadata.g.ts`, contains a minimal set of data to represent your data model on the front-end. Because Vue applications are typically statically compiled, it is necessary for the frontend code to have a representation of your data model as an analog to the :csharp:`ReflectionRepository` available at runtime to Knockout apps that utilize `.cshtml` files.

:ref:`Models <VueModels>`
    The model layer, generated as `models.g.ts`, contains a set of TypeScript interfaces that represent each client-exposed type in your data model. Each interface contains all the :ref:`ModelProperties` of that type, as well as a :ts:`$metadata` property that references the metadata object for that type. Enums and :ref:`DataSources` are also represented in the model layer.

    The model layer also includes a TypeScript class for each type that can be used to easily instantiate a valid implementation of its corresponding interface. However, it is not necessary for the classes to be used, and all parts of Coalesce that interact with the model layer don't perform any `instanceof` checks against models - the :ts:`$metadata` property is used for this instead.

:ref:`API Clients <VueApiClients>`
    The API client layer, generated as `api-clients.g.ts`, exports a class for each API controller that was generated for your data model. These classes are stateless, and provide one method for each API endpoint. This includes both the standard set of endpoints created for :ref:`EntityModels` and :ref:`CustomDTOs`, as well as any custom :ref:`ModelMethods`.

:ref:`ViewModels <VueViewModels>`
    The ViewModel layer, generated as `viewmodels.g.ts`, exports a ViewModel class for each API-backed type in your data model (:ref:`EntityModels`, :ref:`CustomDTOs`, and :ref:`Services`). It also exports an additional ListViewModel type for :ref:`EntityModels`, :ref:`CustomDTOs`.

    These ViewModels contain the majority of functionality that you will use on a day-to-day basis as you build applications with the Coalesce Vue stack. They are all valid implementations of their corresponding model interface, and as such can be used in any place where a model could be used.


Libraries
---------

coalesce-vue
............

.. image:: https://img.shields.io/npm/v/coalesce-vue/dev   :alt: npm (tag)

The generated code is all based on an NPM package, `coalesce-vue`. This package contains most of the core functionality of the Vue stack.  Its version should generally be kept in sync with the IntelliTect.Coalesce Nuget packages in your project.

Mirroring the generated code, coalesce-vue is also split into the same four layers - Metadata, Models, API Clients, and ViewModels. Each layer contains the classes and functions upon which each generated portion of the layer builds.


coalesce-vue-vuetify
....................

.. image:: https://img.shields.io/npm/v/coalesce-vue-vuetify/dev   :alt: npm (tag)

Neither the generated code nor `coalesce-vue` actually provide any pre-made UI components. This is where `coalesce-vue-vuetify` comes in. It provides a set of components based on `Vuetify.js <https://vuetifyjs.com/>`_ that include both low level input and display components (e.g. `c-input` and `c-display`), as well as high level components that provide functionality of whole pages (e.g. `c-admin-table-page` and `c-admin-editor-page`).