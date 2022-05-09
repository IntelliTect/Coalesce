.. _c-list-range-display:

c-list-range-display
====================

.. MARKER:summary
    
Displays pagination information about the current `$items` of a :ref:`ListViewModel <VueListViewModels>` in the format ``<start index> - <end index> of <total count>``.

.. MARKER:summary-end

Uses the pagination information returned from the last successful `$load` call, not the current `$params` of the :ref:`ListViewModel <VueListViewModels>`.

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-list-range-display :list="list" />

Props
-----

`list: ListViewModel`
    The :ref:`ListViewModel <VueListViewModels>` to display pagination information for.




