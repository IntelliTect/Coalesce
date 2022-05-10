
.. This page is very dated, so it has been excluded from the docs. Most of what it explains is handled by _SecurityAttributes anyway.

.. _Security:

Security
--------

Overview
~~~~~~~~

Security is primarily handled through model annotations. These
annotations can be added to classes, properties, and methods.

Coalesce uses the built in Microsoft framework for security and supports
both Active Directory and Identity 3.

By default all views and APIs are marked with the [Authorize] attribute
by default. To make them available to anonymous users, they must be
annotated as such.

Internally these are implemented, when possible, with the MVC Authorize
attribute. When this isn't possible, they are implemented in code and
when serializing objects. For example, fields that are not readable to
the user are not serialized into the API response.

Classes
~~~~~~~

Basic security is done at the class level. There are two attributes that
can be used: Read and Edit. Both of these attributes take a Roles string
argument which allows a comma delimited list of role names. They both
also take an AllowAnonymous parameter, by default this is set to false.
When set to true, it opens the item to anonymous access.

The Read and Edit attributes can be combined to only allow edits for
certain users. Below is an example of how to allow read access to anyone
and write access only for administrators.

    ``` c#

        [Edit(Roles='Admin')] [Read(AllowAnonymous = true)]
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }


    ```

Additionally the Edit attribute has an Allow property that can be set to
false to disallow all edits. In fact this removes the API controller
method for editing.

Properties
~~~~~~~~~~

Properties can be secured in the same way that classes are secured. An
individual property can be secured for read and edit access by role.
AllowAnonymous must be applied to the class. This opens all methods and
must be secured if limited access to certain properties is desired.

The example below allows only admins to edit the last name.
Additionally, only admins can read and consequently edit the gender.

    ``` c#

        [Edit(Roles='Admin')] [Read(AllowAnonymous = true)]
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            [Edit(Roles="Admin")]
            public string LastName { get; set; }
            [Read(Roles="Admin")]
            public string Gender { get; set; }
        }


    ```

Methods
~~~~~~~

Methods are secured via the [Execute] attribute. It also uses the Roles
argument to specify the list of valid roles that can access the method.

The example below allows only admins to access the method.

    ``` c#

        public class Person
        {
            public int PersonId { get; set; }
            
            [Execute(Roles="Admin")]
            public string Hello() {
                return "Hello";
            }
        }


    ```

Using AuthorizeAttribute
~~~~~~~~~~~~~~~~~~~~~~~~

The code Coalesce generates doesn't rely on specifying roles or policies
in the Authorize attribute. In .Net Core this would require additional
work in Startup.cs to create policies

The example below maps the "User" role to an internal AD role name of
AllUsers.

    ``` c#

        RoleMapping.Add("User", "AllUsers");  // Interactive user.


    ```

