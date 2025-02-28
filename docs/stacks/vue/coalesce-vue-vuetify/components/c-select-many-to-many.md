# c-select-many-to-many

<!-- MARKER:summary -->
    
A multi-select dropdown component that allows for selecting values fetched from the generated ``/list`` API endpoints for collection navigation properties that were annotated with [[ManyToMany]](/modeling/model-components/attributes/many-to-many.md).

<!-- MARKER:summary-end -->

::: tip
It is unlikely that you'll ever need to use this component directly - it is highly recommended that you use [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) instead and let it delegate to [c-select-many-to-many](/stacks/vue/coalesce-vue-vuetify/components/c-select-many-to-many.md) for you.
:::

## Examples

``` vue-html
<c-select-many-to-many :model="case" for="caseProducts" />
```

``` vue-html
<c-select-many-to-many 
    :model="case" 
    for="caseProducts"
    variant="outlined"
    density="compact"
/>
```

## Props

See [c-select / Props](./c-select.md#props). 

Since `c-select-many-to-many` internally uses `c-select` as its implementation, all props of `c-select` are also supported by `c-select-many-to-many`.

## Events

The following events and automatic API calls are only used when bound to a `model` that has [auto-saves](/stacks/vue/layers/viewmodels.md#auto-save) enabled.

- `adding` - Fired when a new item has been selected, but before the call to `/save` has completed.
- `added` - Fired when the call to `/save` has completed after adding a new item.
- `deleting` - Fired when an item has been removed, but before the call to `/delete` has completed.
- `deleted` - Fired when the call to `/delete` has completed after removing an item.


