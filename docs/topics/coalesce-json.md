---
title: "Config: Code Gen"
---

# Code Generation Configuration

In Coalesce, all configuration of the code generation is done in a JSON file. This file is typically named `coalesce.json` and is typically placed in the solution root.

## File Resolution

When the code generation is run by invoking `dotnet coalesce`, Coalesce will try to find a configuration file via the following means:

1. If an argument is specified on the command line, it will be used as the location of the file. E.g. `dotnet coalesce C:/Projects/MyProject/config.json`
2. If no argument is given, Coalesce will try to use a file in the working directory named `coalesce.json`
3. If no file is found in the working directory, Coalesce will crawl up the directory tree from the working directory until a file named `coalesce.json` is found. If such a file is never found, an error will be thrown.

## Contents

A full example of a `coalesce.json` file, along with an explanation of each property, is as follows:

```js
{
    "webProject": {
        // Required: Path to the csproj of the web project. Path is relative to location of this coalesce.json file.
        "projectFile": "src/Coalesce.Web/Coalesce.Web.csproj",

        // Optional: Framework to use when evaluating & building dependencies.
        // Not needed if your project only specifies a single framework - only required for multi-targeting projects.
        "framework": "net8.0",

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
        "framework": "net8.0",

        // Optional: Build configuration to use when evaluating & building dependencies.
        // Defaults to "Release".
        "configuration": "Debug",
    },

    // The name of the root generator to use.
    // The only current available value is "Vue" (default).
    "rootGenerator": "Vue",

    // If set, specifies a list of whitelisted root type names that will restrict
    // which types Coalesce will use for code generation.
    // Root types are those that must be annotated with [Coalesce].
    // Useful if want to segment a single data project into multiple web projects,
    // or into different areas/directories within a single web project.
    "rootTypesWhitelist": [
        "MyDbContext", "MyCustomDto"
    ],

    "generatorConfig": {
        // A set of objects keyed by generator name.
        // Generator names may optionally be qualified by their full namespace.
        // All generators are listed when running 'dotnet coalesce' with '--verbosity debug'.
        // For example, "Controllers" or "IntelliTect.Coalesce.CodeGeneration.Vue.Generators.Controllers".
        "GeneratorName": {
            // Optional: true if the generator should be disabled.
            "disabled": true,
            // Optional: Configures a path relative to the default output path for the generator
            // where that generator's output should be placed instead.
            "targetDirectory": "../DifferentFolder"
            // Optional: Indentation size
            "indentationSize": 2
        },
    }
}
```
