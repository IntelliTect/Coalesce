
.. _Includes:

Includes
========

Coalesce provides a number of extension points for loading & serialization which make use of a concept called an "includes string" (also referred to as "include string" or just "includes").


.. contents:: Contents
    :local:
    
    

Includes String
---------------

The includes string is simply a string which can be set to any aribitary value. It is passed from the client to the server in order to control data loading and serialization. It can be set on both the TypeScript ViewModels and the ListViewModels.

    .. code-block:: typescript 

        var person = new ViewModels.Person();
        person.includes = "details";

        var personList = new ListViewModels.PersonList();
        personList.includes = "details";

The default value (i.e. no action) is the empty string.

Special Values
..............

    There are a few values of :ts:`includes` that are either set by default in the auto-generated views, or otherwise have special meaning:

        :code:`none`
            Setting :ts:`includes` to ``none`` prevents :csharp:`IIncludable.Include` (see below) from being called, and also suppresses the :ref:`Default Loading Behavior`. The resulting data will be the requested object (or list of objects) and nothing more.

        :code:`Editor`
            Used when loading an object in the generated CreateEdit views.
            
        :code:`<ModelName>ListGen`
            Used when loading a list of objects in the generated Table and Cards views.
            For example, :code:`PersonListGen`

|

.. _IIncludable:

IIncludable
-----------

The :csharp:`IIncludable` interface offers a way to control loading very similar to :ref:`CustomDataSources`.

.. tip::
    While :csharp:`IIncludable` provides much of the same functionality, :ref:`CustomDataSources` are the recommended way to control loading and serialization of your data when using Coalesce.
    
    :ref:`CustomDataSources` are more modular and more discoverable than functionality tucked away inside the :csharp:`Include` method.

Instead of using :ref:`CustomDataSources` and defining separate methods for each loading technique, :csharp:`IIncludable` provides a single method to define all potential ways of loading data. It is passed the :ts:`include` string.

    .. code-block:: c#

        public class Person : IIncludable<Person>
        {
            public IQueryable<Person> Include(IQueryable<Person> entities, string includes = null)
            {
                entities = entities
                    .Include(f => f.CasesAssigned)
                    .Include(f => f.Company);

                if (include == "network")
                    entities = entities
                        .Include(f => f.Subordinates)
                        .Include(f => f.Supervisor);

                return entities;
            }
        }

In cases where :ref:`CustomDataSources` are used to control loading, the :csharp:`IIncludable.Include` method will not be called. 

When :csharp:`IIncludable` is used, the :ref:`Default Loading Behavior` is suppressed. To get this behavior easily inside your :csharp:`IIncludable.Include` implementation, call the :csharp:`IQueryable<T>.IncludeChildren()` extension method (:csharp:`using IntelliTect.Coalesce.Data;`).

    
|
DtoIncludes & DtoExcludes
-------------------------

Main document: :ref:`DtoIncludesExcludesAttr`.

There are two C# attributes, :csharp:`DtoIncludes` and :csharp:`DtoExcludes`, that can be used to annotate your data model in order to control what data gets put into the DTOs and ultimately serialized to JSON and sent out to the client.

.. include:: ../attributes/dto-includes-excludes.rst
    :start-after: see :ref:`Includes`.
