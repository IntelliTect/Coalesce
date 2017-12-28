
.. _SecurityAttribute:
.. _SecurityAttributes:

Security Attributes
===================

Coalesce provides a collection of four attributes which can provide class-level (and property-level, where appropriate) security controls over the generated API.

This page serves to document the attributes themselves - for information on their implementation and impact on your generated code, see :ref:`Security`.


Properties
**********

    :csharp:`public string Roles { get; set; }` :ctor:`1`
        A comma-delimited list of roles that are authorized to take perform the action represented by the attribute. If the current user belongs to any of the listed roles, the action will be allowed.

        The string set for this property will be outputted as an :csharp:`[Authorize(Roles="RolesString")]` attribute on generated API controller actions.

    :csharp:`public SecurityPermissionLevels PermissionLevel { get; set; }` :ctor:`2`
        The level of access to allow for the action for **class-level security** only. Has no effect for property-level security.

        Enum values are:
            - :csharp:`SecurityPermissionLevels.AllowAll` Allow all users to perform the action for the attribute, including users who are not authenticated at all.
            - :csharp:`SecurityPermissionLevels.AllowAuthorized` Allow only users who are members of the roles specified on the attribute to perform the action. If no roles are specified on the attribute, then all authenticated users are allowed (no anonymous access). 
            - :csharp:`SecurityPermissionLevels.DenyAll` Deny the action to all users, regardless of authentication status or authorization level. If :csharp:`DenyAll` is used, no API endpoint for the action will be generated.


Class vs. Property Security
***************************

.. important::

    There are important differences between class-level security and property-level security, beyond the usage of the attributes themselves.
    Read the :ref:`Security` page for a detailed explanation of these differences.


Implementations
***************

Read
----

Controls permissions for reading of objects and properties through the API.

For **property-level** security only, if a :csharp:`[Read]` attribute is present without an :csharp:`[Edit]` attribute, the property is read-only. See :ref:`Security` for more info.

Example Usage
.............

    .. code-block:: c#

        [Read(Roles = "Management", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
        public class Employee
        {
            public int EmployeeId { get; set; }

            [Read("Payroll")]
            public string LastFourSsn { get; set; }
            
            ...
        }

|
Edit
----

Controls permissions for editing of objects and properties through the API.

For **property-level** security only, if a :csharp:`[Read]` attribute is present, one of its roles must be fulfilled in addition to the roles specified (if any) for the :csharp:`[Edit]` attribute. See :ref:`Security` for more info.

Example Usage
.............

.. code-block:: c#

    [Edit(Roles = "Management", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Edit("Payroll")]
        public string LastFourSsn { get; set; }
        
        ...
    }


|
Create
------

Controls permissions for deletion of an object of the targeted type through the API.

Example Usage
.............

    .. code-block:: c#

        [Create(Roles = "HumanResources", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
        public class Employee
        {
            ...
        }


|
Delete
------

Controls permissions for deletion of an object of the targeted type through the API.

Example Usage
.............

    .. code-block:: c#

        [Delete(Roles = "HumanResources,Management", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
        public class Employee
        {
            ...
        }


Execute
-------

A separate attribute for controlling method execution exists. Its documentation may be found on the ExecuteAttribute_ page.