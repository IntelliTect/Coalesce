# c-select-many-to-many

<!-- MARKER:summary -->
    
A multi-select dropdown component that allows for selecting values fetched from the generated ``/list`` API endpoints for collection navigation properties that were annotated with [[ManyToMany]](/modeling/model-components/attributes/many-to-many.md).

<!-- MARKER:summary-end -->

::: tip
It is unlikely that you'll ever need to use this component directly - it is highly recommended that you use [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) instead and let it delegate to [c-select-many-to-many](/stacks/vue/coalesce-vue-vuetify/components/c-select-many-to-many.md) for you.
:::

[[toc]]

## Examples

``` vue-html
<c-select-many-to-many :model="case" for="caseProducts" />
```

``` vue-html
<c-select-many-to-many 
    :model="case" 
    for="caseProducts" 
    dense
    outlined
/>
```

``` vue-html
<c-select-many-to-many 
    v-model="case.caseProducts" 
    for="Case.caseProducts" 
/>
```

## Props

<Prop def="for: string | Property | Value" lang="ts" />

A metadata specifier for the value being bound. One of:
    
- A string with the name of the value belonging to `model`.
- A direct reference to a metadata object.
- A string in dot-notation that starts with a type name.

::: tip Note
c-select-many-to-many expects metadata for the "real" collection navigation property on a model. If you provide it the string you passed to [[ManyToMany]](/modeling/model-components/attributes/many-to-many.md), an error wil be thrown.
:::

<Prop def="model?: Model" lang="ts" />

An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

<Prop def="value?: any // Vue 2
modelValue?: any // Vue 3" lang="ts" />

If binding the component with ``v-model``, accepts the ``value`` part of ``v-model``.
    
<Prop def="params?: ListParameters" lang="ts" />

An optional set of [Data Source Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters) to pass to API calls made to the server.
    

## Events

The following events and automatic API calls are only used when bound to a `model` that has [auto-saves](/stacks/vue/layers/viewmodels.md#auto-save) enabled.

- `adding` - Fired when a new item has been selected, but before the call to `/save` has completed.
- `added` - Fired when the call to `/save` has completed after adding a new item.
- `deleting` - Fired when an item has been removed, but before the call to `/delete` has completed.
- `deleted` - Fired when the call to `/delete` has completed after removing an item.


