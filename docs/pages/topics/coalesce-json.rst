

.. _CoalesceJson:

Code Generation Configuration
=============================


In Coalesce, all configuration of the code generation is done in a JSON file. This file is typically named ``coalesce.json`` and is typically placed in the solution root.



File Resolution
---------------

When the code generation is ran by invoking ``dotnet coalesce``, Coalesce will try to find a configuration file via the following means:

#. If a parameter is specified on the command line, it will be used as the location of the file. E.g. ``dotnet coalesce C:/Projects/MyProject/config.json``
#. If no parameter is given, Coalesce will try to use a file in the working directory named ``coalesce.json``
#. If no file is found in the working directory, Coalesce will crawl up the directory tree from the working directory until a file named ``coalesce.json`` is found. If such a file is never found, an error will be thrown.


Contents
--------

A full example of a ``coalesce.json`` file, along with an explanation of each property, is as follows:


.. code-block:: javascript

    {
        "webProject": {
            // Required: Path to the csproj of the web project. Path is relative to location of this coalesce.json file.
            "projectFile": "src/Coalesce.Web/Coalesce.Web.csproj",

            // Optional: Framework to use when evaluating & building dependencies.
            // Not needed if your project only specifies a single framework - only required for multi-targeting projects.
            "framework": "netcoreapp2.0",

            // Optional: Build configuration to use when evaluating & building dependencies.
            // Defaults to "Debug".
            "configuration": "Debug",

            // Optional: Override the namespace prefix for generated C# code.
            // Defaults to MSBuild's `$(RootNamespace)` for the project.
            "rootNamespace": "MyCompany.Coalesce.Web",
        },

        "dataProject": {
            // Required: Path to the csproj of the data project. Path is relative to location of this coalesce.json file.
            "projectFile": "src/Coalesce.Domain/Coalesce.Domain.csproj",

            // Optional: Framework to use when evaluating & building dependencies.
            // Not needed if your project only specifies a single framework - only required for multi-targeting projects.
            "framework": "netstandard2.0",

            // Optional: Build configuration to use when evaluating & building dependencies.
            // Defaults to "Release".
            "configuration": "Debug",
        }
    }


Additional CLI Options
----------------------

There are a couple of extra options which are only available as CLI parameters to ``dotnet coalesce``. These options do not affect the behavior of the code generation - only the behavior of the CLI itself.

    ``--debug``
        When this flag is specified when running ``dotnet coalesce``, Coalesce will wait up to 60 seconds for a debugger to be attached to its process before starting code generation.

    ``-v|--verbosity <level>``
        Set the verbosity of the output. Options are ``trace``, ``debug``, ``information``, ``warning``, ``error``, ``critical``, and ``none``.