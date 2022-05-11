# c-admin-table

<!-- MARKER:summary -->
    
An full-featured table for a [ListViewModel](/stacks/vue/layers/viewmodels.md), including a [c-admin-table-toolbar](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-toolbar.md), [c-table](/stacks/vue/coalesce-vue-vuetify/components/c-table.md), and [c-list-pagination](/stacks/vue/coalesce-vue-vuetify/components/c-list-pagination.md).

<!-- MARKER:summary-end -->

The table can be in read mode (default), or toggled into edit mode with the button provided by the [c-admin-table-toolbar](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-toolbar.md). When placed into edit mode, [auto-save](/stacks/vue/layers/viewmodels.md) is enabled.

## Examples

``` vue-html
<c-admin-table :list="personList" />
```

## Props

<Prop def="list: ListViewModel" lang="ts" />

The [ListViewModel](/stacks/vue/layers/viewmodels.md) to render a table for.

<Prop def="pageSizes?: number[]" lang="ts" />

An optional list of available page sizes to offer through the [c-list-pagination](/stacks/vue/coalesce-vue-vuetify/components/c-list-pagination.md)'s [c-list-page-size](/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.md) component. Defaults to `[10, 25, 100]`.

<Prop def="queryBind?: boolean" lang="ts" />

If true, the [Data Source Standard Parameters](/modeling/model-components/data-sources.md) of the provided [ListViewModel](/stacks/vue/layers/viewmodels.md) will be read from and written to the window's query string. The "Editable" state of the table will also be bound to the query string.

