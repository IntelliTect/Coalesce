# c-admin-table-column-selector

<!-- MARKER:summary -->
    
A dropdown menu component for selecting which columns to display in a table. Provides checkboxes for each available column with bulk selection actions.

<!-- MARKER:summary-end -->

This component is typically used internally by [c-admin-table](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table.md), but can be used independently for custom table implementations.

## Examples

``` vue-html
<c-admin-table-column-selector
  :available-props="availableProperties"
  :selected-columns="selectedColumns"
  :default-columns="defaultColumns"
  @update:selected-columns="updateSelection"
/>
```

## Props

<Prop def="availableProps: Property[]" lang="ts" />

An array of [Property](/modeling/model-components/properties.md) objects representing the columns that can be selected.

<Prop def="selectedColumns: string[]" lang="ts" />

An array of property names for the currently selected columns.

<Prop def="defaultColumns: string[]" lang="ts" />

An array of property names representing the default column selection. Used by the "Reset" action.

## Events

<Prop def="@update:selectedColumns" lang="ts" />

Emitted when the user changes the column selection. The event payload is an array of selected property names.

## Actions

The component provides three action buttons:

- **Select All**: Selects all available columns
- **Select None**: Deselects all columns  
- **Reset**: Restores the default column selection