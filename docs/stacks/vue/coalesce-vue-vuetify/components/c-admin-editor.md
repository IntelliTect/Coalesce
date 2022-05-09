.. _c-admin-editor:

c-admin-editor
==============

.. MARKER:summary
    
An editor for a single :ref:`ViewModel <VueInstanceViewModels>` instance. Provides a :ref:`c-input` for each property of the model.

.. MARKER:summary-end

Does not automatically enable :ref:`auto-save <VueViewModelsAutoSave>` - if desired, this must be enabled by the implementor of this component.

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-admin-editor :model="person" />

Props
-----

`model: ViewModel | ListViewModel`
    The :ref:`ViewModel <VueInstanceViewModels>` to render an editor for.



