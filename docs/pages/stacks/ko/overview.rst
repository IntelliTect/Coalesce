
.. _KoOverview:

Knockout Overview
=================

The Knockout_ stack for Coalesce offers the ability to build pages with the time-tested Knockout_ JavaScript library using all of the features of the Coalesce generated APIs and :ref:`ViewModels <TypeScriptViewModels>`. It can be used for anything between adding simple interactive augmentations of MVC pages to building a full MPA-SPA hybrid application.

.. contents:: Contents
    :local:

Getting Started
---------------

Check out :ref:`KoGettingStarted` if you haven't already to learn how to get a new Coalesce Knockout project up and running.

Generated Code
--------------

Below you will find a brief overview of each of the different pieces of code that Coalesce will generate for you when you choose the Knockout stack.

TypeScript
..........

Coalesce generates a number of different types of TypeScript classes to support your data through the generated API.


:ref:`ViewModels <KoTypeScriptViewModels>`
    One view model class is generated for each of your :ref:`EntityModels` and :ref:`CustomDTOs`. These models contain fields for your model :ref:`ModelProperties`, and functions and other members for your model :ref:`ModelMethods`. They also contain a number of standard fields & functions inherited from :ts:`BaseViewModel` which offer basic loading & saving functionality, as well as other handy utility members for use with Knockout.

    See :ref:`KoTypeScriptViewModels` for more details.

:ref:`List ViewModels <KoTypeScriptListViewModels>`
    One ListViewModel is generated for each of your :ref:`EntityModels` and :ref:`CustomDTOs`. These classes contain functionality for loading sets of objects from the server. They provide searching, paging, sorting, and filtering functionality.

    See :ref:`KoTypeScriptListViewModels` for more details.

:ref:`External Type ViewModels <KoExternalViewModel>`
    Any non-primitive types which are not themselves a :ref:`EntityModels` or :ref:`CustomDTOs` which are accessible through the aforementioned types, either through one of its :ref:`ModelProperties`, or return value from one of its :ref:`ModelMethods`, will have a corresponding TypeScript ViewModel generated for it. These ViewModels only provide a :ts:`KnockoutObservable` field for each property on the C# class.

    see :ref:`KoExternalViewModel` for more details.


View Controllers
................

For each of your :ref:`EntityModels` and :ref:`CustomDTOs`, a controller is created in the ``/Controllers/Generated`` directory of your web project. These controllers provide routes for the generated admin views.

As you add your own pages to your application, you should add additional partial classes in the ``/Controllers`` that extend these generated partial classes to expose those pages.


Admin Views
...........

For each of your :ref:`EntityModels` and :ref:`CustomDTOs`, a number of views are generated to provide administrative-level access to your data.

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

