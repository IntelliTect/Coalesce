# c-list-page-size

<!-- MARKER:summary -->
    
A component that provides an dropdown for modifying the `pageSize` [parameter](/modeling/model-components/data-sources.md#standard-parameters) prop of a [ListViewModel](/stacks/vue/layers/viewmodels.md).

<!-- MARKER:summary-end -->

## Example Usage

``` vue-html
<c-list-page-size :list="list" />
```

## Props

<Prop def="list: ListViewModel" lang="ts" />

The [ListViewModel](/stacks/vue/layers/viewmodels.md) whose pagination will be editable.

<Prop def="pageSizes?: number[]" lang="ts" />

An optional list of available page sizes to offer through [c-list-page-size](/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.md). Defaults to `[10, 25, 100]`.



