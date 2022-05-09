.. _c-select-values:

c-select-values
===============

.. MARKER:summary
    
A multi-select input component for collections of non-object values (primarily strings and numbers).

.. MARKER:summary-end

.. tip:: 
    It is unlikely that you'll ever need to use this component directly - it is highly recommended that you use :ref:`c-input` instead and let it delegate to :ref:`c-select-values` for you.

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: sfc

    <c-select-values 
        :model="post.setTags.args" 
        for="Post.methods.setTags.params.tagNames" 
    />

Props
-----

`for: string | CollectionProperty | CollectionValue`
    A metadata specifier for the value being bound. One of:
    
    - A string with the name of the value belonging to `model`.
    - A direct reference to a metadata object.
    - A string in dot-notation that starts with a type name.

`model?: Model`
    An object owning the value that was specified by the `for` prop.

`value: any`
    If binding the component with ``v-model``, accepts the ``value`` part of ``v-model``.


