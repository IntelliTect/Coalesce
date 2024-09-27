# 5.0.2

- feat: Make "not found" messages from data sources clearer when the ID is null or empty string. (#447)
- feat(template): Added multi-tenancy options to the template. (#441, #461)
- fix(template): adjust manual chunking configuration to avoid circular deps. (#455)
- fix(audit): key props now respect configured property exclusions.
- fix: c-admin-method now preserves newlines when displaying success messages.
- fix: c-select not defaulting optional method parameters as clearable.
- fix: method object parameters passed sent as null on the client are received as a default instance of the object on the server. (#456)
- fix: non-nullable value types as root custom method parameters rejected by ModelState validation. (#464)

# 5.0.1

- fix: `c-input` once again respects the validation rules defined by the bound ViewModel.
- fix: `c-select-many-to-many`, when selecting an item, will now attempt to reuse a matching, previously removed item that has not yet been committed with a $bulkSave. This prevents unnecessary delete+create of the same item in a many-to-many when the user changes their mind about the inclusion of an item.

# 5.0.0

## Breaking Changes

- Support for Knockout.js, previously deprecated, has been fully removed.
- Drop all .NET targets except .NET 8

### Breaking: Backend/C#

- Removed automatic `DbContext.SaveChanges()` call from model instance method API endpoints. Model instance methods that intend to save any changes to the database now must inject a `DbContext` and perform this save explicitly. (#405)
- ASP.NET `ModelState` validation is now always performed against all inputs to all endpoints.
- `ClaimsPrincipalExtensions` has been removed. Claim names/types can vary significantly depending on the authentication schemes used within a project, so these are best left defined on a per-project basis. Replacements are present in the new Coalesce project template.
- `IDataSource.GetItemAsync` and `IDataSource.GetListAsync` no longer return tuples. The IncludeTree return from these methods is now attached to the ItemResult/ListResult.
- `StandardBehaviors.AfterSave` is now `AfterSaveAsync` and has a different signature and semantics. Instead of modifying the resulting `item` and `includeTree` with `ref` parameters, these values can be optionally overridden by returning an ItemResult with its `Object` and `IncludeTree` properties populated with non-null values.
- Generated DTOs now generate separate Parameter DTOs (for inputs) and Response DTOs (for outputs). This results in much cleaner and more accurate OpenAPI definitions.
- `IntelliTect.Coalesce.AuditLogging` no longer utilizes `Z.EntityFramework.Plus` - the internal implementation is now purpose-built for Coalesce's needs. Most of the configuration options that were useful to Coalesce applications have been preserved with the same API.
- Audit logging by default now only merges changes when all changes are to non-discrete properties. For all other changes, new records are inserted every time. A plain string is an example of a non-discrete property, while a foreign key is an example of a discrete property.
- `ListResult<T>` constructors have been adjusted to prevent incorrect overload resolution when cloning ListResult objects. The `IListResult` and `IApiResult` interfaces has been removed and replace with a non-generic `ListResult` abstract base class. (#445)

### Breaking: Frontend/Typescript

- Many dependency minimum versions increased, including `vue`, `vue-router`, `vuetify`, `typescript` (5.5+), `date-fns`, and more.
- `bindToQueryString`: some parameters were converted to an options object, overloads added for easier binding to a `ref` (#396)
- API callers (e.g. `ViewModel.$save`) now directly return the inner data from their response ListResult/ItemResult instead of returning the outer axios response. The raw response of the previous request is now available on `.rawResponse`.
- The content disposition of file downloads defaults to "inline" instead of "attachment". This allows files to be opened for display in a browser tab. Forced downloads can still be achieved by placing the `download` attribute on the HTML `a` tag that links to the endpoint's URL, or by setting `ForceDownload` on the `File` object instantiated in your C# method.
- `FilterParameters.filter` is always initialized to an empty object to make binding easier (no need to manually initialize it and sprinkle your code with null forgiveness operators).

## Deprecations

- Support for Vue 2 is officially deprecated. `coalesce-vue-vuetify2` will receive critical bugfixes, but will no longer receive new features that Vue 3 receives. If your application is still on Vue 2, you should [migrate ASAP](https://intellitect.github.io/Coalesce/stacks/vue/vue2-to-vue3.html).
- `ControllerActionAttribute` has been merged into `ExecuteAttribute`.
- `LoadFromDataSourceAttribute` has been merged into `ExecuteAttribute`.
- `CreateControllerAttribute` deprecated in favor of using either `[InternalUse]` or `[Create]`/`[Read]`/`[Edit]`/`[Delete]` to preclude the API endpoints of types.
- TypeScript API clients: `withSimultaneousRequestCaching` renamed to `useSimultaneousRequestCaching`.
- `SecurityPermissionLevels.AllowAuthorized` has been renamed to `SecurityPermissionLevels.AllowAuthenticated` (this enum is used by security attributes).
- `ControllerAttribute` is deprecated.

## Features

- A new Coalesce project template has been created, with parameters for common features. Check this out on the Getting Started page in the documentation. (#438) (#428)

### Backend/C#/Codegen

- Support enum collections on models
- Warn during codegen when NPM dependency versions don't match NuGet versions. (#144)
- Add `--verify` parameter to Coalesce CLI, for use in CI builds to ensure that code generation was not forgotten.
- Added helpful error messages for database migration issues during development (#439)
- Treat `DataType.Html` as multiline text
- Reduced the verbosity of save validation error message
- IncludeTrees are now be produced from most forms of projected queries (#437)

### Frontend/Typescript

- Added a custom time picker to c-datetime-picker since the Vuetify one doesn't fit our needs. (#309, #411)
- Added support for explicit bulk saves to `c-admin-editor-page` and `c-admin-table-page`. This can be passed as a prop through their routes, and defaults to "auto" which requires an explicit save when creating a model that contains init-only fields. (#410)
- Typescript types improved for many coalesce-vue-vuetify3 components.
- Declare more permissive (and accurate) types for ViewModel object/collection props (#371)
- Generated DataSources are now always reactive - no need to explicitly wrap in `reactive` in order to bind parameters.
- DataSources namespaces are aliased onto generated ViewModel and ListViewModel classes.
- Model classes may now have descriptions (via C# attributes), visible in admin pages (#419)
- Support Suspense/KeepAlive in `bindToQueryString` (#396)
- Always show action button column in `c-table` (#420)
- Improved UX on editable admin tables
- Add `DisplayOptions` parameter to `ViewModel.$display` (#400)
- Add `ApiState.confirmInvoke` (#412)
- Add `ViewModel.$bulkSavePreview` (#413)
- Add `immediate` parameter to list auto-load
- Add `useSimultaneousRequestCaching` to ApiState

## Fixes

- OpenAPI definitions for file download endpoints are now emitted correctly. (#436)
- Data source parameters not passed to root data source for bulk saves
- Don't force init-only properties to be required. (#386)
- Stop defaulting persistent-clear enabled on c-select
- Don't attempt to show native datepicker on disabled/readonly c-datetime-picker (#407)
- `for="ModelType"` syntax not working on c-select-string-value
- `$savingProps` getting cleared a tick before model reload after save in vue3.
- Be more careful about automatically opening `c-select-string-value` when the search term changes.
- `c-select-string-value` search input bound incorrectly
- Broken PK detection for StandaloneEntities
- Bulk saves looking at wrong metadata to determine property relational role
- Prevent scroll jumps when c-loader-status progress bar shows/hides (even when placeholder is used).
- Stack overflow when reloading existing recursive ViewModel with recursive input
- Vue module augmentation incompatible with newer vue-router versions
- Vite-plugin: handle invalid local cert files
- Vite-plugin: handle nested `public` folder structures
- Remove opening delay when clicking a c-select.

---

Notice: Coalesce versions prior to v5 did not use a typical release cadence. The changes for these versions have been broken down by month, up through April 2023.

# 4.0.0 2024-07

- feat: support collections of primitives as data source parameters
- fix: #370 `<see>` comments in XML docs are dropped
- fix: autofocus on c-select
- fix: incorrect attribute inheritance in c-select
- fix: incorrect defaults for `clearable` in c-select
- fix: incorrect serialization of null datasource parameters

# 4.0.0 2024-06

- feat: add `additionalRoots` option to $bulkSave
- feat: add `$modelOnlyMode` to `ListViewModel` for more performant read-only usages.
- feat: ban async void
- feat: bulk save enhancements
- fix: stack overflow when reloading existing recursive ViewModel with recursive input

# 4.0.0 2024-05

- chore!: remove eol net7 target
- feat: support `instanceof` operator on models, even if the model was created from the model constructor
- feat: support for ServiceViewModel in c-admin-method(s?)
- fix: #385 display functions/components didn't handle many-to-many collections
- fix: bulk saves failing to save child objects that lack both a reference navigation and a foreign key to their parent.
- fix: don't generate `displayProp` metadata for datasources
- fix: allow null `model` prop on all components that accept it
- fix: c-select not propagating update:modelValue event for inputs that delegate to coalesce components

# 4.0.0 2024-04

- feat: #379 allow global suppression of auto-includes
- fix: don't generate `required` rules for properties that are not sent to the server
- fix: improve threadsafety of ReflectionRepository
- fix(audit): better logic for excluding empty entries that won't break on entities with alternate keys or composite keys.
- types: strongly type `key` parameter of `bindToQueryString`

# 4.0.0 2024-03

- feat: respect HiddenAttribute for methods in vue
- fix: c-select getting stuck on `defineModel` local values
- fix: support DateOnly and TimeOnly in bulkSave

# 4.0.0 2024-02

- feat: #334 add `autosave` functions to ListViewModel
- feat: #369 make code generation MUCH faster
- feat: add a `-WhatIf` CLI option
- feat: expose `GetDataSourceType` on `IDataSourceFactory` and `GetBehaviorsType` on `IBehaviorsFactory`.
- feat: skip loading data that is possibly more stale than a ViewModel's current data.
- feat: acquire member descriptions from `DescriptionAttribute` if `DisplayAttribute` not present
- fix: allow `AddCoalesce()` to work when web services are not present.
- fix: c-select "No results found" message missing
- fix: dont load navigation props if `skipSaving` is true and the nav prop's FK is saving.
- fix: issue where a transformed index.html could be written to wwwroot before the real vite dev server port has a chance to be obtained from vite.
- fix: warnings in vue 3.4.15+ about mutations in computed getters.
- fix: `ViewModelCollection.filter()` and similar array functions to produce a plain array instead of a ViewModelCollection.

# 4.0.0 2024-01

- feat!: emit method call signatures that more accurately reflect optionality of parameters
- feat: #343 add `progress-absolute` prop to `c-loader-status`
- feat: add new syntax for simple usages of `c-loader-status`
- feat: allow viewmodel dirty tracking to detect direct mutations of primitive collection props
- feat: #291 add delete button to c-admin-editor
- feat: add generics to components for better intellisense (#357)
- fix: improper operator precedence in dto edit role checks with multiple role lists
- fix: #347 strip extraneous periods from combined validation message when saves fail.
- fix: #351 overflow for very large value in RangeAttribute
- fix: guid frontend validation rule not getting emitted
- fix(c-select): simplify hint logic, allow null hint
- fix(hmr): update worker import transform for vite 5
- fix(vue3): allow `null` for c-display's `model` prop

# 4.0.0 2023-12

- feat: #290 support some new .net 8 validation attributes
- feat: #342 improve validation of duplicate names
- fix: #325 incorrect URL validation regex
- fix: #350 incorrect HTTP status code returned for forbidden authenticated users
- fix: add more property names for user navigation property detection to audit logs
- fix: audit log long property name wrapping
- fix: c-select-many-to-many - don't forcibly disable in possible bulkSave usage scenarios.
- fix: multitude of fixes for c-admin-audit-log-page

# 4.0.0 2023-11

- refactor!: Removed `System.Linq.Dynamic`
- feat(audit-logs)!: Add a `Description` property to IAuditLog that will be populated from the changed entity's list text.
- feat: #331 implement custom property restrictions with `RestrictAttribute`.
- feat: add `predicate` option to $bulkSave
- feat: add simple EF entity created/modified by/date stamping hook
- feat: support `DefaultValueAttribute` (#308)
- feat: make `ClaimsPrincipal` non-nullable throughout DataSources, Behaviors, and MappingContext.
- fix: bulk save not traversing all children in some scenarios
- fix: default format string for TimeOnly values was including the date
- fix: don't overwrite actively saving props with stale data from get/list calls.
- fix: don't use string_agg in audit log merging for maximum sql server version compatibility
- fix: ListViewModel.$items initial value was incorrect
- fix: preserve two-letter uppercase in ToProperCase
- fix: prevent conflation of Symbol and Reflection TypeViewModel instances
- fix(hmr-plugin): don't copy directories that don't exist
- fix(vue2): ListViewModel.$items reactivity

# 4.0.0 2023-10

- feat: add audit logging (#327)
- feat: support discovering foreign keys via `[ForeignKeyAttribute]` placed on a collection navigation props. Support foreign keys that do not have a reference navigation prop.
- fix: respect attribute inheritance in Symbol contexts
- fix: allow getter-only collection navigation properties
- fix: correctly parse "Z" offset specifier in parseJSONDate
- fix: fix a few scenarios and add a few test around foreign keys without reference nav props
- fix: update classes in c-select to reflect classes used by latest vuetify
- fix: user field detection on c-admin-audit-log-page
- fix(vue2): participate in RefUnwrapBailTypes

# 4.0.0 2023-09

- chore!: remove targets for netstandard and netcoreapp2.2.
- feat!: Added bulk saves (#312, #316)
- feat: support `DateOnly` and `TimeOnly`.
- feat: #293 don't render editors for readonly fields in editable tables
- feat: #294 add `cache` prop to `c-select` and `c-select-many-to-many`
- feat: #296 suppress "Request Aborted" failures on page unload in Firefox
- feat: #313 purge stale response cache entries, and don't error when the storage destination is full
- feat: support named options instances with `UseViteDevelopmentServer`.
- feat: support net8.0 (#317)
- fix: always allow generator tools to roll forward to latest SDK
- fix: don't raise a generation error if the only exposed types are services (no models or external types)
- fix: #315 improper concatenation to null and other UX issues with c-select

# 4.0.0 2023-08

- feat: add additional css classes to c-admin-editor and c-table for custom styling
- feat(vue3): pass slots through c-input
- fix: input component binding to args on ApiCaller instances for types that aren't directly delegated by c-input was not working. Fixes for c-select, c-datetime-picker, c-select-values, and - c-select-string-values.
- fix(vite-plugin) #289: copy public folder on startup so that static assets are not missing.
- fix(vue3): adjust classes used by hide-non-error-details
- fix(vue3): c-datetime-picker was passing raw strings to validation rules
- fix(vue3): do not pass default slot through c-input to vuetify components.
- fix(vue3): don't allow defaults to override the variant of the search field in c-select, as some variants look bad, including `outlined` which will obscure the loading bar with the field's outline.
- fix(vue3): fix handling of clearing c-datetime-picker

# 4.0.0 2023-07

- fix(vue3): #310 simultaneous changes of querystring bound values not working due to stale reads of current querystring.
- fix(vue3): #311 use RefUnwrapBailTypes to inform vue's types that our ViewModels explicitly perform their own reactivity.
- fix(vue3): handle the "clear" button in html5 date inputs
- fix(vue3): leaky styling of `ul` in `c-loader-status`

# 4.0.0 2023-06

- feat!: don't treat abstract entities as if they have no DbSet.
- feat: greatly improve the formats of user input that will be parsed by c-datetime-picker. Also, the vue3 version of c-datetime-picker now emits input as the user types, rather than only on change/blur/- enter.
- feat: support custom display names for model types
- feat(vue): improve customizability of admin pages
- fix (vue3): #298 broken behavior in html5 date pickers.
- fix: handle data source name case-insensitively in mapParamsToDto
- fix: partially revert 39ea504b4 - nullable properties on attributes can't be set with initializers.
- fix: validation errors on c-admin-editor were not being eagerly displayed when editing an existing object

# 4.0.0 2023-05

fix: don't make identity PKs required when they're a reference type (e.g. `string`)
fix: don't require navigation props

# 4.0.0 2023-04

- chore: fix warning in knockout \_Layout.cshtml
- chore: remove targets for netcoreapp3.1, which is EOL
- feat #285: Add NoAutoInclude property to ReadAttribute that will disable the Default Loading Behavior.
- feat: #288 add frontend vue validation for `[UrlAttribute]`
- feat: emit subtype metadata for EmailAddress and Phone attributes.
- feat(vue): #286 Add constructors to generated data sources to allow inline initialization of values
- fix: #284 Always skip non-IsClientSerializable properties for validation
- fix: non-nullable numbers with a `[Range]` excluding zero are implicitly required
- fix: typescript 5 doesn't like usage of raw numbers where enums are expected in generated metadata
- fix!: remove arbitrary default depth on mapToDto.
- fix(vite-plugin): correctly escape newlines so they are rendered correctly into index.html
- fix(vue3): prevent bindToQueryString from mutating the querystring of the new route during navigation
