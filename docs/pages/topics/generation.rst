Code Generation
===============

The primary function of Coalesce is as a code generation framework. Below, you find an overview of the different components of Coalesce's code generation features.

.. contents:: Contents
    :local:


Running Code Generation
-----------------------

Coalesce's code generation is ran via a dotnet CLI tool, ``dotnet coalesce``. In order to invoke this tool, you must have the appropriate references to the package that provides it in your .csproj file:

    .. code-block:: xml

        <Project Sdk="Microsoft.NET.Sdk.Web">

            ...

            <ItemGroup>
                <PackageReference Include="IntelliTect.Coalesce.Knockout" Version="..." />
            </ItemGroup>

            <ItemGroup>
                <DotNetCliToolReference Include="IntelliTect.Coalesce.Tools" Version="..." />
            </ItemGroup>  
        </Project>

CLI Options
...........

All configuration of the way that Coalesce interacts with your projects, including locating, analyzing, and producing generated code, is done in a json configuration file, ``coalesce.json``. Read more about this file at :ref:`CoalesceJson`.

There are a couple of extra options which are only available as CLI parameters to ``dotnet coalesce``. These options do not affect the behavior of the code generation - only the behavior of the CLI itself.

    ``--debug``
        When this flag is specified when running ``dotnet coalesce``, Coalesce will wait for a debugger to be attached to its process before starting code generation.

    ``-v|--verbosity <level>``
        Set the verbosity of the output. Options are ``trace``, ``debug``, ``information``, ``warning``, ``error``, ``critical``, and ``none``.

Generated Code
--------------

Below you will find a brief overview of each of the different pieces of code that Coalesce will generate for you.


TypeScript
..........

Coalesce generates a number of different types of TypeScript classes to support your data through the generated API.

ViewModels
    One view model class is generated for each of your EF Database-mapped POCO classes. These models contain fields for your model :ref:`ModelProperties`, and functions and other members for your model :ref:`ModelMethods`. They also contain a number of standard fields & functions inherited from :ts:`BaseViewModel` which offer basic loading & saving functionality, as well as other handy utility members for use with Knockout.

    See :ref:`TypeScriptViewModel` for more details.

List ViewModels
    One ListViewModel is generated for each of your EF Database-mapped POCO classes. These classes contain functionality for loading sets of objects from the server. They provide searching, paging, sorting, and filtering functionality.

    See :ref:`TypeScriptListViewModel` for more details.

External Type ViewModels
    Any types which are accessible through your Database-mapped POCO classes, either through one of its getter-only :ref:`ModelProperties` or return value from one of its :ref:`ModelMethods`, will have a corresponding TypeScript ViewModel generated for it. These ViewModels only provide a :ts:`KnockoutObservable` field for each property on the C# class.

    see :ref:`TypeScriptExternalViewModel` for more details.


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

