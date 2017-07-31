
.. _ListTextAttribute:

List Text
=========

When a dropdown list is used to select a related object, this controls the text shown in the dropdown by default. When using these dropdown, only the key and this field are returned as search results by the API.

The property with this attribute will also be used as the displayed text for reference navigation properties when they are displayed as text using the :ref:`Computed Text Properties <TypeScriptViewModelComputedText>` properties on the :ref:`TypeScriptViewModel`.

If this attribute is not used, and a property named "Name" exists on the model, that property will be used. Otherwise, the primary key will be used.


Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            [ListText]
            [Hidden]
            [NotMapped]
            public string Name => FirstName + " " + LastName
        }