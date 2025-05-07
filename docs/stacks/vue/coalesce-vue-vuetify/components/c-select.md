# c-select

<!-- MARKER:summary -->
    
A dropdown component that allows for selecting values fetched from the generated ``/list`` API endpoints. 

Used for selecting values for foreign key and navigation properties, or for selecting arbitrary objects or primary keys without a parent or owning object.

<!-- MARKER:summary-end -->

## Examples

Binding to a reference navigation property or foreign key of a model:

``` vue-html
  <c-select :model="person" for="company" />
  <!-- OR: -->
  <c-select :model="person" for="companyId" />
```

Binding an arbitrary primary key value or an arbitrary object:

``` vue-html
  <!-- Binding a key: -->
  <c-select for="Person" v-model:key-value="selectedPersonId" />

  <!-- Binding an object: -->
  <c-select for="Person" v-model:object-value="selectedPerson" />
  <c-select for="Person" v-model="selectedPerson" />
```

Multi-select:

``` vue-html
  <!-- Binding selected primary keys: -->
  <c-select for="Person" multiple v-model:key-value="selectedPeopleIds" />

  <!-- Binding selected objects: -->
  <c-select for="Person" multiple v-model="selectedPeople" />
  <c-select for="Person" multiple v-model:object-value="selectedPeople" />
```

Binding to a collection navigation property of a model:

``` vue-html
  <c-select :model="person" for="casesAssigned" />
```

This will assign `person` and its PK to the inverse navigation property of the relationship (`Case.AssignedTo`) when items are selected, and will null those properties when items are deselected. Note that this scenario is not delegated to by `c-input` automatically and requires direct usage of `c-select` since it is a fairly unusual scenario and usually requires additional customization (e.g. the `params` and `create` props) to make it function well.

----

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

Note: In addition to the below props, `c-select` also supports most props that are supported by Vuetify's [v-text-field](https://vuetifyjs.com/en/components/text-fields/).

<Prop def="for: string | Value | Property | ModelType" lang="ts" />

A metadata specifier for the value being bound. One of:

- The name of a model type.
- The name of a foreign key or navigation property belonging to `model`. 
- A direct reference to a metadata object.

::: tip
When the binding can only locate a PK value and the corresponding object cannot be found (e.g. there is no navigation property, or the navigation property is null), c-select will automatically attempt to load the object from the server so it can be displayed in the UI.
:::

<Prop def="model?: Model" lang="ts" />

An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

If `for` specifies a foreign key or reference navigation property, both the foreign key and the navigation property of the `model` will be updated when the selected value is changed.

<Prop def="modelValue?: any" lang="ts" />

When binding the component with ``v-model``, accepts the ``modelValue`` part of ``v-model``. If `for` was specified as a foreign key, this will expect a key; likewise, if `for` was specified as a type or as a navigation property, this will expect an object.

<Prop def="multiple?: boolean" lang="ts" />

Enables multi-select functionality. Bindings for `modelValue`, `keyValue`, and `objectValue` will accept and emit arrays instead of single values.

<Prop def="keyValue?: TKey
'onUpdate:keyValue': (value: TKey) => void" lang="ts" />

When bound with `v-model:key-value="keyValue"`, allows binding the primary key of the selected object explicitly. Binds an array when in multi-select mode.

<Prop def="objectValue?: TModel
'onUpdate:objectValue': (value: TModel) => void" lang="ts" />

When bound with `v-model:object-value="objectValue"`, allows binding the selected object explicitly. Binds an array when in multi-select mode.

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

If provided and non-false, enables [response caching](/stacks/vue/layers/api-clients.md#response-caching) on the component's internal API callers.

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

`#item="{ item: TModel, search: string }"` - Slot used to customize the text of both items inside the list, as well as the text of selected items. By default, items are rendered with [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md). Slot is passed a parameter `item` containing a [model instance](/stacks/vue/layers/models.md), and `search` containing the current search query.

`#list-item="{ item: TModel, search: string, selected: boolean }"` - Slot used to customize the text of items inside the list. If not provided, falls back to the `item` slot. Contents are wrapped in a `v-list-item-title`.

`#selected-item="{ item: TModel, search: string, index:  number, remove: () => void }"` - Slot used to customize the text of selected items. If not provided, falls back to the `item` slot. The `remove` function will deselect the item and is only valid when using multi-select.