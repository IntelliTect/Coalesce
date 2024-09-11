# Code Generation Overview

Coalesce's principal purpose is a code generation framework for automating the creation of the boring-but-necessary parts of a web application. Below, you find an overview of the different components of Coalesce's code generation features.


## Running Code Generation

Coalesce's code generation is ran via a dotnet CLI tool, ``dotnet coalesce``. In order to invoke this tool, you must have the appropriate references to the package that provides it in your .csproj file:

``` xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>

    <!-- Necessary to use DotNetCliToolReference with modern framework versions -->
    <DotnetCliToolTargetFramework>net8.0</DotnetCliToolTargetFramework>
  </PropertyGroup>

  ...

  <ItemGroup>
    <PackageReference Include="IntelliTect.Coalesce" Version="..." />
  </ItemGroup>

  <ItemGroup>
    <DotNetCliToolReference Include="IntelliTect.Coalesce.Tools" Version="..." />
  </ItemGroup>  
</Project>
```

### CLI Options

All configuration of the way that Coalesce interacts with your projects, including locating, analyzing, and producing generated code, is done in a json configuration file, ``coalesce.json``. Read more about this file at [Code Generation Configuration](/topics/coalesce-json.md).

There are a couple of extra options which are only available as CLI parameters to ``dotnet coalesce``. These options do not affect the behavior of the code generation - only the behavior of the CLI itself.

- `<configFile>` - First positional parameter. Path to the `coalesce.json` configuration file. If not specified, will search upwards from the current folder for a file named `coalesce.json`.
- ``--debug`` - When this flag is specified when running ``dotnet coalesce``, Coalesce will wait for a debugger to be attached to its process before starting code generation.
- `--what-if|-WhatIf` - Runs all code generation, but does not make changes to disk.
- `--verify` - Assert that the code generation does not have any pending changes to its output. Useful in CI builds when combined with `--what-if` to ensure that developers haven't forgotten to run code gen before committing changes.
- ``-v|--verbosity <level>`` - Set the verbosity of the output. Options are ``trace``, ``debug``, ``information``, ``warning``, ``error``, ``critical``, and ``none``.

## Generated Code

Coalesce will generate a full vertical stack of code for you:

### Backend C#

#### API Controllers
For each of your [Entity Models](/modeling/model-types/entities.md), [Custom DTOs](/modeling/model-types/dtos.md), and [Services](/modeling/model-types/services.md), an API controller is created in the ``/Api/Generated`` directory of your web project. These controllers provide a number of endpoints for interacting with your data.

These controllers can be secured at a high level using [Security Attributes](/modeling/model-components/attributes/security-attribute.md), and when applicable to the type, with [Data Sources](/modeling/model-components/data-sources.md) and [Behaviors](/modeling/model-components/behaviors.md).

#### C# DTOs
For each of your [Entity Models](/modeling/model-types/entities.md), a C# DTO class is created. These classes are used to hold the data that will be serialized and sent to the client, as well as data that has been received from the client before it has been mapped back to your EF POCO class.

See [Generated C# DTOs](/stacks/agnostic/dtos.md) for more information.

### Frontend - Vue

An overview of the Vue generated code can be found at [Vue Overview](/stacks/vue/overview.md).
