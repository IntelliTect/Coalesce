# c-select

<!-- MARKER:summary -->
    
A dropdown component that allows for selecting values fetched from the generated ``/list`` API endpoints. 

Used both for selecting values for foreign key and navigation properties, and for selecting arbitrary objects or primary keys independent of a parent or owning object.

<!-- MARKER:summary-end -->

[[toc]]

## Examples

Binding to a navigation property or foreign key of a model:

``` vue-html
  <c-select :model="person" for="company" />
  <!-- OR: -->
  <c-select :model="person" for="companyId" />
```

Binding an arbitrary primary key value or an arbitrary object:

``` vue-html
  <!-- Binding a key: -->
  <c-select for="Person" :key-value.sync="selectedPersonId" />

  <!-- Binding an object: -->
  <c-select for="Person" :object-value.sync="selectedPerson" />
  <c-select for="Person" v-model="selectedPerson" />
```

Examples of other props:

``` vue-html
<c-select 
  for="Person" 
  v-model="selectedPerson"
  :clearable="false"
  preselect-first
  :params="{ pageSize: 42, filter: { isActive: true } }"
  :create="createMethods"
  dense
  outlined
  color="pink"
/>
<!-- `createMethods` is defined in the docs of `create` below -->
```

## Props

<Prop def="for: string | ForeignKeyProperty | ModelReferenceNavigationProperty | ModelType" lang="ts" />

A metadata specifier for the value being bound. One of:

- The name of a foreign key or reference navigation property belonging to `model`. 
- The name of a model type.
- A direct reference to a metadata object.
- A string in dot-notation that starts with a type name that resolves to a foreign key or reference navigation property.

::: tip
When binding by a key value, if the corresponding object cannot be found (e.g. there is no navigation property, or the navigation property is null), c-select will automatically attempt to load the object from the server so it can be displayed in the UI.
:::

<Prop def="model?: Model" lang="ts" />

An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

If `for` specifies a foreign key or reference navigation property, both the foreign key and the navigation property of the `model` will be updated when the selected value is changed.

<Prop def="value?: any // Vue 2
modelValue?: any // Vue 3" lang="ts" />

When binding the component with ``v-model``, accepts the ``value`` part of ``v-model``. If `for` was specified as a foreign key, this will expect a key; likewise, if `for` was specified as a type or as a navigation property, this will expect an object.

<Prop def="keyValue?: any" lang="ts" />

When bound with `:key-value.sync="keyValue"`, allows binding the primary key of the selected object explicitly.

<Prop def="objectValue?: any" lang="ts" />

When bound with `:object-value.sync="objectValue"`, allows binding the selected object explicitly.

<Prop def="clearable?: boolean" lang="ts" />

Whether the selection can be cleared or not, emitting `null` as the input value.

If not specified and the component is bound to a foreign key or reference navigation property, defaults to whether or not the foreign key has a ``required`` validation rule defined in its [Metadata](/stacks/vue/layers/metadata.md).

<Prop def="preselectFirst?: boolean = false" lang="ts" />

If true, then when the first list results for the component are received by the client just after the component is created, c-select will emit the first item in the list as the selected value.

<Prop def="preselectSingle?: boolean = false" lang="ts" />

If true, then when the first list results for the component are received by the client just after the component is created, if the results contained exactly one item, c-select will emit that only item as the selected value.

<Prop def="reloadOnOpen?: boolean = false" lang="ts" />

If true, the list results will be reloaded when the dropdown menu is opened. By default, list results are loaded when the component is mounted and also when any of its parameters change (either search input or the `params` prop).

<Prop def="params?: ListParameters" lang="ts" />

An optional set of [Data Source Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters) to pass to API calls made to the server.

<Prop def="cache?: ResponseCachingConfiguration | boolean" lang="ts" />

If provided and non-false, enables [response caching](/stacks/vue/layers/api-clients.html#response-caching) on the component's internal API callers.

<Prop def="create?: {
  getLabel: (search: string, items: TModel[]) => string | false,
  getItem: (search: string, label: string) => Promise<TModel>
}" lang="ts" />

A object containing a pair of methods that allowing users to create new items from directly within the c-select if a matching object is not found. 

The object must contain the following two methods. You should define these in your component's ``script`` section - don't try to define them inline in your component.

<div style="margin-left: 20px">

<Prop def="create.getLabel: (search: string, items: TModel[]) => string | false" lang="ts" id="member-create-getLabel" />

A function that will be called with the user's current search term, as well as the collection of currently loaded items being presented to the user as valid selection options.

It should return either a `string` that will be presented to the user as an option in the dropdown that can be clicked to invoke the `getItem` function below, or it should return `false` to prevent such an option from being shown to the user.

<Prop def="create.getItem: (search: string, label: string) => Promise<TModel>" lang="ts" id="member-create-getItem"  />

A function that will be invoked when the user clicks the option in the dropdown list described by `getLabel`. It will be given the user's current search term as well as the value of the label returned from `getLabel` as parameters. It must perform the necessary operations to create the new object on the server and then return a reference to that object.

</div>

For example:

```ts
createMethods = {
  getLabel(search: string, items: Person[]) {
    const searchLower = search.toLowerCase();
    if (items.some(a => a.name?.toLowerCase().indexOf(searchLower) == 0)) {
      return false;
    }
    return search;
  },
  async getItem(search: string, label: string) {
    const client = new PersonApiClient();
    return (await client.addPersonByName(label)).data.object!;
  }
}
```

## Slots

`#item="{ item, search }"` - Slot used to customize the text of both items inside the list, as well as the text of selected items. By default, items are rendered with [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md). Slot is passed a parameter `item` containing a [model instance](/stacks/vue/layers/models.md), and `search` containing the current search query..

`#list-item="{ item, search }"` - Slot used to customize the text of items inside the list. If not provided, falls back to the `item` slot.

`#selected-item="{ item, search }"` - Slot used to customize the text of selected items. If not provided, falls back to the `item` slot.