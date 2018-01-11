## General Breaking Changes:

| CHANGE | RESOLUTION
| ------ |----------|
| NPM Packages: Upgraded to `gulp-typescript` 3.0 and `typescript` 2.3. Breaking changes may be found at http://dev.ivogabe.com/gulp-typescript-3/. | Add/update references to `"gulp-typescript": "^3.1.6"` and `"typescript": "~2.3.2"` in your package.json. Newer versions are fine, too. Update your `gulpfile.js` according to the breaking changes at http://dev.ivogabe.com/gulp-typescript-3/.
| TypeScript Global variable `baseUrl` is gone. | Replaced by `Coalesce.GlobalConfiguration.baseApiUrl` and `baseViewUrl`.
| TypeScript global variable `saveTimeoutInMs` is gone. | Replaced by `Coalesce.GlobalConfiguration.saveTimeoutMs`, and the corresponding configuration in class-level and instance-level `CoalesceConfiguration<>` objects.
| Various knockout bindings that would interpret a `saveImmediately` supplementary binding no longer do so. | The `saveImmediately` standalone binding still exists, but will only function if the currently scoped binding object is a Coalesce `BaseViewModel`. This is because the binding must access the model's `coalesceConfig` to set the save timeout, since `saveTimeoutInMs` is no longer a global.
| Knockout binding `select2AjaxText`: no longer takes `object` as a complimentary binding in order to compute the url | Now takes `url` instead to be consistent with other select2 bindings.
| `Scripts/Coalesce/intellitect.*` files are now named `Scripts/Coalesce/coalesce.*` , and now have a singular reference point: `Scripts/coalesce.dependencies.d.ts`. This file is only generated if it is missing - it can be modified at your desire. | Any `<reference ... />` elements in your typescript files that referenced the core coalesce TypeScript files should instead reference Scripts/coalesce.dependencies.d.ts. 
| Namespace `intellitect.utilities` is now `Coalesce.Utilities` | Replace all references to the old name with the new.
| Namespace `intellitect.webApi` is now `Coalesce.ModalHelpers` | Replace all references to the old name with the new.
| Generated API Controllers no longer have "Api" in the file name in order to better match the name of the class within. | No changes needed unless your project referenced these files explicitly by name (unlikely).
| `BaseViewController.IndexImplementation` no longer sets `ViewBag.ParentIdName` or `ViewBag.ParentId`. These properties were not used. | If you used these, implement your own custom, robust logic in your controllers that depend upon `IndexImplementation`.
| The `datePicker` knockout binding no longer requires a `delaySave` supplementary binding in order to prevent spamming saves as a user changes values - this behavior is now default, and can be turned off by adding binding `updateImmediate: true`. | Remove usages of `delaySave` used with `datePicker` bindings if desired. If you used any datePickers where `delaySave` was not desired, add `updateImmediate: true`.
| The `select2Ajax` knockout binding no longer violates type contracts set forth by the generated ViewModels when `setObject: true` is used. Instead, when `setObject` is used, the `object` observable will be updated with a proper instance of a `Coalesce.BaseViewModel`. | Update `select2Ajax` bindings which used `setObject: true` to also include another new binding `itemViewModel` which should be set to the class that is being selected. Any dropdowns that need this fix will throw an exception without it. Additionally, update any code that expects that the objects referenced by the `object` binding of `select2Ajax` to be raw JS objects - these observables will now always be populated with instances of a `Coalesce.BaseViewModel`.
| The `select2Ajax` binding no longer specifies only specific fields in its API request when `setObject: true` is used. | It is strongly discourage that `setObject: true` be used without requesting whole objects form the server. However, you can still add `?fields=field1,field2...` to the queryString of the `url` specified with a `select2Ajax` binding to retain the old behavior.
----

## BaseViewModel & Generated ViewModels:

| CHANGE | RESOLUTION
| ------ |----------|
| `BaseViewModel` is no longer a generic type `BaseViewModel<T>`. It now uses TypeScripts polymorphic `this` types for its self-referential needs | Remove any usages of the generic parameter. Replace with [Polymorphic this types](https://www.typescriptlang.org/docs/handbook/advanced-types.html) as needed.
| `<ModelName>ListUrl` properties on generated ViewModels are now correctly camelCased. | Rename references from `MyModelListUrl` to `myModelListUrl`.
| `deleteCallbacks` have been removed. | When deleting a model, pass a callback to `deleteItem` or `deleteItemWithConfirmation`.
| `customField1`, `2`, & `3` have been removed. | Annotate your C# models with `[TypeScriptPartial]`, and create any custom fields on the stub that is generated. Alternatively, place any view-specific, object-specific logic in the code that drives a particular view.
| `BaseViewModel.isDataFromSaveLoaded` has been changed to `BaseViewModel.coalesceConfig.loadResponseFromSaves` | Find and replace usages of the old property.
| `BaseViewModel.afterLoadFromDto` has been changed to `BaseViewModel.coalesceConfig.onLoadFromDto` | Find and replace usages of the old property.
| `loadValidValues()` is gone, as are the other model-specific methods and collections of a similar nature. | Instantiate a `ListViewModel` of the desired object type and use it to load lists of items.
| `isSavingWithChildren: boolean` is now named `isThisOrChildSaving` | Simple find-and-replace.
| `reload()` has been removed. | Replace usages of `reload()` with no-argument calls to `load()`. Be wary of places where `reload()` was passed directly as a callback - the replacement call will need to be wrapped in another function in such cases: `() => load()`.
| `changeIsExpanded(isExpanded)` is now named `toggleIsExpanded`, and no longer takes a parameter. | If a parameter was used, set the `isExpanded` observable directly. Otherwise, replace the old name with the new.
| `changeIsEditing(isEditing)` is now named `toggleIsEditing`, and no longer takes a parameter. | If a parameter was used, set the `isEditing` observable directly. Otherwise, replace the old name with the new.
| `isSelectedToggle()` is now named `toggleIsSelected`. | Simple find-and-replace.
| `originalData: KnockoutObservable<any>` has been removed. | Create a `[TypeScriptPartial]`, create this field, and populate it in your constructor with the value of the first constructor parameter of the stub that will be provided upon regeneration.
| `public init()` has been removed from `BaseViewModel`. | Create a `[TypeScriptPartial]` and add desired behavior into the constructor of the stub that will be provided upon regeneration.
| `BaseViewModel.autoSaveEnabled` is deprecated. | Use `BaseViewModel.coalesceConfig.autoSaveEnabled` observable instead.
| `BaseViewModel.showBusyWhenSaving` is deprecated. | Use `BaseViewModel.coalesceConfig.showBusyWhenSaving` observable instead.
| `BaseViewModel.showFailureAlerts` is deprecated. | Use `BaseViewModel.coalesceConfig.showFailureAlerts` observable instead.
| `BaseViewModel.validationIssues` has been removed. | Use `BaseViewModel.message` to get errors that occurred while saving a model. No other methods other than `save` were populating this collection, and it was only being populated with exception messages - not validation issues.

## API Endpoints

| CHANGE | RESOLUTION
| ------ |----------|
| API Endpoint `CustomList` has been merged with `List`. Parameters remain unchanged, except where otherwise noted in this section. | Replace all usages of `CustomList` with `List`.
| Property filter API parameters are now specified using `?filter.propertyName=value` (formerly, this was just `?propertyName=value`). | Adjust manual API calls (that don't use the generated ListViewModels) accordingly.
| The `PropertyValues` endpoint has been removed, as it was deemed too specific a use case to warrant the need to accommodate its existence into your security model for each and every entity type. | Replace usages with a custom static method on the relevant model that will return an `ICollection<string>` with the desired results.
| API endpoints that formerly took an arbitrary `where` parameter (stored in `BaseListViewModel.query.where`) no longer do so. This was removed because the security implications were such that a carefully-crafted where statement could reveal information about the state of the database that would be otherwise inaccessible to the user. | In cases where this behavior is needed, create a custom data source (see the "New Features" section below) and add public properties marked with `[Coalesce]` that represent the needed inputs to perform the query in C#.
| Endpoint `Get` now returns an `ItemResult<TDto>` instead of a raw `TDto`.
| Endpoint `Delete` now returns an `ItemResult` instead of a `bool`.

## List API, BaseListViewModel & Generated ListViewModels

| CHANGE | RESOLUTION
| ------ |----------|
| Like `BaseViewModel`, `BaseListViewModel` no longer has a self-referential generic type parameter. It now uses TypeScripts polymorphic `this` types for its self-referential needs | Remove any usages of the generic parameter. Replace with [Polymorphic this types](https://www.typescriptlang.org/docs/handbook/advanced-types.html) as needed.

| `BaseListViewModel.query` is now `BaseListViewModel.filter` to match the new API signature. | Rename references accordingly.



## Projects, Namespaces, & Generation

| CHANGE | RESOLUTION
| ------ |----------|
| Knockout code generation is now contained in IntelliTect.Coalesce.CodeGeneration.Knockout | If using Coalesce as a submodule, optionally add this project to your solution.
| Knockout helpers are now contained in project `IntelliTect.Coalesce.Knockout` | In all cases, replace project references to `IntelliTect.Coalesce` with `IntelliTect.Coalesce.Knockout`. If using Coalesce as a submodule, optionally add this project to your solution.
| Namespaces for `Display` and `Knockout` classes have changed from `IntelliTect.Coalesce.Helpers` to `IntelliTect.Coalesce.Knockout.Helpers`. | Update using statements accordingly.
| The `Coalesce` directory created in the root of a generated web project is no longer used nor needed. A new way to customize/override templates will be introduced at a later date. | Remove this folder. 
| Coalesce no longer generates any project configuration files that are not found when it generates. This includes: `tsd.json`, `gulpfile.js`, `bower.json`, `package.json`, `site.scss`, `tsconfig.json` | When creating a new project with Coalesce, use the [Starter Project](https://github.com/IntelliTect/Coalesce.Starter/). A `dotnet new` template will be introduced at a later date.
| Many `.cshtml` view files are no longer generated by Coalesce: `Shared/_AdminLayout.cshtml`, `Shared/_Master.cshtml`, `Shared/_Layout.cshtml`, `_ViewStart.cshtml`, `_ViewImports.cshtml` | When creating a new project with Coalesce, use the [Starter Project](https://github.com/IntelliTect/Coalesce.Starter/). A `dotnet new` template will be introduced at a later date.


## Configuration & Discovery
| CHANGE | RESOLUTION
| ------ |-----------|
| Your `DbContext` that Coalesce will generate from is now discovered by the presence of a `[Coalesce]` attribute on its class definition. | Place `[Coalesce]` on your `DbContext` that Coalesce should generate from.
| `IEnumerable<IClassDto<>>` properties on your `DbContext` are no longer used to discover custom `IClassDto<>` implementations that should be generated | Place a `[Coalesce]` attribute on any `IClassDto<>` classes you wish to generate from. Remove the property on the `DbContext`.
| Added extension methods for `IServiceProvider`: `services.AddCoalesce()`. This replaces the need for the (formerly) required `ReflectionRepository.AddContext` call in Startup.cs. | Replace call to `ReflectionRepository.AddContext` in `Configure` or `ConfigureServices` with `services.AddCoalesce()` in `ConfigureServices` in your `Startup.cs`.
| The default timezone used by Coalesce when interpreting dates without timezone information is no longer "Pacific Standard Time" - it is now `TimeZoneInfo.Local`. | Use one of the `UseTimeZone` overloads in `services.AddCoalesce(b => b.UseTimeZone)` to override this behavior. Use `b.UseTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"))` will restore the old behavior.


## Modeling
| CHANGE | RESOLUTION
| ------ |-----------|
| Custom DataSources have been completely overhauled. `public static IQueryable<T>` methods on your models are no longer considered data sources. | See the "New Features" section below.
| The `IIncludable<>` interface has been removed. | Move desired behavior into a custom data source. Override the default data source if needed. 
| The `IIncludeExternal<>` interface has been removed. | Override the `TransformResults` method of the `StandardDataSource<,>` in a custom data source to attach items from an external location.
| The `IExcludable` interface has been removed. | Move desired behavior into a custom data source. Override the `TransformResults` method in the data source if needed. 
| The `IBeforeSave<,>`, `IAfterSave<,>`, `ISaveCustomizable<,>`, `IBeforeDelete<,>`, `IAfterDelete<>`, and `IDeleteCustomizable<,>` have been removed. | Override the corresponding methods of the `StandardBehaviors<,>` derivative that you create for the type that formerly used these interfaces. See the "New Features" section below.
| `ListGroupAttribute` was removed as a consequence of the removal of the `PropertyValues` endpoint. | Remove usages of this attribute. Implement logic for sourcing values from multiple fields in the replacement methods written to replace `PropertyValues`.
| `Fingerprintable<,>` and `IFingerprintable<,>` have been removed. | To implement this functionality in a general way, derive a class from `StandardBehaviors<,>` and register your derived class as the default behaviors: `services.AddCoalesce(b => b.AddContext<AppDbContext>().UseDefaultBehaviors(typeof(MyBehaviors<,>)));`, and implement the fingerprint logic in your custom behavior class (probably in `BeforeSave`).
| Client-exposed methods on your models must be explicitly exposed by adding the `[Coalesce]` attribute. | Add `[Coalesce]` to methods you wish to expose to the client. Remove `[InternalUse]` from those you don't wish to expose - it is no longer needed for methods.
| The `[FileDownload]` attribute was removed, as it served no purpose. At best, it was misleading that Coalesce would generate file download API endpoints, which it does not. | Remove usages of this attribute. Replace with `[InternalUse]` for properties that shouldn't be sent in normal API responses.

## New Features:

* Modular DataSources
  * Custom DataSources are now implemented as classes that you define. These classes should inherit from `IDataSource<T>`, where `<T>` is the type of the entity served - the same `<T>` in the old `static IQueryable<T>` methods. 
  * However, you will almost certainly want to inherit from `StandardDataSource<T, TContext>` to maintain the standard set of Coalesce behaviors. Override the `GetQuery` method of `StandardDataSource<,>` and replace it with the former contents of your `static IQueryable<T>` method. 
  * To globally override the standardDataSource, register a custom implementation of `StandardDataSource<T, TContext>` with your applications `IServiceCollection` at startup.
  * Annotate your DataSource with `[Coalesce]` for it to be discovered, or place it as a nested class of a model that belongs to a `DbContext` that is annotated with `[Coalesce]`.
  * To prevent `StandardDataSource<T, TContext>` from being made available for a given type `T`, annotate a custom data source that serves the same `T` with `[DefaultDataSource]`.
  * DataSources may contain primitive (strings, numerics, bools, dates, enums) properties annotated with `[Coalesce]`. These properties will be made available in the generated TypeScript and will be automatically populated on the server with the values specified on the client.
  * In TypeScript, the enum that used to contain all the possible datasources is now a namespace that contains all the possible datasource classes.

* Modular behaviors
  * The `I(Before/After)(Save/Delete)` interfaces have been removed in favor of placing all such logic into a dedicated class. These classes should inherit from `IBehaviors<T>`, where `<T>` is the type of entity being controlled.
  * `StandardBehaviors<T, TContext>` contains all of the logic used for saving and deleting items, and can have any of its methods overridden to change this behavior, including complete replacement of the standard save & delete behaviors.
  * To globally override the standard behaviors, register a custom implementation of `StandardBehaviors<T, TContext>` with your applications `IServiceCollection` at startup.

* Hierarchical configuration system:
  * Many points of configuration for ViewModels and ListViewModels are done via a hierarchical configuration system.
  * All configuration properties are observables. Setters will set the property at that level, causing it cascade down until it reaches an instance, or until it is overridden at a lower level.
  * Setting a configuration property to null will reset it back to inheriting from an ancestor. Setting the global defaults to null will probably cause errors.
  * Configuration defaults can be found around line 68 of coalesce.ko.base.ts.
  * A global object, Coalesce.GlobalConfiguration, has most of the default configuration.
  * Two more global configuration objects exist as properties on Coalesce.GlobalConfiguration. These are `.viewModel` and `.listViewModel`, and they control the defaults for ViewModels and ListViewModels, respectively. There are some properties on these that are specific to one type or the other that don't appear on Coalesce.GlobalConfiguration itself.
  * On each ViewModel and ListViewModel class, there is a class-level configuration object named `coalesceConfig`. For example, `ViewModels.User.coalesceConfig`. These objects control settings for all instances of their type.
  * On each ViewModel and ListViewModel instance, there is an instance-level configuration object named `coalesceConfig`. This object controls settings for each specific instance of a model.

* New attribute: [Controller]. Place this attribute on your C# model classes to customize the generation of the API controllers.

