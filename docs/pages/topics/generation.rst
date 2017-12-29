Code Generation
===============

Coalesce generates a number of bits and pieces of code based on your models.

.. contents:: Contents
    :local:



Generated Code
--------------

Below you will find a brief overview of each of the different pieces of code that Coalesce will generate for you.


TypeScript
..........

Coalesce generates a number of different types of TypeScript classes to support your data through the generated API.

ViewModels
    One view model class is generated for each of your EF Database-mapped POCO classes. These models contain fields for your model :ref:`ModelProperties`, and functions and other members for your model :ref:`ModelMethods`. They also contain a number of standard fields & functions inherited from :ts:`BaseViewModel` which offer basic loading & saving functionality, as well as other handy utility members for use with Knockout.

    See :ref:`TypeScriptViewModel` for more details.

ListViewModels
    One ListViewModel is generated for each of your EF Database-mapped POCO classes. These classes contain functionality for loading sets of objects from the server. They provide searching, paging, sorting, and filtering functionality.

    See :ref:`TypeScriptListViewModel` for more details.

ExternalType
    Any types which are accessible through your Database-mapped POCO classes, either through one of its getter-only :ref:`ModelProperties` or return value from one of its :ref:`ModelMethods`, will have a corresponding TypeScript ViewModel generated for it. These ViewModels only provide a :ts:`KnockoutObservable` field for each property on the C# class.


C# DTOs
.......

For each of your EF Database-mapped POCO classes, a C# DTO class is created. These classes are used to hold the data that will be serialized and sent to the client, as well as data that has been received from the client before it has been mapped back to your EF POCO class.

See :ref:`GenDTOs` for more information.


API Controllers
...............

For each of your EF Database-mapped POCO classes, an API controller is created in the ``/Api/Generated`` directory of your web project. These controllers provide a number of endpoints for interacting with your data.

.. See :ref:`Api` for more information.


View Controllers
................

For each of your EF Database-mapped POCO classes, a controller is created in the ``/Controllers/Generated`` directory of your web project. These controllers provide routes for the generated admin views.

As you add your own pages to your application, you should add additional partial classes in the ``/Controllers`` that extend these generated partial classes to expose those pages.


Admin Views
...........

For each of your EF Database-mapped POCO classes, a number of views are generated to provide administrative-level access to your data.

Table
    Provides a basic table view with sorting, searching, and paging of your data.

TableEdit
    Provides the table view, but with inline editing in the table.

Cards
    Provides a card-based view of your data with searching and paging.

CreateEdit
    Provides an editor view which can be used to create new entities or edit existing ones.

EditorHtml
    Provides a minimal amount of HTML to display an editor for the object type. This is used by the :ts:`showEditor` method on the generated TypeScript ViewModels.

