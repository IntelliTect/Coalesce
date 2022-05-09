
.. _c-display:

c-display
=========

.. MARKER:summary

A general-purpose component for displaying any :ref:`VueMetadataValue` by rendering the value to a string with the :ref:`display functions from the Models Layer <VueModelDisplayFunctions>`. For string and number :ref:`values <VueMetadataValue>`, usage of this component is largely superfluous. For all other value types including dates, booleans, enums, objects, and collections, it is very handy.

.. MARKER:summary-end


.. contents:: Contents
    :local:

Examples
--------

Typical usage, providing an object and a property on that object:

.. code-block:: sfc

    <c-display :model="person" for="gender" />

Customizing date formatting:

.. code-block:: sfc

    <c-display :model="person" for="birthDate" format="M/d/yyyy" />

A contrived example of using c-display to render the result of an :ref:`API Caller <VueApiCallers>`:

.. code-block:: sfc

    <c-display 
        :value="person.setFirstName.result" 
        :for="person.$metadata.methods.setFirstName.return" 
        element="div"
    />

Displaying a standalone date value without a model or other source of metadata:

.. code-block:: sfc

    <c-display :value="dateProp" format="M/d/yyyy" />

Props
-----

`for: string | Property | Value`
    A metadata specifier for the value being bound. Either a direct reference to the metadata object, or a string with the name of the value belonging to `model`, or a string in dot-notation that starts with a type name.

`model?: Model | DataSource`
    An object owning the value that was specified by the `for` prop.

`format: DisplayOptions["format"]`
    Shorthand for `:options="{ format: format }"`, allowing for specification of the format to be used when displaying dates.

    See :ref:`DisplayOptions` for details on the options available for `format`.

`options: DisplayOptions`
    Specify options for formatting some kinds of values, including dates. See :ref:`DisplayOptions` for details.

`value: any`
    Can be provided the value to be displayed in conjunction with the `for` prop, as an alternative to the `model` prop.

    This is an uncommon scenario - it is generally easier to use the `for`/`model` props together.

Slots
-----

``default``
    Used to display fallback content if the value being displayed is either `null` or `""` (empty string).


