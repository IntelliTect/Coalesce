.. _c-list-page-size:

c-list-page-size
================

.. MARKER:summary
    
A component that provides an dropdown for modifying the :ts:`pageSize` :ref:`parameter <DataSourceStandardParameters>` prop of a :ref:`ListViewModel <VueListViewModels>`.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

  <c-list-page-size :list="list" />

Props
-----

:ts:`list: ListViewModel`
    The :ref:`ListViewModel <VueListViewModels>` whose pagination will be editable.

:ts:`pageSizes?: number[]`
    An optional list of available page sizes to offer through :ref:`c-list-page-size`. Defaults to :ts:`[10, 25, 100]`.



