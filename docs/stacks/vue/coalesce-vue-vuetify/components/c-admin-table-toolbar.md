# c-admin-table-toolbar

<!-- MARKER:summary -->
    
A full-featured toolbar for a [ListViewModel](/stacks/vue/layers/viewmodels.md) designed to be used on an admin page, including "Create" and "Reload" buttons, a [c-list-range-display](/stacks/vue/coalesce-vue-vuetify/components/c-list-range-display.md), a [c-list-page](/stacks/vue/coalesce-vue-vuetify/components/c-list-page.md), a search field, [c-list-filters](/stacks/vue/coalesce-vue-vuetify/components/c-list-filters.md), and a [c-list-page-size](/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.md).

<!-- MARKER:summary-end -->

## Examples

``` vue-html
<c-admin-table-toolbar :list="personList" />
```

``` vue-html
<c-admin-table-toolbar :list="personList" color="pink" :editable.sync="isEditable" />
```

## Props

<Prop def="list: ListViewModel" lang="ts" />

The [ListViewModel](/stacks/vue/layers/viewmodels.md) to render the toolbar for.

<Prop def="color: string = 'primary'" lang="ts" />

The [color](https://vuetifyjs.com/en/styles/colors/) of the toolbar.

<Prop def="editable?: boolean" lang="ts" />

If provided, adds a button to toggle the value of this prop. Should be bound with the `.sync` modifier.



