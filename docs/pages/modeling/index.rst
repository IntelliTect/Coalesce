
Intro to Modeling
=================


Overview
--------

Models are the core business objects of your application - they serve as the fundamental representation of data in your application. The design of your models is very important. In `Entity Framework Core`_, data models are just Plain Old CLR Objects (POCOs).

.. _Entity Framework Core:
.. _EF Core:
.. _EF:
    https://docs.microsoft.com/en-us/ef/core/



Building a Data Model
---------------------

To build your data model that Coalesce will generate code for, follow the best practices for `EF Core`_.

Guidance on this topic is available in abundance in the `Entity Framework Core`_ documentation.

Don't worry about querying or saving data for now - Coalesce will provide a lot of that functionality for you. To get started, just build your POCOs and :csharp:`DbContext` classes.

Before you start building, you are highly encouraged to read the sections below. The linked pages explain in greater detail what Coalesce will build for you for each part of your data model.


Properties
~~~~~~~~~~

Read :ref:`ModelProperties` for an outline of the different types of properties that you may place on your models and the code that Coalesce will generate for each of them.


Attributes
~~~~~~~~~~

Coalesce provides a number of C# attributes that can be used to decorate your model classes and their properties in order to customize behavior, appearance, security, and more. Coalesce also supports a number of annotations from :csharp:`System.ComponentModel.DataAnnotations`.

Read :ref:`ModelAttributes` to learn more.


Interfaces
~~~~~~~~~~

There are several interfaces in Coalesce that you may implement in your models to add custom behavior. Read :ref:`ModelInterfaces` to learn about the different model interfaces and what they can offer.


Methods
~~~~~~~

You can place both static and interface methods on your model classes. Any public methods will have a generated API endpoint and corresponding generated TypeScript members for calling this API endpoint. Read :ref:`ModelMethods` to learn more.


