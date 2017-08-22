
Intro to Modeling
=================


Overview
--------

Models are the core business objects of your application. More specifically, they
are the fundamental representation of data in your application. The design of your
models is very important. In `Entity Framework Core`_, data models are just
Plain Old CLR Classes (POCOs).

.. _Entity Framework Core:
.. _EF Core:
.. _EF:
    https://docs.microsoft.com/en-us/ef/core/



Building a Data Model
---------------------

To build your data model that Coalesce will generate code for, follow the best practices for `EF Core`_.

Guidance on this topic is available in abundance in the `Entity Framework Core`_ documentation.

Don't worry about querying or saving data for now - Coalesce will provide a lot of that functionality for you. To get started, just build your POCOs and :csharp:`DbContext` classes.


Customizing Your Data Model
---------------------------

Once you have built out a simple POCO data model, you can get started on the fun part - customizing it.

Coalesce includes a number of ways in which you can cutomize your data model. Cutomizations affect the generated API and the generated views.


Properties
~~~~~~~~~~

Read :ref:`ModelProperties` for an outline of the different types of properties that you may place on your models and the code that Coalesce will generate for each of them.


Attributes
~~~~~~~~~~

Coalesce provides a number of C# attributes that can be used to decorate your model classes and their properties in order to customize behavior, appearance, security, and more. Coalesce also supports a number of annotations from :csharp:`System.ComponentModel.DataAnnotations`.

Read :ref:`ModelAttributes` to learn more.


Interfaces
~~~~~~~~~~

TODO: INSERT LINKS TO THESE PAGES IN SMALL INTRO SECTIONS IN THIS DOC LIKE I DID WITH LOADING-AND-SERIALIZATION.RST
    modeling/interfaces
    modeling/methods
    modeling/external-type



| 

**The generated admin views have documentation pages that include all
view model documentation specific to each class.**


Validation
^^^^^^^^^^

Validation is handled using standard MVC attributes. These attributes
are not only enforced on the server side in the database, but are also
passed to the client and enforced using the KnockoutValidation_ library.
There is also flexible annotation-based validation for the client side.
Full validation documentation is in the Annotations section

| 


| 

Security
^^^^^^^^

Security is handled via attributes on the class, properties, and
methods. See :ref:`Security` for more information.
