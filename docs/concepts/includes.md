
.. _Includes:

Includes String
===============

Coalesce provides a number of extension points for loading & serialization which make use of a concept called an "includes string" (also referred to as "include string" or just "includes").


.. contents:: Contents
    :local:
    

Includes String
---------------

The includes string is simply a string which can be set to any arbitrary value. It is passed from the client to the server in order to control data loading and serialization. It can be set on both the TypeScript ViewModels and the ListViewModels.

.. tabs::

    .. code-tab:: vue

        import { PersonViewModel, PersonListViewModel } from '@/viewmodels.g'
        
        var person = new PersonViewModel();
        person.$includes = "details";

        var personList = new PersonListViewModel();
        personList.$includes = "details";

    .. code-tab:: knockout

        var person = new ViewModels.Person();
        person.includes = "details";

        var personList = new ListViewModels.PersonList();
        personList.includes = "details";

The default value (i.e. no action) is the empty string.

Special Values
..............

There are a few values of `includes` that are either set by default in the auto-generated views, or otherwise have special meaning:

    :code:`none`
        Setting `includes` to ``none`` suppresses the :ref:`DefaultLoadingBehavior` provided by the :ref:`StandardDataSource` - The resulting data will be the requested object (or list of objects) and nothing more.

    :code:`Editor`
        Used when loading an object in the generated Knockout CreateEdit views.
        
    :code:`<ModelName>ListGen`
        Used when loading a list of objects in the generated Knockout Table and Cards views.
        For example, :code:`PersonListGen`
    
|

DtoIncludes & DtoExcludes
-------------------------

Main document: :ref:`DtoIncludesExcludesAttr`.

There are two C# attributes, `DtoIncludes` and `DtoExcludes`, that can be used to annotate your data model in order to control what data gets put into the DTOs and ultimately serialized to JSON and sent out to the client.

.. include:: ../modeling/model-components/attributes/dto-includes-excludes.rst
    :start-after: see :ref:`Includes`.
