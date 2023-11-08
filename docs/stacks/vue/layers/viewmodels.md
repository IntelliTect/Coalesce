# Vue ViewModel Layer

<!-- MARKER:summary -->

The ViewModel layer, generated as `viewmodels.g.ts`, exports a ViewModel class for each API-backed type in your data model ([Entity Models](/modeling/model-types/entities.md), [Custom DTOs](/modeling/model-types/dtos.md), and [Services](/modeling/model-types/services.md)). It also exports a ListViewModel type for [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md).

These classes provide a wide array of functionality that is useful when interacting with your data model through a user interface. The generated ViewModels are the primary way that Coalesce is used when developing a Vue application.

<!-- MARKER:summary-end -->

[[toc]]

## ViewModels 

The following members can be found on the generated [Entity](/modeling/model-types/entities.md) and [Custom DTO](/modeling/model-types/dtos.md) ViewModels, exported from `viewmodels.g.ts` as `<TypeName>ViewModel`.

### Model Data Properties

Each ViewModel class implements the corresponding interface from the [Model Layer](/stacks/vue/layers/models.md), meaning that the ViewModel has a data property for each [Property](/modeling/model-components/properties.md) on the model. Object-typed properties will be typed as the corresponding generated ViewModel.

Changing the value of a property will automatically flag that property as dirty. See [Auto-save & Dirty Flags](/stacks/vue/layers/viewmodels.md) below for information on how property dirty flags are used.

There are a few special behaviors when assigning to different kinds of data properties on View Models as well:

#### Model Object Properties
- If the object being assigned to the property is not a ViewModel instance, a new instance will be created automatically and used instead of the incoming object. 
- If the model property is a reference navigation, the corresponding foreign key property will automatically be set to the primary key of that object. If the incoming value was null, the foreign key will be set to null.
- If deep auto-saves are enabled on the instance being assigned to, auto-save will be spread to the incoming object, and to all other objects reachable from that object.

#### Model Collection Properties
  - When assigning an entire array, any items in the array that are not a ViewModel instance will have an instance created for them.
  - The same rule goes for pushing items into the existing array for a model collection - a new ViewModel instance will be created and be used instead of the object(s) being pushed.
    
#### Foreign Key Properties
If the corresponding navigation property contains an object, and that object's primary key doesn't match the new foreign key value being assigned, the navigation property will be set to null.


### Other Data Properties & Functions


<Prop def="readonly $metadata: ModelType" lang="ts" />

The metadata object from the [Metadata Layer](/stacks/vue/layers/metadata.md) layer for the type represented by the ViewModel.


<Prop def="readonly $stableId: number" lang="ts" />

An immutable number that is unique among all ViewModel instances, regardless of type.

Useful for uniquely identifying instances with ``:key="vm.$stableId"`` in a Vue component, especially for instances that lack a primary key.


<Prop def="$primaryKey: string | number" lang="ts" />

A getter/setter property that wraps the primary key of the model. Used to interact with the primary key of any ViewModel in a polymorphic way.


<Prop def="$display(prop?: string | Property): string" lang="ts" />

Returns a string representation of the object, or one of its properties if specified, suitable for display.


<Prop def="$addChild(prop: string | ModelCollectionNavigationProperty, initialDirtyData?: {})" lang="ts" />

Creates a new instance of an item for the specified child model collection, adds it to that collection, and returns the item.
If `initialDirtyData` is provided, it will be loaded into the new instance with `$loadDirtyData()`.


### Loading & Parameters


<Prop def="$load: ItemApiState;
$load(id?: TKey) => ItemResultPromise<TModel>;" lang="ts" idPrefix="member-item" />

An [API Caller](/stacks/vue/layers/api-clients.md#api-callers) for the ``/get`` endpoint. Accepts an optional `id` argument - if not provided, the ViewModel's `$primaryKey` is used instead. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters).


<Prop def="$params: DataSourceParameters" lang="ts" idPrefix="member-item" />

An object containing the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters) to be used for the `$load`, `$save`, `$bulkSave`, and `$delete` API callers.


<Prop def="$dataSource: DataSource" lang="ts" idPrefix="member-item" />

Getter/setter wrapper around `$params.dataSource`. Takes an instance of a [Data Source](/modeling/model-components/data-sources.md) class [generated in the Model Layer](/stacks/vue/layers/models.md).


<Prop def="$includes: string | null" lang="ts" idPrefix="member-item" />

Getter/setter wrapper around `$params.includes`. See [Includes String](/concepts/includes.md) for more information.


<Prop def="$loadCleanData(source: {} | TModel, purgeUnsaved = false)" lang="ts" />

Loads data from the provided model into the current ViewModel, and then clears all dirty flags.

Data is loaded recursively into all related ViewModel instances, preserving existing instances whose primary keys match the incoming data.

If auto-save is enabled, only non-dirty properties are updated. This prevents user input that is pending a save from being overwritten by the response from an auto-save ``/save`` request.

If `purgeUnsaved` is true, items without a primary key will be dropped from collection navigation properties. This is used by the `$load` caller in order to fully reset the object graph with the state from the server.
    

<Prop def="$loadDirtyData(source: {} | TModel)" lang="ts" />

Same as `$loadCleanData`, but does not clear any existing dirty flags, nor does it clear any dirty flags that will be set while mutating the data properties of any ViewModel instance that gets loaded.

<Prop def="constructor(initialDirtyData?: {} | TModel | null)" lang="ts" />

Create a new instance of the ViewModel, loading the given initial data with `$loadDirtyData()` if provided.


### Saving and Deleting


<Prop def="$save: ItemApiState;
$save(overrideProps?: Partial<TModel>) => ItemResultPromise<TModel>;" lang="ts" idPrefix="member-item" />

An [API Caller](/stacks/vue/layers/api-clients.md#api-callers) for the ``/save`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters). A save operation saves only properties on the model it is called on - for deep/bulk saves, see [$bulkSave](#member-_bulksave).

This caller is used for both manually-triggered saves in custom code and for auto-saves. If the [Rules/Validation](/stacks/vue/layers/viewmodels.md#rules-validation) report any errors when the caller is invoked, an error will be thrown.

`overrideProps` can provide properties to save that override the [data properties](#model-data-properties) on the ViewModel instance. This allows for manually saving a change to a property without setting the property on the ViewModel instance into a dirty state. This makes it easier to handle some scenarios where changing the value of the property may put the UI into a logically inconsistent state until the save response has been returned from the server - for example, if a change to one property affects the computed value of other properties.

When a save creates a new record and a new primary key is returned from the server, any entities attached to the current ViewModel via a collection navigation property will have their foreign keys set to the new primary key. This behavior, combined with the usage of deep auto-saves, allows for complex object graphs to be constructed even before any model in the graph has been created.

When a save is in progress, the names of properties being saved are in contained in `$savingProps`.

Saving behavior can be further customized with `$loadResponseFromSaves` and `$saveMode`, listed below.


<Prop def="$delete: ItemApiState;
$delete() => ItemResultPromise<TModel>;" lang="ts" idPrefix="member-item" />

An [API Caller](/stacks/vue/layers/api-clients.md#api-callers) for the ``/delete`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters).

If the object was loaded as a child of a collection, it will be removed from that collection upon being deleted. Note that ViewModels currently only support tracking of a single parent collection, so if an object is programmatically added to additional collections, it will only be removed from one of them upon delete.


<Prop def="$loadResponseFromSaves: boolean" lang="ts" />

Default `true` - controls if a ViewModel will be loaded with the data from the model returned by the ``/save`` endpoint when saved with the `$save` API caller. There is seldom any reason to disable this.


<Prop def="$savingProps: ReadonlySet<string>" lang="ts" />

When `$save.isLoading == true`, contains the properties of the model currently being saved by `$save` (including auto-saves). Does not include non-dirty properties even if `$saveMode == 'whole'`.

This can be used to make per-property UI state changes during saves - for example, displaying progress indicators on/near individual inputs, or disabling input controls.


<Prop def="$saveMode: 'surgical' | 'whole'" lang="ts" />

Configures which properties of the model are sent to the server during a save or bulk save.

<div style="margin-left: 20px">

`"surgical"` (default)

By default, only dirty properties (and always the primary key) are sent to the server when performing a save. 

This improves the handling of concurrent changes being made by multiple users against different fields of the same entity at the same time - specifically, it prevents a user with a stale value of some field X from overwriting a more recent value of X in the database when the user is only making changes to some other property Y and has no intention of changing X. 

Save mode `"surgical"` doesn't help when multiple users are editing field X at the same time - if such a scenario is applicable to your application, you must implement [more advanced handling of concurrency conflicts](https://learn.microsoft.com/en-us/ef/core/saving/concurrency).

::: warning
@[import-md "after":"MARKER:surgical-saves-warning", "before":"MARKER:end-surgical-saves-warning"](../../../modeling/model-types/dtos.md)
::: 

`"whole"`

All serializable properties of the object are sent back to the server with every save. 
        
</div>


<Prop def="$getPropDirty(propName: string): boolean" lang="ts" />

Returns true if the given property is flagged as dirty.


<Prop def="$setPropDirty(propName: string, dirty: boolean = true, triggerAutoSave = true)" lang="ts" />

Manually set the dirty flag of the given property to the desired state. This seldom needs to be done explicitly, as mutating a property will automatically flag it as dirty.

If `dirty` is true and `triggerAutoSave` is false, auto-save (if enabled) will not be immediately triggered for this specific flag change. Note that a future change to any other property's dirty flag will still trigger a save of all dirty properties.


<Prop def="$isDirty: boolean" lang="ts" />

Getter/setter that summarizes the model's property-level dirty flags. Returns true if any properties are dirty.

When set to false, all property dirty flags are cleared. When set to true, all properties are marked as dirty.



### Auto-save


<Prop def="// Vue Options API
$startAutoSave(vue: Vue, options: AutoSaveOptions<this> = {})
&nbsp;
// Vue Composition API
$useAutoSave(options: AutoSaveOptions<this> = {})" lang="ts" idPrefix="member-autosave" />

Starts auto-saving of the instance when its savable data properties become dirty. Saves are performed with the `$save` [API Caller](/stacks/vue/layers/api-clients.md#api-callers) (documented above) and will not be performed if the ViewModel has any validation errors - see [Rules/Validation](/stacks/vue/layers/viewmodels.md#rules-validation) below.

``` ts
type AutoSaveOptions<TThis> = 
{ 
    /** Time, in milliseconds, to debounce saves for.  */
    wait?: number;
    
    /** If true, auto-saving will also be enabled for all view models that are
        reachable from the navigation properties & collections of the current view model. */
    deep?: boolean;

    /** Additional options to pass to the third parameter of lodash's `debounce` function. */
    debounce?: DebounceSettings;

    /** A function that will be called before autosaving that can return false to prevent a save. 
        Only allowed if not using deep auto-saves.
    */
    predicate?: (viewModel: TThis) => boolean;
}
```

<Prop def="$stopAutoSave(): void" lang="ts" />
    
Turns off auto-saving of the instance. Does not recursively disable auto-saves on related instances if `deep` was used when auto-save was enabled.

<Prop def="readonly $isAutoSaveEnabled: boolean" lang="ts" />

Returns true if auto-save is currently active on the instance.



### Bulk saves

<Prop def="$bulkSave: ItemApiState;
$bulkSave(options: BulkSaveOptions) => ItemResultPromise<TModel>;" lang="ts" />

Bulk saves save all changes to an object graph in one API call and one database transaction. This includes creation, updates, and deletions of entities.

To use bulk saves, you can work with your ViewModel instances on the client much in the same way you would on the server with Entity Framework. Assign objects to reference navigation properties and modify scalar values to perform creates and updates. To perform deletions, you must call `model.$remove()` on the ViewModel you want to remove, similar how you would call `DbSet<>.Remove(model)` on the server.

If the client-side [Rules/Validation](/stacks/vue/layers/viewmodels.md#rules-validation) report any errors for any of the models being saved in the operation, an error will be thrown.

On the server, each affected entity is handled through the same standard mechanisms as are used by individual saves or deletes ([Behaviors](/modeling/model-components/behaviors.md), [Data Sources](/modeling/model-components/data-sources.md), and [Security Attributes](/modeling/model-components/attributes/security-attribute.md)), but with a bit of sugar on top:
* All operations are wrapped in a single database transaction that is rolled back if any individual operation fails.
* Foreign keys will be fixed up as new items are created, allowing a parent and child record to be created at the same time even when the client has no foreign key to link the two together.

For the response to a bulk save, the server will load and return the root ViewModel that `$bulkSave` was called upon, using the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters).

@[import-md "start":"export interface BulkSaveOptions", "end":"\n}\n", "prepend":"``` ts", "append":"```"](../../../../src/coalesce-vue/src/viewmodel.ts)

<Prop def="$remove(): void" lang="ts" />

Removes the item from its parent collection (if it is in a collection), and marks the item for deletion in the next bulk save.

<Prop def="readonly $isRemoved: boolean" lang="ts" />

Returns true if the instance was previously removed by calling `$remove()`.

### Rules/Validation


<Prop def="$addRule(prop: string | Property, identifier: string, rule: (val: any) => true | string)" lang="ts" />

Add a custom validation rule to the ViewModel for the specified property. `identifier` should be a short, unique slug that describes the rule; it is not displayed in the UI, but is used if you wish to later remove the rule with `$removeRule()`.

The function you provide should take a single argument that contains the current value of the property, and should either return `true` to indicate that the validation rule has succeeded, or a string that will be displayed as an error message to the user.

Any failing validation rules on a ViewModel will prevent that ViewModel's `$save` caller from being invoked.


<Prop def="$removeRule(prop: string | Property, identifier: string)" lang="ts" />

Remove a validation rule from the ViewModel for the specified property and rule identifier.

This can be used to remove either a rule that was provided by the generated [Metadata Layer](/stacks/vue/layers/metadata.md), or a custom rule that was added by `$addRule`. Reference your generated metadata file `metadata.g.ts` to see any generated rules and the identifiers they use.


<Prop def="$getRules(prop: string | Property): ((val: any) => string | true)[]" lang="ts" />

Returns an array of active rule functions for the specified property, or `undefined` if the property has no active validation rules.


<Prop def="$getErrors(prop?: string | Property): Generator<string>" lang="ts" />

Returns a [generator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator) that provides all error messages for either a specific property (if provided) or the entire model (if no prop argument is provided).

::: tip
You can obtain an array from a generator with `Array.from(vm.$getErrors())` or `[...vm.$getErrors()]`
:::


<Prop def="readonly $hasError: boolean" lang="ts" />

Indicates if any properties have validation errors.


### Generated Members

#### API Callers
For each of the instance [Methods](/modeling/model-components/methods.md) of the type, an [API Caller](/stacks/vue/layers/api-clients.md#api-callers) will be generated.

#### `addTo*()` Functions
For each [collection navigation property](/modeling/model-components/properties.md), a method is generated that will create a new instance of the ViewModel for the collected type, add it to the collection, and then return the new object.
    
#### Many-to-many helper collections
For each [collection navigation property](/modeling/model-components/properties.md) annotated with [[ManyToMany]](/modeling/model-components/attributes/many-to-many.md), a getter-only property is generated that returns a collection of the object on the far side of the many-to-many relationship. Nulls are filtered from this collection.



## ListViewModels

The following members can be found on the generated ListViewModels, exported from `viewmodels.g.ts` as `*TypeName*ListViewModel`.

### Data Properties

<Prop def="readonly $items: T[]" lang="ts" />

Collection holding the results of the last successful invocation of the `$load` [API Caller](/stacks/vue/layers/api-clients.md#api-callers).



### Parameters & API Callers


<Prop def="$params: ListParameters" lang="ts" idPrefix="member-list" />

An object containing the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters) to be used for the `$load` and `$count` API callers.


<Prop def="$dataSource: DataSource" lang="ts" idPrefix="member-list" />

Getter/setter wrapper around `$params.dataSource`. Takes an instance of a [Data Source](/modeling/model-components/data-sources.md) class [generated in the Model Layer](/stacks/vue/layers/models.md).


<Prop def="$includes: string | null" lang="ts" idPrefix="member-list" />

Getter/setter wrapper around `$params.includes`. See [Includes String](/concepts/includes.md) for more information.


<Prop def="$load: ListApiState;
$load() => ListResultPromise<TModel>" lang="ts" idPrefix="member-list" />

An [API Caller](/stacks/vue/layers/api-clients.md#api-callers) for the ``/list`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters).

Results are available in the `$items` property. The `result` property of the `$load` API Caller contains the raw results and is not recommended for use in general development - `$items` should always be preferred.


<Prop def="$count: ItemApiState;
$count() => ItemResultPromise<number>" lang="ts" />

An [API Caller](/stacks/vue/layers/api-clients.md#api-callers) for the ``/count`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters).

The result is available in `$count.result` - this API Caller does not interact with other properties on the ListViewModel like `$pageSize` or `$pageCount`.


<Prop def="readonly $hasPreviousPage: boolean 
readonly $hasNextPage: boolean" lang="ts" />

Properties which indicate if `$page` can be decremented or incremented, respectively. `$pageCount` and `$page` are used to make this determination.

<Prop def="$previousPage(): void 
$nextPage(): void" lang="ts" />

Methods that will decrement or increment `$page`, respectively. Each does nothing if there is no previous or next page as returned by `$hasPreviousPage` and `$hasNextPage`.


<Prop def="$page: number" lang="ts" />

Getter/setter wrapper for `$params.page`. Controls the page that will be requested on the next invocation of `$load`.


<Prop def="$pageSize: number" lang="ts" />

Getter/setter wrapper for `$params.pageSize`. Controls the page that will be requested on the next invocation of `$load`.


<Prop def="readonly $pageCount: number" lang="ts" />

Shorthand for `$load.pageCount` - returns the page count reported by the last successful invocation of `$load`.



### Auto-Load


<Prop def="// Vue Options API
$startAutoLoad(vue: Vue, options: AutoLoadOptions<this> = {})
&nbsp;
// Vue Composition API
$useAutoLoad(options: AutoLoadOptions<this> = {})" lang="ts" idPrefix="member-autoLoad" />

Starts auto-loading of the list as changes to its parameters occur. Loads are performed with the `$load` [API Caller](/stacks/vue/layers/api-clients.md#api-callers).


``` ts
type AutoLoadOptions<TThis> =
{ 
    /** Time, in milliseconds, to debounce loads for.  */
    wait?: number;

    /** Additional options to pass to the third parameter of lodash's `debounce` function. */
    debounce?: DebounceSettings;

    /** A function that will be called before loading that can return false to prevent a load. */
    predicate?: (viewModel: TThis) => boolean;
}
```

<Prop def="$stopAutoLoad()" lang="ts" />

Manually turns off auto-loading of the instance.



### Generated Members

#### API Callers
    
For each of the static [Methods](/modeling/model-components/methods.md) on the type, an [API Caller](/stacks/vue/layers/api-clients.md#api-callers) will be created.



## Service ViewModels

The following members can be found on the generated Service ViewModels, exported from `viewmodels.g.ts` as `<ServiceName>ViewModel`.

### Generated Members

#### API Callers
    
For each method of the [Service](/modeling/model-types/services.md), an [API Caller](/stacks/vue/layers/api-clients.md#api-callers) will be created.
