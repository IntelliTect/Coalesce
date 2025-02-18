# c-input

<!-- MARKER:summary -->
    
A general-purpose input component for [Properties](/modeling/model-components/properties.md), [Method Parameters](/modeling/model-components/methods.md#parameters), and [Data Source Parameters](/modeling/model-components/data-sources.md#custom-parameters). c-input delegates to other components based on the type of value it is bound to. This includes both other [Coalesce Vuetify Components](/stacks/vue/coalesce-vue-vuetify/overview.md) as well as direct usages of some [Vuetify](https://vuetifyjs.com/) components.

<!-- MARKER:summary-end -->

A summary of the components delegated to, by type:


<table> 
<thead>
<tr>
<th>Property/Parameter Type</th>
<th>Target Component</th>
</tr>
</thead>
<tr>
<td>

String

</td>
<td>

- Single: 
  - [v-textarea](https://vuetifyjs.com/en/components/textarea/) if attribute ``textarea`` is provided to ``c-input`` or if `[DataType(DataType.MultilineText)]` is present in C#.
  - [v-text-field](https://vuetifyjs.com/en/components/text-fields/) otherwise. Additionally, `[DataTypeAttribute]` values of `DataType.EmailAddress`, `DataType.PhoneNumber`, `DataType.Password`, or `"Color"` on the field will apply appropriate adjustments to the field.
- Multiple: [c-select-values](/stacks/vue/coalesce-vue-vuetify/components/c-select-values.md)

</td>
</tr>
<tr>
<td>

Number

</td>
<td>

- Single: [v-text-field](https://vuetifyjs.com/en/components/text-fields/)
- Multiple: [c-select-values](/stacks/vue/coalesce-vue-vuetify/components/c-select-values.md)

</td>
</tr>



<tr>
<td>

Boolean

</td>
<td>

[v-switch](https://vuetifyjs.com/en/components/selection-controls/), or [v-checkbox](https://vuetifyjs.com/en/components/selection-controls/) if flag attribute ``checkbox`` is provided to ``c-input``

</td>
</tr>



<tr>
<td>

Enum(s)

</td>
<td>

[v-select](https://vuetifyjs.com/en/components/selects/)

</td>
</tr>



<tr>
<td>

File(s)

</td>
<td>

[v-file-input](https://vuetifyjs.com/en/components/file-inputs/)

</td>
</tr>



<tr>
<td>

Date and/or Time

</td>
<td>

[c-datetime-picker](/stacks/vue/coalesce-vue-vuetify/components/c-datetime-picker.md)

</td>
</tr>



<tr>
<td>

[CRUD Model(s)](/modeling/model-types/crud.md)

</td>
<td>

- Single: [c-select](/stacks/vue/coalesce-vue-vuetify/components/c-select.md)
- Multiple: [c-select](/stacks/vue/coalesce-vue-vuetify/components/c-select.md)
- [[ManyToMany]](/modeling/model-components/attributes/many-to-many.md): [c-select-many-to-many](/stacks/vue/coalesce-vue-vuetify/components/c-select-many-to-many.md)

</td>
</tr>
</table>



Any other unsupported type will simply be displayed with [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md), unless a [default slot](https://vuejs.org/guide/components/slots.html) is provided - in that case, the default slot will be rendered instead.

When bound to a [ViewModel](/stacks/vue/layers/viewmodels.md), the [validation rules](/stacks/vue/layers/viewmodels.md#rules-validation) for the bound property will be obtained from the [ViewModel](/stacks/vue/layers/viewmodels.md#rules-validation) and passed to [Vuetify](https://vuetifyjs.com/)'s `rules` prop.

## Examples

### Model properties

Typical usage, providing an object and a property on that object:

``` vue-html
<c-input :model="person" for="firstName" />
```

Customizing the [Vuetify](https://vuetifyjs.com/) component used:

``` vue-html
<c-input :model="comment" for="content" textarea variant="solo" />
```

### Method Parameters

Binding to [Method Parameters](/modeling/model-components/methods.md#parameters) on an [API Caller](/stacks/vue/layers/api-clients.md#api-callers) args object:

``` vue-html
<c-input :model="person.setFirstName" for="newName" />
```

Or, without using an API Caller args object:

``` vue-html
<c-input v-model="newName" for="Person.methods.setFirstName.newName" />
```
``` ts
const newName = ref<string>();
```

### Data Source Parameters

``` vue-html
<c-input :model="nameStartsWithSource" for="startsWith" />
```
``` ts
const nameStartsWithSource = new PersonListViewModel.DataSources.NameStartsWith();
```

### Other usages

Usage with ``v-model`` (this scenario is atypical - the model/for pair of props are used in almost all scenarios):

``` vue-html
<c-input v-model="person.firstName" for="Person.firstName" />
```

## Props

In addition to the props below, all other attributes are passed through to the delegated-to component, allowing for full customization of the underlying [Vuetify](https://vuetifyjs.com/) component.


<!-- MARKER:c-for-model-props -->

<Prop def="for?: string | Property | Value" lang="ts" />

A metadata specifier for the value being bound. One of:
    
- A string with the name of the value belonging to `model`.
- A direct reference to a metadata object.
- A string in dot-notation that starts with a type name.

<Prop def="model?: Model | DataSource" lang="ts" />

An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

<!-- MARKER:c-for-model-props-end -->

<Prop def="modelValue?: any" lang="ts" />

If binding the component with ``v-model``, accepts the ``modelValue`` part of ``v-model``.


<Prop def="filter?: (value: EnumMember)" lang="ts" />

If binding to an enum, filters the enum members that are available for selection.


## Slots

``default`` - Used to display fallback content if c-input does not support the type of the value being bound. Generally this does not need to be used, as you should avoid creating c-input components for unsupported types in the first place.


