

Inject
======

Used to mark a method parameter for dependency injection from the application's :csharp:`IServiceProvider`.

This gets translated to a :csharp:`Microsoft.AspNetCore.Mvc.FromServicesAttribute` in the generated API controller's action.


Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string GetFullName([Inject] ILogger<Person> logger)
            {
                logger.LogInformation(0, "Person " + PersonId + "'s full name was requested");
                return FirstName + " " + LastName";
            }
        }