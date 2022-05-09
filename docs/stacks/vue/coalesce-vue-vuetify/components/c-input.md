.. _c-input:

c-input
=======

.. MARKER:summary
    
A general-purpose input component for most :ref:`Values <VueMetadataValue>`. c-input does not have much functionality of its own - instead, it delegates to the right kind of component based on the type of value to which it is bound. This includes both other `Coalesce Vuetify Components <VuetifyOverview>`_ as well as direct usages of some Vuetify_ components.

.. MARKER:summary-end

All attributes are passed through to the delegated-to component, allowing for full customization of the underlying Vuetify_ component.

A summary of the components delegated to, by type: 

- string, number: `v-text-field <https://vuetifyjs.com/en/components/text-fields/>`_, or `v-textarea <https://vuetifyjs.com/en/components/textarea/>`_ if flag attribute ``textarea`` is provided to ``c-input``.
- boolean: `v-switch <https://vuetifyjs.com/en/components/selection-controls/>`_, or `v-checkbox <https://vuetifyjs.com/en/components/selection-controls/>`_ if flag attribute ``checkbox`` is provided to ``c-input``.
- enum: `v-select <https://vuetifyjs.com/en/components/selects/>`_
- file: `v-file-input <https://vuetifyjs.com/en/components/file-inputs/>`_
- date: :ref:`c-datetime-picker`
- model: :ref:`c-select`
- :ref:`ManyToMany` collection: :ref:`c-select-many-to-many`
- Non-object collection: :ref:`c-select-values`

Any other unsupported type will simply be displayed with :ref:`c-display`, unless a `default slot <https://vuejs.org/v2/guide/components-slots.html>`_ is provided - in that case, the default slot will be rendered instead.

When bound to a :ref:`ViewModel <VueInstanceViewModels>`, the :ref:`validation rules <VueViewModelsValidation>` for the bound property will be obtained from the :ref:`ViewModel <VueInstanceViewModels>` and passed to Vuetify_'s ``rules`` prop.

.. contents:: Contents
    :local:

Examples
--------

Typical usage, providing an object and a property on that object:

.. code-block:: sfc

    <c-input :model="person" for="firstName" />

Customizing the Vuetify_ component used:

.. code-block:: sfc

    <c-input :model="comment" for="content" textarea solo />

Binding to :ref:`API Caller <VueApiCallers>` args objects:

.. code-block:: sfc

    <c-input 
        :model="person.setFirstName" 
        for="newName" />

Or, using a more verbose syntax:

.. code-block:: sfc

    <c-input 
        :model="person.setFirstName.args" 
        for="Person.methods.setFirstName.newName" />

Binding to :ref:`Data Source Parameters <DataSourceParameters>`:

.. code-block:: sfc

    <c-input :model="personList.$dataSource" for="startsWith" />

Usage with ``v-model`` (this scenario is atypical - the model/for pair of props are used in almost all scenarios):

.. code-block:: sfc

    <c-input v-model="person.firstName" for="Person.firstName" />

Props
-----

.. MARKER:c-for-model-props

`for?: string | Property | Value`
    A metadata specifier for the value being bound. One of:
    
    - A string with the name of the value belonging to `model`.
    - A direct reference to a metadata object.
    - A string in dot-notation that starts with a type name.

`model?: Model | DataSource`
    An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

.. MARKER:c-for-model-props-end

`value?: any`
    If binding the component with ``v-model``, accepts the ``value`` part of ``v-model``.

Slots
-----

``default``
    Used to display fallback content if c-input does not support the type of the value being bound. Generally this does not need to be used, as you should avoid creating c-input components for unsupported types in the first place.


