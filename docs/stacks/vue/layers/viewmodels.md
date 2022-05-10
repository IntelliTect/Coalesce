.. _VueViewModels:

ViewModel Layer
================

The ViewModel layer, generated as `viewmodels.g.ts`, exports for each [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md) in your data model both a ViewModel class representing a single instance of the type, and a ListViewModel class that is used to interact with the list functionality in Coalesce. Additionally, each [Service](/modeling/model-types/services.md) also has a ViewModel class generated.

These classes provide a wide array of functionality that is useful when interacting with your data model through a user interface. The generated ViewModels are the primary way that Coalesce is used when developing a Vue application.

[[toc]]

.. _VueInstanceViewModels:

ViewModels 
----------

The following members can be found on the generated ViewModels, exported from `viewmodels.g.ts` as `*TypeName*ViewModel`.

Model Data Properties
.....................

Each ViewModel class implements the corresponding interface from the [Model Layer](/stacks/vue/layers/models.md), meaning that the ViewModel has a data property for each [Property](/modeling/model-components/properties.md) on the model. Object-typed properties will be typed as the corresponding generated ViewModel.

Changing the value of a property will automatically flag that property as dirty. See [Auto-save & Dirty Flags](/stacks/vue/layers/viewmodels.md) below for information on how property dirty flags are used.

There are a few special behaviors when assigning to different kinds of data properties on View Models as well:

Model Object Properties
    - If the object being assigned to the property is not a ViewModel instance, a new instance will be created automatically and used instead of the incoming object. 
    - If the model property is a reference navigation, the corresponding foreign key property will automatically be set to the primary key of that object. If the incoming value was null, the foreign key will be set to null.
    - If deep auto-saves are enabled on the instance being assigned to, auto-save will be spread to the incoming object, and to all other objects reachable from that object.

Model Collection Properties
    - When assigning an entire array, any items in the array that are not a ViewModel instance will have an instance created for them.
    - The same rule goes for pushing items into the existing array for a model collection - a new ViewModel instance will be created and be used instead of the object(s) being pushed.
    
Foreign Key Properties
    If the corresponding navigation property contains an object, and that object's primary key doesn't match the new foreign key value being assigned, the navigation property will be set to null.

|

Other Data Properties & Functions
.................................

`readonly $metadata: ModelType`
    The metadata object from the [Metadata Layer](/stacks/vue/layers/metadata.md) layer for the type represented by the ViewModel.

`readonly $stableId: number`
    An immutable number that is unique among all ViewModel instances, regardless of type.

    Useful for uniquely identifying instances with ``:key="vm.$stableId"`` in a Vue component, especially for instances that lack a primary key.

`$primaryKey: string | number`
    A getter/setter property that wraps the primary key of the model. Used to interact with the primary key of any ViewModel in a polymorphic way.

`$display(prop?: string | Property): string`
    Returns a string representation of the object, or one of its properties if specified, suitable for display.

`$addChild(prop: string | ModelCollectionNavigationProperty)`
    Creates a new instance of an item for the specified child model collection, adds it to that collection, and returns the item.

|

API Callers & Parameters
........................

`$load: ItemApiState`
    An [API Caller](/stacks/vue/layers/api-clients.md) for the ``/get`` endpoint. Accepts an optional `id` argument - if not provided, the ViewModel's `$primaryKey` is used instead. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md).

`$save: ItemApiState`
    An [API Caller](/stacks/vue/layers/api-clients.md) for the ``/save`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md).

    This caller is used for both manually-triggered saves in custom code and for auto-saves. If the [Rules/Validation](/stacks/vue/layers/viewmodels.md) report any errors when the caller is invoked, an error will be thrown.

    When a save creates a new record and a new primary key is returned from the server, any entities attached to the current ViewModel via a collection navigation property will have their foreign keys set to the new primary key. This behavior, combined with the usage of deep auto-saves, allows for complex object graphs to be constructed even before any model in the graph has been created.

    Saving behavior can be further customized with `$loadResponseFromSaves` and `$saveMode`, listed below.

`$loadResponseFromSaves: boolean`
    Default `true` - controls if a ViewModel will be loaded with the data from the model returned by the ``/save`` endpoint when saved with the `$save` API caller. There is seldom any reason to disable this.

`$saveMode: "surgical" | "whole"`
    Configures which properties of the model are sent to the server during a save.

    `"surgical"` (default)
        By default, only dirty properties (and always the primary key) are sent to the server when performing a save. 
        
        This improves the handling of concurrent changes being made by multiple users against different fields of the same entity at the same time - specifically, it prevents a user with a stale value of some field X from overwriting a more recent value of X in the database when the user is only making changes to some other property Y and has no intention of changing X. 
        
        Save mode `"surgical"` doesn't help when multiple users are editing field X at the same time - if such a scenario is applicable to your application, you must implement [more advanced handling of concurrency conflicts](https://docs.microsoft.com/en-us/ef/core/saving/concurrency).

        .. warning:: 

            Surgical saves require DTOs on the server that are capable of determining which of their properties have been set by the model binder, as surgical saves are sent from the client by entirely omitting properties from the ``x-www-form-urlencoded`` body that is sent to the server.

            The [Generated C# DTOs](/stacks/agnostic/dtos.md) implement the necessary logic for this; however, any [Custom DTOs](/modeling/model-types/dtos.md) you have written are unlikely to be implementing the same behavior. For [Custom DTOs](/modeling/model-types/dtos.md), either implement the same pattern that can be seen in the [Generated C# DTOs](/stacks/agnostic/dtos.md), or use save mode `"whole"` instead.

    `"whole"`
        All serializable properties of the object are sent back to the server with every save. 
        

`$delete: ItemApiState`
    An [API Caller](/stacks/vue/layers/api-clients.md) for the ``/delete`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md).

    If the object was loaded as a child of a collection, it will be removed from that collection upon being deleted. Note that ViewModels currently only support tracking of a single parent collection, so if an object is programmatically added to additional collections, it will only be removed from one of them upon delete.

`$params: DataSourceParameters`
    An object containing the [Standard Parameters](/modeling/model-components/data-sources.md) to be used for the `$load`, `$save`, and `$delete` API callers.

`$dataSource: DataSource`
    Getter/setter wrapper around `$params.dataSource`. Takes an instance of a [Data Source](/modeling/model-components/data-sources.md) class [generated in the Model Layer](/stacks/vue/layers/models.md).

`$includes: string | null`
    Getter/setter wrapper around `$params.includes`. See [Includes String](/concepts/includes.md) for more information.


|

.. _VueViewModelsAutoSave:

Auto-save & Dirty Flags
.......................

`$startAutosave(vue: Vue, options: AutoSaveOptions<this> = {})`
    Starts auto-saving of the instance when its savable data properties become dirty. Saves are performed with the `$save` [API Caller](/stacks/vue/layers/api-clients.md) (documented below) and will not be performed if the ViewModel has any validation errors - see [Rules/Validation](/stacks/vue/layers/viewmodels.md) below.

    Requires a reference to a Vue instance in order to manage lifetime (auto-save hooks will be destroyed when the Vue component provided is destroyed). Options are as follows:

    ``` ts

        { 
            /** Time, in milliseconds, to debounce saves for.  */
            wait?: number;
            
            /** If true, auto-saving will also be enabled for all view models that are
                reachable from the navigation properties & collections of the current view model. */
            deep?: boolean;

            /** A function that will be called before autosaving that can return false to prevent a save. 
                Only allowed if not using deep auto-saves.
            */
            predicate?: (viewModel: TThis) => boolean;
        }


    ```

`$stopAutosave()`
    Turns off auto-saving of the instance. Does not recursively disable auto-saves on related instances if `deep` was used when auto-save was enabled.

`$getPropDirty(propName: string): boolean`
    Returns true if the given property is flagged as dirty.

`$setPropDirty(propName: string, dirty: boolean = true, triggerAutosave = true)`
    Manually set the dirty flag of the given property to the desired state. This seldom needs to be done explicitly, as mutating a property will automatically flag it as dirty.

    If `dirty` is true and `triggerAutosave` is false, auto-save (if enabled) will not be immediately triggered for this specific flag change. Note that a future change to any other property's dirty flag will still trigger a save of all dirty properties.

`$isDirty: boolean`
    Getter/setter that summarizes the model's property-level dirty flags. Returns true if any properties are dirty.

    When set to false, all property dirty flags are cleared. When set to true, all properties are marked as dirty.

`$loadCleanData(source: {} | TModel)`
    Loads data from the provided model into the current ViewModel, and then clears all dirty flags.

    Data is loaded recursively into all related ViewModel instances, preserving existing instances whose primary keys match the incoming data.

    If auto-save is enabled, only non-dirty properties are updated. This prevents user input that is pending a save from being overwritten by the response from an auto-save ``/save`` request.
    
`$loadDirtyData(source: {} | TModel)`
    Same as `$loadCleanData`, but does not clear any existing dirty flags, nor does it clear any dirty flags that will be set while mutating the data properties of any ViewModel instance that gets loaded.

`constructor(initialDirtyData?: {} | TModel | null)` (Constructor)
    Create a new instance of the ViewModel, loading the given initial data with `$loadDirtyData()` if provided.

|

.. _VueViewModelsValidation:

Rules/Validation
................

`$addRule(prop: string | Property, identifier: string, rule: (val: any) => true | string)`
    Add a custom validation rule to the ViewModel for the specified property. `identifier` should be a short, unique slug that describes the rule; it is not displayed in the UI, but is used if you wish to later remove the rule with `$removeRule()`.

    The function you provide should take a single argument that contains the current value of the property, and should either return `true` to indicate that the validation rule has succeeded, or a string that will be displayed as an error message to the user.

    Any failing validation rules on a ViewModel will prevent that ViewModel's `$save` caller from being invoked.

`$removeRule(prop: string | Property, identifier: string)`
    Remove a validation rule from the ViewModel for the specified property with the specified identifier.

    This can be used to remove from the ViewModel instance either a rule that was provided by the generated [Metadata Layer](/stacks/vue/layers/metadata.md), or a custom rule that was added by `$addRule`. Reference your generated metadata file `metadata.g.ts` to see any generated rules and the identifiers they use.

`$getRules(prop: string | Property)`
    Returns an array of active rule functions for the specified property, or `undefined` if the property has no active validation rules.

`$getErrors(prop?: string | Property): Generator<string>`
    Returns a [generator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator) that provides all error messages for either a specific property (if provided) or the entire model (if no prop argument is provided).

    .. tip:: You can obtain an array from a generator with `Array.from(vm.$getErrors())` or `[...vm.$getErrors()]`

`readonly $hasError: boolean`
    Indicates if any properties have validation errors.


|

Generated Members
.................

Method Callers
    For each of the instance [Methods](/modeling/model-components/methods.md) of the type, an [API Caller](/stacks/vue/layers/api-clients.md) will be generated.

`addTo*()`
    For each [collection navigation property](/modeling/model-components/properties.md), a method is generated that will create a new instance of the ViewModel for the collected type, add it to the collection, and then return the new object.
    
Many-to-many helper collections
    For each [collection navigation property](/modeling/model-components/properties.md) annotated with [[ManyToMany]](/modeling/model-components/attributes/many-to-many.md), a getter-only property is generated that returns a collection of the object on the far side of the many-to-many relationship. Nulls are filtered from this collection.

|



.. _VueListViewModels:

ListViewModels
--------------

The following members can be found on the generated ListViewModels, exported from `viewmodels.g.ts` as `*TypeName*ListViewModel`.

Data Properties
...............

`$items`
    Collection holding the results of the last successful invocation of the `$load` [API Caller](/stacks/vue/layers/api-clients.md).


|

Parameters & API Callers
........................

`$params: DataSourceParameters`
    An object containing the [Standard Parameters](/modeling/model-components/data-sources.md) to be used for the `$load` and `$count` API callers.

`$load: ListApiState`
    An [API Caller](/stacks/vue/layers/api-clients.md) for the ``/list`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md).

    Results are available in the `$items` property. The `result` property of the `$load` API Caller contains the raw results and is not recommended for use in general development - `$items` should always be preferred.

`$count: ItemApiState`
    An [API Caller](/stacks/vue/layers/api-clients.md) for the ``/count`` endpoint. Uses the instance's `$params` object for the [Standard Parameters](/modeling/model-components/data-sources.md).

    The result is available in `$count.result` - this API Caller does not interact with other properties on the ListViewModel like `$pageSize` or `$pageCount`.

`readonly $hasPreviousPage: boolean`, `readonly $hasNextPage: boolean`
    Properties which indicate if `$page` can be decremented or incremented, respectively. `$pageCount` and `$page` are used to make this determination.

`$previousPage()`, `$nextPage()`
    Methods that will decrement or increment `$page`, respectively. Each does nothing if there is no previous or next page as returned by `$hasPreviousPage` and `$hasNextPage`.

`$page: number`
    Getter/setter wrapper for `$params.page`. Controls the page that will be requested on the next invocation of `$load`.

`$pageSize: number`
    Getter/setter wrapper for `$params.pageSize`. Controls the page that will be requested on the next invocation of `$load`.

`readonly $pageCount: number`
    Shorthand for `$load.pageCount` - returns the page count reported by the last successful invocation of `$load`.


|

Auto-Load
.........

`$startAutoLoad(vue: Vue, options: AutoLoadOptions<this> = {})`
    Starts auto-loading of the list as changes to its parameters occur. Loads are performed with the `$load` [API Caller](/stacks/vue/layers/api-clients.md).

    Requires a reference to a Vue instance in order to manage lifetime (auto-load hooks will be destroyed when the Vue component provided is destroyed). Options are as follows:

    ``` ts

        { 
            /** Time, in milliseconds, to debounce loads for.  */
            wait?: number;

            /** A function that will be called before loading that can return false to prevent a load. 
            */
            predicate?: (viewModel: TThis) => boolean;
        }


    ```

`$stopAutoLoad()`
    Manually turns off auto-loading of the instance.


|

Generated Members
.................

Method Callers
    For each of the static [Methods](/modeling/model-components/methods.md) on the type, an [API Caller](/stacks/vue/layers/api-clients.md) will be created.





|


Service ViewModels
------------------

The following members can be found on the generated Service ViewModels, exported from `viewmodels.g.ts` as `*ServiceName*ViewModel`.

Generated Members
.................

Method Callers
    For each method of the [Service](/modeling/model-types/services.md), an [API Caller](/stacks/vue/layers/api-clients.md) will be created.
