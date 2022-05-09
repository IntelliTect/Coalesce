.. _c-admin-methods:

c-admin-methods
===============

.. MARKER:summary
    
Renders in a Vuetify_ `v-expansion-panels <https://vuetifyjs.com/en/components/expansion-panels/>`_ a :ref:`c-admin-method` for each method on a :ref:`ViewModel <VueInstanceViewModels>` or :ref:`ListViewModel <VueListViewModels>`.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-admin-methods :model="person" auto-reload-model />
    
.. code-block:: sfc

    <c-admin-methods :model="personList" auto-reload-model />

Props
-----

`model: ViewModel | ListViewModel`
    An :ref:`ViewModel <VueInstanceViewModels>` or :ref:`ListViewModel <VueListViewModels>` whose methods should each render as a :ref:`c-admin-method`.

`autoReloadModel?: boolean = false`
    True if the `model` should have its `$load` invoked after a successful invocation of any method.




