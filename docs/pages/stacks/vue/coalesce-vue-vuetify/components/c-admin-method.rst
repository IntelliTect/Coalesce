.. _c-admin-method:

c-admin-method
==============

.. MARKER:summary
    
Provides an interface for invoking a :ref:`method <ModelMethods>` and rendering its result, designed to be use in an admin page.

.. MARKER:summary-end

For each parameter of a method, a :ref:`c-input` will be rendered to accept the input of that parameter. A button is provided to trigger an invocation of the method, progress and errors are rendered with a :ref:`c-loader-status`, and results are rendered with :ref:`c-display`.

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-admin-method :model="person" for="setTitle" auto-reload-model />

Props
-----

:ts:`for: string | Method`
    A metadata specifier for the method. One of:
    
    - A string with the name of the method belonging to :ts:`model`. 
    - A direct reference to a method's metadata object.
    - A string in dot-notation that starts with a type name and ending with a method name.

:ts:`model: ViewModel | ListViewModel`
    An :ref:`ViewModel <VueInstanceViewModels>` or :ref:`ListViewModel <VueListViewModels>` owning the method and :ref:`API Caller <VueApiCallers>` that was specified by the :ts:`for` prop.

:ts:`autoReloadModel?: boolean = false`
    True if the :ts:`model` should have its :ts:`$load` invoked after a successful invocation of the method.


