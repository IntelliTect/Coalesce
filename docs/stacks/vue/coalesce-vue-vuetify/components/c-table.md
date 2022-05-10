.. _c-table:

c-table
=======

.. MARKER:summary
    
A table component for displaying the contents of a [ListViewModel](/stacks/vue/layers/viewmodels.md). Also supports modifying the list's [sort parameters](/modeling/model-components/data-sources.md) by clicking on column headers. Pairs well with a [c-list-pagination](/stacks/vue/coalesce-vue-vuetify/components/c-list-pagination.md).

.. MARKER:summary-end

[[toc]]


Examples
--------

A simple table, rendering the items of a [ListViewModel](/stacks/vue/layers/viewmodels.md):

``` vue-html

  <c-table :list="list" />
 

```

A more complex example using more of the available options: 

``` vue-html
    
  <c-table
    :list="list"
    :props="['firstName', 'lastName']"
    :extra-headers="['Actions']"
  >
    <template #item.append="{item}"> 
      <td>
        <v-btn
          title="Edit"
          text icon
          :to="{name: 'edit-person', params: { id: item.$primaryKey }}"
        >
          <i class="fa fa-edit"></i>
        </v-btn>
      </td>
    </template>
  </c-table>


```

Props
-----

`list: ListViewModel`
    The [ListViewModel](/stacks/vue/layers/viewmodels.md) to display pagination information for.

`props?: string[]`
    If provided, specifies which properties, and their ordering, should be given a column in the table. 
    
    If not provided, all non-key columns that aren't annotated with [[Hidden(HiddenAttribute.Areas.List)]](/modeling/model-components/attributes/hidden.md) are given a column.

`extraHeaders?: string[]`
    The text contents of one or more extra ``th`` elements to render in the table. Should be used in conjunction with the ``item.append`` slot.

`editable: boolean = false`
    If true, properties in each table cell will be rendered with [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md). Non-editable properties will be rendered in accordance with the value of the `admin` prop.

`admin: boolean = false`
    If true, properties in each table cell will be rendered with [c-admin-display](/stacks/vue/coalesce-vue-vuetify/components/c-admin-display.md) instead of [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md).

Slots
-----

``item.append``
    A slot rendered after the ``td`` elements on each row that render the properties of each item in the table. Should be provided zero or more additional ``td`` elements. The number should match the number of additional headers provided to the `extraHeaders` prop.