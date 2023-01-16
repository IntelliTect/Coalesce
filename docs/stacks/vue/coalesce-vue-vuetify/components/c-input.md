# c-input

<!-- MARKER:summary -->
    
A general-purpose input component for most [Values](/stacks/vue/layers/metadata.md). c-input does not have much functionality of its own - instead, it delegates to the right kind of component based on the type of value to which it is bound. This includes both other [Coalesce Vuetify Components](/stacks/vue/coalesce-vue-vuetify/overview.md) as well as direct usages of some [Vuetify](https://vuetifyjs.com/) components.

<!-- MARKER:summary-end -->

All attributes are passed through to the delegated-to component, allowing for full customization of the underlying [Vuetify](https://vuetifyjs.com/) component.

A summary of the components delegated to, by type:

- string: 
    - [v-textarea](https://vuetifyjs.com/en/components/textarea/) if flag attribute ``textarea`` is provided to ``c-input`` or if `[DataType(DataType.MultilineText)]` is present in C#.
    - Otherwise, [v-text-field](https://vuetifyjs.com/en/components/text-fields/). Additionally, `[DataTypeAttribute]` values of `DataType.EmailAddress`, `DataType.PhoneNumber`, `DataType.Password`, or `"Color"` on the field will apply appropriate adjustments to the field.
- number: [v-text-field](https://vuetifyjs.com/en/components/text-fields/).
- boolean: [v-switch](https://vuetifyjs.com/en/components/selection-controls/), or [v-checkbox](https://vuetifyjs.com/en/components/selection-controls/) if flag attribute ``checkbox`` is provided to ``c-input``.
- enum: [v-select](https://vuetifyjs.com/en/components/selects/)
- file: [v-file-input](https://vuetifyjs.com/en/components/file-inputs/)
- date: [c-datetime-picker](/stacks/vue/coalesce-vue-vuetify/components/c-datetime-picker.md)
- model: [c-select](/stacks/vue/coalesce-vue-vuetify/components/c-select.md)
- [[ManyToMany]](/modeling/model-components/attributes/many-to-many.md) collection: [c-select-many-to-many](/stacks/vue/coalesce-vue-vuetify/components/c-select-many-to-many.md)
- Non-object collection: [c-select-values](/stacks/vue/coalesce-vue-vuetify/components/c-select-values.md)

Any other unsupported type will simply be displayed with [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md), unless a [default slot](https://vuejs.org/v2/guide/components-slots.html) is provided - in that case, the default slot will be rendered instead.

When bound to a [ViewModel](/stacks/vue/layers/viewmodels.md), the [validation rules](/stacks/vue/layers/viewmodels.md#rules-validation) for the bound property will be obtained from the [ViewModel](/stacks/vue/layers/viewmodels.md#rules-validation) and passed to [Vuetify](https://vuetifyjs.com/)'s `rules` prop.

[[toc]]

## Examples

Typical usage, providing an object and a property on that object:

``` vue-html
<c-input :model="person" for="firstName" />
```

Customizing the [Vuetify](https://vuetifyjs.com/) component used:

``` vue-html
<c-input :model="comment" for="content" textarea solo />
```

Binding to [API Caller](/stacks/vue/layers/api-clients.md#api-callers) args objects:

``` vue-html
<c-input 
    :model="person.setFirstName" 
    for="newName" />
```

Or, using a more verbose syntax:

``` vue-html
<c-input 
    :model="person.setFirstName.args" 
    for="Person.methods.setFirstName.newName" />
```

Binding to [Data Source Parameters](/modeling/model-components/data-sources.md#custom-parameters):

``` vue-html
<c-input :model="personList.$dataSource" for="startsWith" />
```

Usage with ``v-model`` (this scenario is atypical - the model/for pair of props are used in almost all scenarios):

``` vue-html
<c-input v-model="person.firstName" for="Person.firstName" />
```

## Props

<!-- MARKER:c-for-model-props -->

<Prop def="for?: string | Property | Value" lang="ts" />

A metadata specifier for the value being bound. One of:
    
- A string with the name of the value belonging to `model`.
- A direct reference to a metadata object.
- A string in dot-notation that starts with a type name.

<Prop def="model?: Model | DataSource" lang="ts" />

An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

<!-- MARKER:c-for-model-props-end -->

<Prop def="value?: any" lang="ts" />

If binding the component with ``v-model``, accepts the ``value`` part of ``v-model``.

## Slots

``default`` - Used to display fallback content if c-input does not support the type of the value being bound. Generally this does not need to be used, as you should avoid creating c-input components for unsupported types in the first place.


