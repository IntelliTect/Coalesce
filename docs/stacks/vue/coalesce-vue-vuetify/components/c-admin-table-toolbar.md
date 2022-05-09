.. _c-admin-table-toolbar:

c-admin-table-toolbar
=====================

.. MARKER:summary
    
A full-featured toolbar for a :ref:`ListViewModel <VueListViewModels>` designed to be used on an admin page, including "Create" and "Reload" buttons, a :ref:`c-list-range-display`, a :ref:`c-list-page`, a search field, :ref:`c-list-filters`, and a :ref:`c-list-page-size`.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-admin-table-toolbar :list="personList" />

.. code-block:: sfc

    <c-admin-table-toolbar :list="personList" color="pink" :editable.sync="isEditable" />

Props
-----

`list: ListViewModel`
    The :ref:`ListViewModel <VueListViewModels>` to render the toolbar for.

`color: string = "primary"`
    The `color <https://vuetifyjs.com/en/styles/colors/>`_ of the toolbar.

`editable?: boolean`
    If provided, adds a button to toggle the value of this prop. Should be bound with the `.sync` modifier.



