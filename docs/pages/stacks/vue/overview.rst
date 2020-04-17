
.. _VueOverview:

Vue Overview
============

The `Vue`_ stack for Coalesce has been designed from the ground up to be used to build modern web applications using current technologies like Webpack, Vue CLI, ES Modules, and more. It enables you to use all of the features of Vue.js, including building a SPA, and the ability to use modern component frameworks like Vuetify_. 

.. contents:: Contents
    :local:

Getting Started
---------------

Check out :ref:`VueGettingStarted` if you haven't already to learn how to get a new Coalesce Vue project up and running.

.. _VueLayers:

TypeScript Layers
-----------------

.. image:: https://img.shields.io/npm/v/coalesce-vue/dev?color=42b883&label=coalesce-vue@dev
   :target: https://www.npmjs.com/package/coalesce-vue


The generated code for the Vue stack all builds on the coalesce-vue_ NPM package which contains most of the core functionality of the Vue stack.  Its version should generally be kept in sync with the `IntelliTect.Coalesce NuGet packages <https://www.nuget.org/packages/IntelliTect.Coalesce/>`_ in your project.

Both the generated code and coalesce-vue_ are split into four layers, with each layer building on the layers underneath. From the bottom, these layers are:



:ref:`VueMetadata`
.............................

The metadata layer, generated as `metadata.g.ts`, contains a minimal set of metadata to represent your data model on the front-end. Because Vue applications are typically compiled into a set of static assets, it is necessary for the frontend code to have a representation of your data model as an analog to the :csharp:`ReflectionRepository` available at runtime to Knockout apps that utilize `.cshtml` files.

:ref:`Read more about the Metadata layer <VueMetadata>`




:ref:`VueModels`
.........................

The model layer, generated as `models.g.ts`, contains a set of TypeScript interfaces that represent each client-exposed type in your data model. Each interface declares all the :ref:`ModelProperties` of that type, as well as a :ts:`$metadata` property that references the metadata object for that type. Enums and :ref:`DataSources` are also represented in the model layer.

:ref:`Read more about the Model layer <VueModels>`



:ref:`VueApiClients`
..................................

The API client layer, generated as `api-clients.g.ts`, exports a class for each API controller that was generated for your data model. These classes are stateless and provide one method for each API endpoint. This includes both the standard set of endpoints created for :ref:`EntityModels` and :ref:`CustomDTOs`, as well as any custom :ref:`ModelMethods`.

:ref:`Read more about the API Client layer <VueApiClients>`



:ref:`VueViewModels`
.................................

The ViewModel layer, generated as `viewmodels.g.ts`, exports a ViewModel class for each API-backed type in your data model (:ref:`EntityModels`, :ref:`CustomDTOs`, and :ref:`Services`). It also exports an additional ListViewModel type for :ref:`EntityModels`, :ref:`CustomDTOs`.

These ViewModels contain the majority of functionality that you will use on a day-to-day basis as you build applications with the Coalesce Vue stack. They are all valid implementations of their corresponding model interface, and as such can be used in any place where a model could be used.

:ref:`Read more about the ViewModel layer <VueViewModels>`


Vue Components
--------------

.. include:: ./coalesce-vue-vuetify/overview.rst
   :start-after: MARKER:summary
   :end-before: MARKER:summary-end

:ref:`Read more about the Vuetify Components here <VuetifyOverview>`.

Admin Views
-----------

The Vue_ stack for Coalesce does not generate any admin views for you like the :ref:`Knockout stack <KoOverview>` does. Instead, it provides some high level components that provide functionality of whole pages like :ref:`c-admin-table-page` and :ref:`c-admin-editor-page` - these are the analogues of the generated admin Table and CreateEdit views in the :ref:`Knockout stack <KoOverview>`. 

These components are driven off of the generated layers described above rather than being statically generated like the Knockout_ admin pages - this allows us to keep bundle size to a minimum.

The template described in :ref:`VueGettingStarted` comes with routes already in place for these page-level components. For example, ``/admin/Person`` for a table, ``/admin/Person/edit`` to create a new ``Person``, and ``/admin/Person/edit/:id`` to edit a ``Person``.

:ref:`Read more about the Vuetify Components here <VuetifyOverview>`.
