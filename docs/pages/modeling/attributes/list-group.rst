

List Group
==========

List groups are used for string fields that should provide a dropdown
list. This allows for multiple properties to contribute values to a
common list. This is a simple solution to using a linked table where
adding items is really easy, but it is also easy to select existing
items.


Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            
            [ListGroup("School")]
            public string Education1School { get; set; }

            [ListGroup("School")]
            public string Education2School { get; set; }
        }

    Example of usage in a .cshtml file to produce a dropdown that will present values from both :csharp:`Education1School` and :csharp:`Education2School`. The dropdown also allows for input of new values that aren't already in the list. See HtmlHelpers_ for more.

    .. code-block:: html

        <div>
            @(Knockout.SelectFor<Models.Person>(p => p.Education1School))
        </div>
