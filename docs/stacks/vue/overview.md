
# Vue Overview

The `Vue` stack for Coalesce has been designed from the ground up to be used to build modern web applications using current technologies like Vite or Webpack + Vue CLI, ES Modules, and more. It enables you to use all of the features of Vue.js, including building a SPA, and the ability to use modern component frameworks like [Vuetify](https://vuetifyjs.com/). 

[[toc]]

## Getting Started

Check out [Getting Started with Vue](/stacks/vue/getting-started.md) if you haven't already to learn how to get a new Coalesce Vue project up and running.

## TypeScript Layers

[![](https://img.shields.io/npm/v/coalesce-vue/dev?color=42b883&label=coalesce-vue%40dev)](https://www.npmjs.com/package/coalesce-vue)


The generated code for the Vue stack all builds on the [coalesce-vue](https://www.npmjs.com/package/coalesce-vue) NPM package which contains most of the core functionality of the Vue stack.  Its version should generally be kept in sync with the [IntelliTect.Coalesce NuGet packages](https://www.nuget.org/packages/IntelliTect.Coalesce/) in your project.

Both the generated code and [coalesce-vue](https://www.npmjs.com/package/coalesce-vue) are split into four layers, with each layer building on the layers underneath. From the bottom, these layers are:



### [Metadata Layer](/stacks/vue/layers/metadata.md)

The metadata layer, generated as `metadata.g.ts`, contains a minimal set of metadata to represent your data model on the front-end. Because Vue applications are typically compiled into a set of static assets, it is necessary for the frontend code to have a representation of your data model as an analog to the `ReflectionRepository` available at runtime to Knockout apps that utilize `.cshtml` files.

[Read more about the Metadata layer](/stacks/vue/layers/metadata.md)




### [Model Layer](/stacks/vue/layers/models.md)

The model layer, generated as `models.g.ts`, contains a set of TypeScript interfaces that represent each client-exposed type in your data model. Each interface declares all the [Properties](/modeling/model-components/properties.md) of that type, as well as a `$metadata` property that references the metadata object for that type. Enums and [Data Sources](/modeling/model-components/data-sources.md) are also represented in the model layer.

[Read more about the Model layer](/stacks/vue/layers/models.md)



### [API Client Layer](/stacks/vue/layers/api-clients.md)

The API client layer, generated as `api-clients.g.ts`, exports a class for each API controller that was generated for your data model. These classes are stateless and provide one method for each API endpoint. This includes both the standard set of endpoints created for [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md), as well as any custom [Methods](/modeling/model-components/methods.md).

[Read more about the API Client layer](/stacks/vue/layers/api-clients.md)



### [ViewModel Layer](/stacks/vue/layers/viewmodels.md)

The ViewModel layer, generated as `viewmodels.g.ts`, exports a ViewModel class for each API-backed type in your data model ([Entity Models](/modeling/model-types/entities.md), [Custom DTOs](/modeling/model-types/dtos.md), and [Services](/modeling/model-types/services.md)). It also exports an additional ListViewModel type for [Entity Models](/modeling/model-types/entities.md), [Custom DTOs](/modeling/model-types/dtos.md).

These ViewModels contain the majority of functionality that you will use on a day-to-day basis as you build applications with the Coalesce Vue stack. They are all valid implementations of their corresponding model interface, and as such can be used in any place where a model could be used.

[Read more about the ViewModel layer](/stacks/vue/layers/viewmodels.md)


## Vue Components

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./coalesce-vue-vuetify/overview.md)

[Read more about the Vuetify Components here](/stacks/vue/coalesce-vue-vuetify/overview.md).

## Admin Views

The [Vue](https://vuejs.org/) stack for Coalesce does not generate any admin views for you like the [Knockout stack](/stacks/ko/overview.md) does. Instead, it provides some high level components that provide functionality of whole pages like [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md) and [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md) - these are the analogues of the generated admin Table and CreateEdit views in the [Knockout stack](/stacks/ko/overview.md). 

These components are driven off of the generated layers described above rather than being statically generated like the [Knockout](http://knockoutjs.com/) admin pages - this allows us to keep bundle size to a minimum.

The template described in [Getting Started with Vue](/stacks/vue/getting-started.md) comes with routes already in place for these page-level components. For example, ``/admin/Person`` for a table, ``/admin/Person/edit`` to create a new ``Person``, and ``/admin/Person/edit/:id`` to edit a ``Person``.

[Read more about the Vuetify Components here](/stacks/vue/coalesce-vue-vuetify/overview.md).
