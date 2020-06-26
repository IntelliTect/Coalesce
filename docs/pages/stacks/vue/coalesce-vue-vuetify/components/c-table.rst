.. _c-table:

c-table
=======

.. MARKER:summary
    
A table component for displaying the contents of a :ref:`ListViewModel <VueListViewModels>`. Also supports modifying the list's :ref:`sort parameters <DataSourceStandardParameters>` by clicking on column headers. Pairs well with a :ref:`c-list-pagination`.

.. MARKER:summary-end

.. contents:: Contents
    :local:


Examples
--------

A simple table, rendering the items of a :ref:`ListViewModel <VueListViewModels>`:

.. code-block:: sfc

  <c-table :list="list" />
 
A more complex example using more of the available options: 

.. code-block:: sfc
    
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

Props
-----

:ts:`list: ListViewModel`
    The :ref:`ListViewModel <VueListViewModels>` to display pagination information for.

:ts:`props?: string[]`
    If provided, specifies which properties should be given a column in the table. 
    
    If not provided, all non-key columns that aren't annotated with :ref:`[Hidden(HiddenAttribute.Areas.List)] <HiddenAttribute>` are given a column.

:ts:`extraHeaders?: string[]`
    The text contents of one or more extra ``th`` elements to render in the table. Should be used in conjunction with the ``item.append`` slot.

:ts:`editable: boolean = false`
    If true, properties in each table cell will be rendered with :ref:`c-input`. Non-editable properties will be rendered in accordance with the value of the :ts:`admin` prop.

:ts:`admin: boolean = false`
    If true, properties in each table cell will be rendered with :ref:`c-admin-display` instead of :ref:`c-display`.

Slots
-----

``item.append``
    A slot rendered after the ``td`` elements on each row that render the properties of each item in the table. Should be provided zero or more additional ``td`` elements. The number should match the number of additional headers provided to the :ts:`extraHeaders` prop.