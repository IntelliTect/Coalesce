
.. _InjectAttribute:

[Inject]
========

Used to mark a :ref:`Method <ModelMethods>` parameter for dependency injection from the application's :csharp:`IServiceProvider`.

See :ref:`ModelMethods` for more.

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
            logger.LogInformation("Person " + PersonId + "'s full name was requested");
            return FirstName + " " + LastName";
        }
    }