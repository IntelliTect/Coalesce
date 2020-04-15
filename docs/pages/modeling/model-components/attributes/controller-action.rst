.. _ControllerActionAttribute:

[ControllerAction]
=====================

Specifies the HTTP method/verb to use in the generated API controller for the model method. If this attribute is excluded or no 
method is specified, the controller action will use POST. Note that using the GET method will cause all method parameters to be returned as URL parameters
which are not encrypted.

Example Usage
-------------

.. code-block:: c#

    public class Person
    {
        public int PersonId { get; set; }
        public string {get; set; }

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Get)]
        public static long PersonCount(AppDbContext db, string lastNameStartsWith = "")
        {
            return db.People.Count(f => f.LastName.StartsWith(lastNameStartsWith));
        }
    }

Properties
----------

:csharp:`public HttpMethod Method { get; set; }` :ctor:`1`
    The HTTP method to use on the generated API Controller.

    Enum values are:
        - :csharp:`HttpMethod.Post` Use the POST method.
        - :csharp:`HttpMethod.Get` Use the GET method.
        - :csharp:`HttpMethod.Put` Use the PUT method.
        - :csharp:`HttpMethod.Delete` Use the DELETE method.
        - :csharp:`HttpMethod.Patch` Use the PATCH method.
