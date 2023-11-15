# Coalesce  

[Documentation](https://intellitect.github.io/Coalesce) &middot; [Get Started](#Get-Started) &middot; [Builds](#Builds)

Coalesce is a framework for rapid-development of ASP.NET Core + Vue.js web applications. It works from the Entity Framework Core data model that you design, automating the creation of the glue - DTOs, API Controllers, and TypeScript - that sit between your data and the UI of your application. 

## Fundamentals

* **Code Generation**: Write your data model. We'll build the DTOs, API controllers, and TypeScript files that are needed to start building a front-end UI right away. A full suite of CRUD endpoints are generated with inner-workings that are overridable.
* **Extensibility**: We don't want to box you in - one of the primary goals of Coalesce is to be as flexible as possible. If something that Coalesce is doing doesn't quite fit your needs, you can configure it or override it easily. You should never feel like you need to eject from the framework.
* **Security**: Coalesce is designed with security in mind. All classes, properties, and methods can be restricted per-role, or be completely excluded from the generated APIs. Row-level security can be implemented using custom data sources. The guiding principal here is that it should always be easy to know exactly what parts of your data Coalesce is exposing for you. [Read more about security in the Coalesce Documentation](https://intellitect.github.io/Coalesce/topics/security).

## Features

* **CRUD for EF models**: For each `DbSet<>` on your `DbContext`, a full set of `/get`, `/save`, `/delete`, `/count`, and `/list` endpoints are created. Each can be secured or omitted entirely via attributes, or customized with much greater granularity with custom [Data Sources](https://intellitect.github.io/Coalesce/modeling/model-components/data-sources/) and/or [Behaviors](https://intellitect.github.io/Coalesce/modeling/model-components/behaviors). `/list` and `/count` have paging, searching, sorting, and filtering built-in.
* **Endpoints for Methods**: For functionality that doesn't fit the mold of a CRUD action against an entity, place [methods on your entities](https://intellitect.github.io/Coalesce/modeling/model-components/methods/) and annotate with `[Coalesce]` - controllers and TypeScript will be built for those as well. Have functions that don't belong on an entity class? Place them in a [service class](https://intellitect.github.io/Coalesce/modeling/model-types/services/) marked with `[Coalesce]` instead - you'll get the same set of features.
* **Modern SPA Tooling**: Coalesce provides a seamless development experience between your ASP.NET Core server and your front-end Vue app using [Vite](https://vitejs.dev/). Leverage Vite's rich plugin ecosystem with hot module replacement, and launch your entire application &hyphen; frontend and backend &hyphen; with a single `dotnet run` or Visual Studio "F5". (For legacy Coalesce apps, Knockout.js is still supported as an alternative to Vue, but is no longer under active development and has limited feature support.)
* **... and more!** Check out the documentation to see all of Coalesce's features in-depth: https://intellitect.github.io/Coalesce

## Get Started

The best way to get started with Coalesce is using the `dotnet new` template:

* [Vue](https://github.com/IntelliTect/Coalesce.Vue.Template): `dotnet new install IntelliTect.Coalesce.Vue.Template; dotnet new coalescevue`

This command will use the current folder name for the project names, namespaces, etc. It will create a solution in the current folder with .Data and .Web projects.

After you create your project, you should start reading through [the Documentation](https://intellitect.github.io/Coalesce) to see all the things that Coalesce can do.

## Builds
|channel|build|IntelliTect.Coalesce|coalesce-vue
|:--:|:--:|:--:|:--:
|dev|[![GitHub Workflow Status (branch)](https://img.shields.io/github/actions/workflow/status/IntelliTect/Coalesce/alpha.yml?branch=dev&label=Build%20Status&logo=github)](https://github.com/IntelliTect/Coalesce/actions/workflows/alpha.yml)|[![NuGet](https://img.shields.io/static/v1?color=blue&label=nuget&logo=nuget&message=alpha%20prereleases)](https://www.nuget.org/packages/IntelliTect.Coalesce)|[![npm](https://img.shields.io/npm/v/coalesce-vue/latest.svg)](https://www.npmjs.com/package/coalesce-vue)
|master|[![GitHub Workflow Status (branch)](https://img.shields.io/github/actions/workflow/status/IntelliTect/Coalesce/release.yml?label=Build%20Status&logo=github)](https://github.com/IntelliTect/Coalesce/actions/workflows/release.yml)|[![NuGet](https://img.shields.io/nuget/v/IntelliTect.Coalesce.svg?label=nuget&logo=nuget)](https://www.nuget.org/packages/IntelliTect.Coalesce)


## Support
Full support for Coalesce is available. Please contact us for more information at info@intellitect.com.


## Contributing

See the contribution guide [here](CONTRIBUTING.md).
