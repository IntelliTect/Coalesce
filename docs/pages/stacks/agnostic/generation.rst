Code Generation Overview
========================

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
            <PackageReference Include="IntelliTect.Coalesce" Version="..." />
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

Coalesce has the option of two front-end stacks - either `Knockout <http://knockoutjs.com/>`_, or `Vue <https://vuejs.org/>`_. The Vue-based stack is the current focus of all development efforts against Coalesce going forward - the Knockout stack is effectively in maintenance-only mode.

For either stack, Coalesce will generate a variety of different kinds of code for you:

Server-side C#
.............................

    API Controllers
        For each of your :ref:`EntityModels`, :ref:`CustomDTOs`, and :ref:`Services`, an API controller is created in the ``/Api/Generated`` directory of your web project. These controllers provide a number of endpoints for interacting with your data.

        These controllers can be secured at a high level using :ref:`SecurityAttributes`, and when applicable to the type, with :ref:`DataSources` and :ref:`Behaviors`.

    C# DTOs
        For each of your :ref:`EntityModels`, a C# DTO class is created. These classes are used to hold the data that will be serialized and sent to the client, as well as data that has been received from the client before it has been mapped back to your EF POCO class.

        See :ref:`GenDTOs` for more information.

Vue
...

An overview of the Vue stack can be found at :ref:`VueOverview`.

Knockout
........

An overview of the Knockout stack can be found at :ref:`KoOverview`.

