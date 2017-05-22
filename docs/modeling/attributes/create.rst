
Create
======

Controls permissions for creation of a new object of this type through the API.

This attribute is a :csharp:`SecurityAttribute`. View the documentation for SecurityAttribute_ for details on usage.

Example Usage
-------------

.. code-block:: c#

    [Create(Roles = "CreateRole", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
    public class Person
    {
        public int PersonId { get; set; }
        
        ...
    }