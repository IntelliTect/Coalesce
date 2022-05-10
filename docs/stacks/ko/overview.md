
.. _KoOverview:

Knockout Overview
=================

The [Knockout](http://knockoutjs.com/) stack for Coalesce offers the ability to build pages with the time-tested [Knockout](http://knockoutjs.com/) JavaScript library using all of the features of the Coalesce generated APIs and [ViewModels](/stacks/disambiguation/view-model.md). It can be used for anything between adding simple interactive augmentations of MVC pages to building a full MPA-SPA hybrid application.

[[toc]]

Getting Started
---------------

Check out [Getting Started with Knockout](/stacks/ko/getting-started.md) if you haven't already to learn how to get a new Coalesce Knockout project up and running.

Generated Code
--------------

Below you will find a brief overview of each of the different pieces of code that Coalesce will generate for you when you choose the Knockout stack.

TypeScript
..........

Coalesce generates a number of different types of TypeScript classes to support your data through the generated API.


[ViewModels](/stacks/ko/client/view-model.md)
    One view model class is generated for each of your [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md). These models contain fields for your model [Properties](/modeling/model-components/properties.md), and functions and other members for your model [Methods](/modeling/model-components/methods.md). They also contain a number of standard fields & functions inherited from `BaseViewModel` which offer basic loading & saving functionality, as well as other handy utility members for use with Knockout.

    See [TypeScript ViewModels](/stacks/ko/client/view-model.md) for more details.

[List ViewModels](/stacks/ko/client/list-view-model.md)
    One ListViewModel is generated for each of your [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md). These classes contain functionality for loading sets of objects from the server. They provide searching, paging, sorting, and filtering functionality.

    See [TypeScript ListViewModels](/stacks/ko/client/list-view-model.md) for more details.

[External Type ViewModels](/stacks/ko/client/external-view-model.md)
    Any non-primitive types which are not themselves a [Entity Models](/modeling/model-types/entities.md) or [Custom DTOs](/modeling/model-types/dtos.md) which are accessible through the aforementioned types, either through one of its [Properties](/modeling/model-components/properties.md), or return value from one of its [Methods](/modeling/model-components/methods.md), will have a corresponding TypeScript ViewModel generated for it. These ViewModels only provide a `KnockoutObservable` field for each property on the C# class.

    see [TypeScript External ViewModels](/stacks/ko/client/external-view-model.md) for more details.


View Controllers
................

For each of your [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md), a controller is created in the ``/Controllers/Generated`` directory of your web project. These controllers provide routes for the generated admin views.

As you add your own pages to your application, you should add additional partial classes in the ``/Controllers`` that extend these generated partial classes to expose those pages.


Admin Views
...........

For each of your [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md), a number of views are generated to provide administrative-level access to your data.

Table
    Provides a basic table view with sorting, searching, and paging of your data.

TableEdit
    Provides the table view, but with inline editing in the table.

Cards
    Provides a card-based view of your data with searching and paging.

CreateEdit
    Provides an editor view which can be used to create new entities or edit existing ones.

EditorHtml
    Provides a minimal amount of HTML to display an editor for the object type. This is used by the `showEditor` method on the generated TypeScript ViewModels.

