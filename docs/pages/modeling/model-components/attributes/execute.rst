
.. _ExecuteAttribute:

[Execute]
=========

Controls permissions for executing of a static or instance method through the API.

For other security controls, see :ref:`SecurityAttribute`.

Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }
            
            [Coalesce, Execute(Roles = "Payroll,HR")]
            public void GiveRaise(int centsPerHour) {
                ...
            }

            ...
        }


Properties
----------

    :csharp:`public string Roles { get; set; }`
        A comma-separated list of roles which are allowed to execute the method.