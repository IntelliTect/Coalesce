# Upgrading Coalesce

Upgrading your project's version of Coalesce is a relatively straightforward process. The latest version of Coalesce can be viewed [on npm](https://www.npmjs.com/package/coalesce-vue?activeTab=versions) or [on NuGet](https://www.nuget.org/packages/IntelliTect.Coalesce#versions-body-tab).

You should always use the same version for both NPM and NuGet dependencies. Having mismatched versions in a single project can introduce errors or subtle bugs.

## NPM upgrades

First, upgrade the NPM dependencies in your web project.

1. Open `package.json` in your web project and replace the versions of `coalesce-vue` and `coalesce-vue-vuetify3` with the new version number.
``` json 
{
  "dependencies": {
    "coalesce-vue": "5.3.1",
    "coalesce-vue-vuetify3": "5.3.1",
  }
}
```
2. Save the changes to `package.json`.
3. Run `npm i` in your web project to install the new versions.


## NuGet upgrades

Coalesce projects usually have a single variable, `CoalesceVersion`, to control all Coalesce NuGet package dependencies, declared in `Directory.Build.props`. This file is in the root of the solution, next to the `.sln` file; it is also included in the "Solution Items" folder in Visual Studio's Solution Explorer.

To upgrade the NuGet packages:
1. Update the value of `CoalesceVersion` with the new version number.
``` xml
<Project>
  <PropertyGroup>
    <CoalesceVersion>5.3.1</CoalesceVersion>
  </PropertyGroup>
</Project>
```
2. Save the changes.
3. Run `dotnet restore` in the solution root or in the web project. Then, run code generation with `dotnet coalesce` or `npm run coalesce` in the web project.

::: tip
The above information describes the default project configuration that originates from the Coalesce project template. If your project has diverged from this configuration, consult with your project team members, or just explore/search the files in your project.
:::

::: warning
It is not recommended to use the Visual Studio NuGet Package Manager to manage Coalesce versions because it will erase usages of the central `CoalesceVersion` variable, which can lead to version mismatches within your solution. It also fails to maintain the version of the `DotNetCliToolReference`, which provides Coalesce's code generator.
:::