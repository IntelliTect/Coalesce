# Code Generation Overview

Coalesce's principal purpose is a code generation framework for automating the creation of the boring-but-necessary parts of a web application. Below, you find an overview of the different components of Coalesce's code generation features.


## Running Code Generation

Coalesce's code generation is run via a dotnet CLI tool, ``dotnet coalesce``. In order to invoke this tool, you must have the appropriate references to the package that provides it in your .csproj file:

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

### AI-triggered Generation

Code generation can also be triggered through the [MCP server](/topics/mcp-server.md), which enables AI assistants to run code generation as part of development workflows. This helps prevent AI assistants from getting confused by exactly which command and which directory to run code generation from.

## Generated Code

When you run `dotnet coalesce`, Coalesce will generate a full vertical stack of code for you:

### Backend C#

#### API Controllers
For each of your [CRUD Models](/modeling/model-types/crud.md) and [Services](/modeling/model-types/services.md), an API controller is created in the ``/Api/Generated`` directory of your web project. These controllers provide a number of endpoints for interacting with your data.

These controllers can be secured at a high level using [Security Attributes](/modeling/model-components/attributes/security-attribute.md), or for more granularity and customization, with [Data Sources](/modeling/model-components/data-sources.md) and [Behaviors](/modeling/model-components/behaviors.md).

#### C# DTOs
For each of your [Entity Models](/modeling/model-types/entities.md) and [Standalone Entities](/modeling/model-types/standalone-entities.md), a C# DTO class is created. These classes are used to hold the data that will be serialized and sent to the client, as well as data that has been received from the client before it has been mapped back to your EF POCO class. These classes enable property-level security trimming, sparse updates, accurate OpenAPI definitions, and more.

See [Generated C# DTOs](/stacks/agnostic/dtos.md) for more information.

#### Semantic Kernel Plugins

<Beta/> 

For models, methods, and data sources annotated with the [[SemanticKernel]](/modeling/model-components/attributes/semantic-kernel.md) attribute, plugin classes are generated in the `/KernelPlugins/Generated` directory. These plugins expose your application's functionality as AI-callable functions that can be used with Microsoft Semantic Kernel for building AI-powered features.

### Frontend - Vue

For your frontend Vue application, Coalesce generates TypeScript models, API clients, and feature-rich ViewModels that provide complete type safety and seamless integration with your backend. This includes reactive data models, CRUD operations, and validation that stay in sync with your C# models.

A more in-depth look at the Vue generated code can be found at [Vue Overview](/stacks/vue/overview.md).
