# c-list-page

<!-- MARKER:summary -->
    
A component that provides previous/next buttons and a text field for modifying the `page` [parameter](/modeling/model-components/data-sources.md#standard-parameters) prop of a [ListViewModel](/stacks/vue/layers/viewmodels.md).

<!-- MARKER:summary-end -->

## Example Usage

``` vue-html
<c-list-page :list="list" />
```

## Props

<Prop def="list: ListViewModel" lang="ts" />

The [ListViewModel](/stacks/vue/layers/viewmodels.md) whose current page will be changeable with the component.




