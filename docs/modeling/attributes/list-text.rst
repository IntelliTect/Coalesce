
List Text
=========

When a dropdown list is used to select a related object, this controls
the text shown in the dropdown by default. When using these dropdown,
only the key and this field are returned as search results.


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