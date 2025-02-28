# CRUD Models

The primary function of Coalesce, above all else, is to provide Create, Read, Update, and Delete (CRUD) API endpoints around your application's data model that you can easily call from your front-end code without having to wade through the tedium of building these API endpoints by hand.

To this end, there are three different methods of defining models that support CRUD operations:

- [EF Entity Models](#ef-entity-models)
- [Standalone Entities](#standalone-entities)
- [Custom DTOs](#custom-dtos)

 All of these support the following:

- Generated API Controllers with `/get`, `/list`, `/count`, `/save` endpoints (`/bulkSave` unavailable for Standalone Entities).
- [Custom Methods](/modeling/model-components/methods.md)
- [TypeScript ViewModels](/stacks/vue/layers/viewmodels.md#viewmodels) and [TypeScript ListViewModels](/stacks/vue/layers/viewmodels.md#listviewmodels)
- [Data Sources](/modeling/model-components/data-sources.md) and [Behaviors](/modeling/model-components/behaviors.md)
- Admin pages


## EF Entity Models

In Coalesce applications, most, if not all, of your data models will be [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) entity models. These are built with standard EF conventions, but their interactions with Coalesce can be greatly customized.

Read more about [EF Entity Models](/modeling/model-types/entities.md).


## Standalone Entities

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./standalone-entities.md)  

Read more about [Standalone Entities](/modeling/model-types/standalone-entities.md).


## Custom DTOs

@[import-md "after":"MARKER:summary", "before":"MARKER:summary-end"](./dtos.md)

Read more about [Custom DTOs](/modeling/model-types/dtos.md).