.. _c-select-many-to-many:

c-select-many-to-many
=====================

.. MARKER:summary
    
A multi-select dropdown component that allows for selecting values fetched from the generated ``/list`` API endpoints for collection navigation properties that were annotated with :ref:`ManyToMany`.

.. MARKER:summary-end

.. tip:: 
    It is unlikely that you'll ever need to use this component directly - it is highly recommended that you use :ref:`c-input` instead and let it delegate to :ref:`c-select-many-to-many` for you.

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-select-many-to-many :model="case" for="caseProducts" />

.. code-block:: sfc

    <c-select-many-to-many 
        :model="case" 
        for="caseProducts" 
        dense
        outlined

    />

.. code-block:: sfc

    <c-select-many-to-many 
        v-model="case.caseProducts" 
        for="Case.caseProducts" 
    />

Props
-----

`for: string | Property | Value`
    A metadata specifier for the value being bound. One of:
    
    - A string with the name of the value belonging to `model`.
    - A direct reference to a metadata object.
    - A string in dot-notation that starts with a type name.

    .. important::

        c-select-many-to-many expects metadata for the "real" collection collection navigation property on a model. If you provide it the string you passed to :ref:`ManyToMany`, an error wil be thrown.

`model?: Model`
    An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

`value: any`
    If binding the component with ``v-model``, accepts the ``value`` part of ``v-model``.
    
`params?: ListParameters`
    An optional set of :ref:`Data Source Standard Parameters <DataSourceStandardParameters>` to pass to API calls made to the server.
    



