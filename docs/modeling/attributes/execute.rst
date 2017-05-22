
Execute
=======

Controls permissions for executing of a static or instance method through the API.

Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }
            
            [Execute(Roles = "Payroll,HR")]
            public void GiveRaise(int centsPerHour) {
                ...
            }

            ...\
        }


Properties
----------

    :csharp:`public string Roles { get; set; }`
        A comma-separated list of roles which are allowed to execute the method.