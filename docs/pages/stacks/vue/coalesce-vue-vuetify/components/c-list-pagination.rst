.. _c-list-pagination:

c-list-pagination
=================

.. MARKER:summary
    
A component that provides an interface for modifying the pagination :ref:`parameters <DataSourceStandardParameters>` of a :ref:`ListViewModel <VueListViewModels>`.

This is a composite of :ref:`c-list-page-size`, :ref:`c-list-range-display`, and :ref:`c-list-page`, arranged horizontally. It is designed to be used above or below a table (e.g. :ref:`c-table`).

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

  <c-list-pagination :list="list" />

Props
-----

:ts:`list: ListViewModel`
    The :ref:`ListViewModel <VueListViewModels>` whose pagination will be editable.

:ts:`pageSizes?: number[]`
    An optional list of available page sizes to offer through :ref:`c-list-page-size`. Defaults to :ts:`[10, 25, 100]`.



