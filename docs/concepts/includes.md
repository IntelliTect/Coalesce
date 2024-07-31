# Includes String
Coalesce provides a number of extension points for loading & serialization which make use of a concept called an "includes string" (also referred to as "include string" or just "includes").

    
## Includes String
The includes string is simply a string which can be set to any arbitrary value. It is passed from the client to the server in order to customize data loading and serialization. It can be set on both the TypeScript ViewModels and the ListViewModels.

<CodeTabs>
<template #vue>

``` ts
import { PersonViewModel, PersonListViewModel } from '@/viewmodels.g'

var person = new PersonViewModel();
person.$includes = "details";

var personList = new PersonListViewModel();
personList.$includes = "details";
```

</template>
</CodeTabs>

The default value (i.e. no action) is the empty string.

### Special Values

There are a few values of `includes` that are either set by default in the auto-generated views, or otherwise have special meaning:

| Value | Description |
|------|---|
| `'none'` | Setting `includes` to ``none`` suppresses the [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior) provided by the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source) - The resulting data will be the requested object (or list of objects) and nothing more. |
| `'admin-list'` | Used when loading a list of objects in the [Vue admin list page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md). |
| `'admin-editor'` | Used when loading an object in the [Vue admin editor component](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md). |
| `'Editor'` | Legacy. Used when loading an object in the generated Knockout CreateEdit views.  |
| `'<ModelName>ListGen'` | Legacy. Used when loading a list of objects in the generated Knockout Table and Cards views. For example, `PersonListGen` |


## DtoIncludes & DtoExcludes

Main document: [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md).

There are two C# attributes, `DtoIncludes` and `DtoExcludes`, that can be used to annotate your data model in order to customize what data gets put into the DTOs and ultimately serialized to JSON and sent out to the client.

@[import-md "after":"see [Includes String]"](../modeling/model-components/attributes/dto-includes-excludes.md)