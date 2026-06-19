# Includes String
Coalesce provides extension points for loading and serialization that use a concept called an "includes string" (also referred to as "include string" or just "includes").

The includes string is simply an arbitrary string that is passed from the client to the server to customize data loading and serialization. It can be set on both the TypeScript ViewModels and the ListViewModels.

It is available for use in your [custom data sources](/modeling/model-components/data-sources.md) via [IDataSourceParameters](/modeling/model-components/data-sources.md#standard-parameters) and its derived types. It can also be used in [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md) attributes.

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

## SearchAttribute Includes/Excludes

`includes` is also used by [`[Search]`](/modeling/model-components/attributes/search.md) when you set `SearchAttribute.Includes` or `SearchAttribute.Excludes`.

- `Search(Includes = "...")`: property is searchable only when the request `includes` string matches.
- `Search(Excludes = "...")`: property is not searchable when the request `includes` string matches.

Example:

```cs
[Search(Includes = "details")]
public string Biography { get; set; }
```

With the above, `Biography` participates in list searching only when the request has `?includes=details`.

### Special Values

There are a few values of `includes` that are either set by default in the auto-generated views, or otherwise have special meaning:

| Value | Description |
|------|---|
| `'none'` | Setting `includes` to ``none`` suppresses the [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior) provided by the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source) - The resulting data will be the requested object (or list of objects) and nothing more. |
| `'admin-list'` | Used when loading a list of objects in the [Vue admin list page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md). |
| `'admin-editor'` | Used when loading an object in the [Vue admin editor component](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md). |


## DtoIncludes & DtoExcludes

Main document: [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md).

There are two C# attributes, `DtoIncludes` and `DtoExcludes`, that can be used to annotate your data model to customize what data gets put into the DTOs and ultimately serialized to JSON and sent to the client.

@[import-md "after":"see [Includes String]"](../modeling/model-components/attributes/dto-includes-excludes.md)