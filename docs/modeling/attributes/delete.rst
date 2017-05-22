
Delete
======

Controls permissions for deletion of an object of this type through the API.

This attribute is a :csharp:`SecurityAttribute`. View the documentation for SecurityAttribute_ for details on usage.


Example Usage
-------------

    .. code-block:: c#

        [Delete(Roles = "DeleteRole", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
        public class Person
        {
            public int PersonId { get; set; }
            
            ...
        }