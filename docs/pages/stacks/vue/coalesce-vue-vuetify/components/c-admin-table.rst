.. _c-admin-table:

c-admin-table
=============

.. MARKER:summary
    
An full-featured table for a :ref:`ListViewModel <VueListViewModels>`, including a :ref:`c-admin-table-toolbar`, :ref:`c-table`, and :ref:`c-list-pagination`.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-admin-table :model="personList" />

Props
-----

:ts:`list: ListViewModel`
    The :ref:`ListViewModel <VueListViewModels>` to render a table for.


:ts:`pageSizes?: number[]`
    An optional list of available page sizes to offer through the :ref:`c-list-pagination`'s :ref:`c-list-page-size` component. Defaults to :ts:`[10, 25, 100]`.

