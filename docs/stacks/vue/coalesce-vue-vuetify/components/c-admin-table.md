.. _c-admin-table:

c-admin-table
=============

.. MARKER:summary
    
An full-featured table for a :ref:`ListViewModel <VueListViewModels>`, including a :ref:`c-admin-table-toolbar`, :ref:`c-table`, and :ref:`c-list-pagination`.

.. MARKER:summary-end

The table can be in read mode (default), or toggled into edit mode with the button provided by the :ref:`c-admin-table-toolbar`. When placed into edit mode, :ref:`auto-save <VueViewModelsAutoSave>` is enabled.

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-admin-table :list="personList" />

Props
-----

`list: ListViewModel`
    The :ref:`ListViewModel <VueListViewModels>` to render a table for.

`pageSizes?: number[]`
    An optional list of available page sizes to offer through the :ref:`c-list-pagination`'s :ref:`c-list-page-size` component. Defaults to `[10, 25, 100]`.

`queryBind?: boolean`
    If true, the :ref:`Data Source Standard Parameters <DataSourceStandardParameters>` of the provided :ref:`ListViewModel <VueListViewModels>` will be read from and written to the window's query string. The "Editable" state of the table will also be bound to the query string.

