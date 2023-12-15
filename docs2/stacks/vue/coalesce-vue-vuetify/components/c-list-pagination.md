# c-list-pagination

<!-- MARKER:summary -->
    
A component that provides an interface for modifying the pagination [parameters](/modeling/model-components/data-sources.md#standard-parameters) of a [ListViewModel](/stacks/vue/layers/viewmodels.md).

This is a composite of [c-list-page-size](/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.md), [c-list-range-display](/stacks/vue/coalesce-vue-vuetify/components/c-list-range-display.md), and [c-list-page](/stacks/vue/coalesce-vue-vuetify/components/c-list-page.md), arranged horizontally. It is designed to be used above or below a table (e.g. [c-table](/stacks/vue/coalesce-vue-vuetify/components/c-table.md)).

<!-- MARKER:summary-end -->

## Example Usage

``` vue-html
<c-list-pagination :list="list" />
```

## Props

<Prop def="list: ListViewModel" lang="ts" />

The [ListViewModel](/stacks/vue/layers/viewmodels.md) whose pagination will be editable.

<Prop def="pageSizes?: number[]" lang="ts" />

An optional list of available page sizes to offer through [c-list-page-size](/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.md). Defaults to `[10, 25, 100]`.



