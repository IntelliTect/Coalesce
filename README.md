# Coalesce  

[Home](http://coalesce.intellitect.com/) &middot; [Documentation](https://intellitect.github.io/Coalesce) &middot; [Get Started](#Get-Started) &middot; [Builds](#Builds)

Coalesce is a framework for rapid-development of ASP.NET Core web applications. It works from the Entity Framework Core data model that you design, automating the creation of the glue - DTOs, API Controllers, and TypeScript - that sit between your data and the UI of your application. 

## Fundamentals

* **Code Generation**: Write your data model. We'll build the DTOs, API controllers, and TypeScript files that are needed to start building a front-end UI right away. A full suite of CRUD endpoints are generated with inner-workings that are overridable.
* **Extensibility**: We don't want to box you in - one of the primary goals of Coalesce is to be as flexible as possible. If something that Coalesce is doing doesn't quite fit your needs, you should be able to configure it or override it easily. You should never feel like you need to eject from the framework.
* **Security**: Coalesce is designed with security in mind. All classes, properties, and methods can be restricted per-role, or their data even hidden entirely from the client. Row-level security can be implemented using custom data sources. The guiding principal here is that it should always be easy to know exactly what parts of your data Coalesce is exposing for you.

## Features

* **CRUD for EF models**: For each `DbSet<>` on your `DbContext`, a full set of `/get`, `/save`, `/delete`, `/count`, and `/list` endpoints are created. Each can be secured or omitted entirely via attributes, or customized with much greater granularity with custom [Data Sources](https://intellitect.github.io/Coalesce/modeling/model-components/data-sources/) and/or [Behaviors](https://intellitect.github.io/Coalesce/modeling/model-components/behaviors). `/list` and `/count` have paging, searching, sorting, and filtering built-in.
* **Endpoints for Methods**: For client requests that don't fit the mold of a simple CRUD action against an entity, place [methods on your entities](https://intellitect.github.io/Coalesce/modeling/model-components/methods/) and annotate with `[Coalesce]` - controllers and TypeScript will be built for those as well. Have functions that don't belong on an entity class? Place them in a [service class](https://intellitect.github.io/Coalesce/modeling/model-types/services/) marked with `[Coalesce]` instead - you'll get the same set of features.
* **Choose your Front-end**: Coalesce supports both Vue and Knockout. While the paradigms used in the TypeScript code generated for each are quite different, they both make requests against the same API. At the end of the day, they both can accomplish the same tasks. The Vue stack is the recommended stack when creating new applications.
* **... and more!** Check out the documentation to see all of Coalesce's features in-depth: https://intellitect.github.io/Coalesce

## Get Started

The best way to get started with Coalesce is using the `dotnet new` templates that have been created:

* [Vue](https://github.com/IntelliTect/Coalesce.Vue.Template): `dotnet new --install IntelliTect.Coalesce.Vue.Template; dotnet new coalescevue`
* [Knockout](https://github.com/IntelliTect/Coalesce.KnockoutJS.Template): `dotnet new --install IntelliTect.Coalesce.KnockoutJS.Template; dotnet new coalesceko`

After you create your project, you should start reading through [the Documentation](https://intellitect.github.io/Coalesce) to see all the things that Coalesce can do.

## Builds
|channel|build|IntelliTect.Coalesce|coalesce-vue
|:--:|:--:|:--:|:--:
|master|[![Build status](https://intellitect.visualstudio.com/Coalesce/_apis/build/status/Production)](https://intellitect.visualstudio.com/Coalesce/_build/latest?definitionId=64)|[![NuGet](https://img.shields.io/nuget/v/IntelliTect.Coalesce.svg)](https://www.nuget.org/packages/IntelliTect.Coalesce)
|dev|[![Build Status](https://intellitect.visualstudio.com/Coalesce/_apis/build/status/Alpha)](https://intellitect.visualstudio.com/Coalesce/_build/latest?definitionId=62)|[![MyGet](https://img.shields.io/myget/intellitect-coalesce/v/IntelliTect.Coalesce.svg?label=myget)](https://www.myget.org/feed/intellitect-coalesce/package/nuget/IntelliTect.Coalesce)|[![npm](https://img.shields.io/npm/v/coalesce-vue/dev.svg)](https://www.npmjs.com/package/coalesce-vue)


## Contributing

See the contribution guide [here](CONTRIBUTING.md).
