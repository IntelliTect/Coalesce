# c-list-filters

<!-- MARKER:summary -->
    
A component that provides an interface for modifying the `filters` prop of a [ListViewModel](/stacks/vue/layers/viewmodels.md)'s [parameters](/modeling/model-components/data-sources.md#standard-parameters). When `columnSelection` is enabled, also provides column visibility controls.

<!-- MARKER:summary-end -->

## Example Usage

### Filters Only
``` vue-html
<c-list-filters :list="list" :available-props="availableProps" />
```

### Filters with Column Selection
``` vue-html
<c-list-filters 
  :list="list" 
  :available-props="availableProps"
  v-model:selected-columns="selectedColumns"
  column-selection
/>
```

## Props

<Prop def="list: ListViewModel" lang="ts" />

The [ListViewModel](/stacks/vue/layers/viewmodels.md) whose filters will be editable.

<Prop def="columnSelection?: boolean" lang="ts" />

When `true`, enables column selection functionality. The component will show checkboxes next to each property name to control column visibility, and the button text and icon will change to indicate "Columns & Filters" mode.

## Model Values

<Prop def="v-model:selectedColumns?: string[] | null" lang="ts" />

When `columnSelection` is enabled, this model value controls which columns are currently selected for display. 
- `null` indicates default columns should be used
- An array of property names indicates the specific columns that should be visible




