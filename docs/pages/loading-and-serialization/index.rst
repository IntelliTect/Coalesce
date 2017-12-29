
.. _ControllingLoading:

Intro to Loading and Serialization
==================================

   
Coalesce provides a few different ways to control what data is pulled out of the database and sent back to the client.


Controlling Loading
-------------------

Coalesce is based on :ref:`Entity Framework Core`. As a result, it is subjected to all of the rules and conventions that EF imposes. Chief among these is lazy loading - EF Core does not support lazy loading.

.. _DefaultLoadingBehavior:

Default Loading Behavior
........................

When an object or list of objects is requested, the default behavior of Coalesce is to load all of the immediate relationships of the object (parent objects and child collections), as well as the far side of many-to-many relationships.

In most cases, however, you'll probably want more or less data than what the default behavior provides. You can achieve this by using the methods outlined below.


Custom Data Sources
...................

:ref:`CustomDataSources` are the principal means of controlling loading and serialization in Coalesce.

They allow for simple customization of an EF query, or more advanced loading and definition of the structure of the serialized output data.

Read :ref:`CustomDataSources` to learn all about them.


Controlling Serialization
-------------------------

Include Tree
............

An important concept in controlling the output of Coalesce's API, :ref:`IncludeTree` is a representation of the structure of the object graph that should be sent to the client.

In most simple cases, it "just works" and is transparent to the developer, but there are cases where you perform complex loading from the database inside custom data sources or model methods and need to be able to control the shape of the returned data.

Read :ref:`IncludeTree` to learn more.

DtoIncludes & DtoExcludes
.........................

The :csharp:`DtoIncludes` and :csharp:`DtoExcludes` attributes allow specific properties to be conditionally whitelisted or blacklisted for/from inclusion into the :ref:`GenDTOs`, and ultimately, serialization to JSON and transmission to the client.

Read :ref:`Includes` and :ref:`DtoIncludesExcludesAttr` to learn more.
