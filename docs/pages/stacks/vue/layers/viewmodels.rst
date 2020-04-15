.. _VueViewModels:

ViewModel Layer
================

The ViewModel layer, generated as `viewmodels.g.ts`, exports for each :ref:`EntityModels` and :ref:`CustomDTOs` in your data model both a ViewModel class representing a single instance of the type, and a ListViewModel class that is used to interact with the list functionality in Coalesce. Additionally, each :ref:`Service <Services>` also has a ViewModel class generated.

These classes provide a wide array of functionality that is useful when interacting with your data model through a user interface. The generated ViewModels are the primary way that Coalesce is used when developing a Vue application.

.. contents:: Contents
    :local:

.. _VueInstanceViewModels:

ViewModels 
----------

The following members can be found on the generated ViewModels, exported from `viewmodels.g.ts` as :ts:`*TypeName*ViewModel`.

Data Properties
...............

Data Properties
    Each ViewModel class implements the corresponding interface from the :ref:`VueModels`, meaning that the ViewModel has a data property for each :ref:`Property <ModelProperties>` on the model. Object-typed properties will be typed as the corresponding generated ViewModel.

    Changing the value of a property will automatically flag that property as dirty. See :ref:`VueViewModelsAutoSave` below for information on how property dirty flags are used.

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

:ts:`readonly $metadata: ModelType`
    The metadata object from the :ref:`VueMetadata` layer for the type represented by the ViewModel.

:ts:`$primaryKey: string | number`
    A getter/setter property that wraps the primary key of the model. Used to interact with the primary key of any ViewModel in a polymorphic way.

:ts:`$display(prop?: string | Property): string`
    Returns a string representation of the object, or one of its properties if specified, suitable for display.

:ts:`constructor(initialDirtyData?: {} | TModel | null)` (Constructor)
    Create a new instance of the ViewModel, loading it with the given initial data (if any) and flagging any loaded properties as dirty (see below).

:ts:`readonly $stableId: number`
    An immutable number that is unique among all ViewModel instances, regardless of type.

    Useful for uniquely identifying instances with ``:key="vm.$stableId"`` in a Vue component, especially for instances that lack a primary key.

Parameters & API Callers
........................
:ts:`$load: ItemApiState`
    An :ref:`API Caller <VueApiCallers>` for the ``/get`` endpoint. Accepts an optional :ts:`id` argument - if not provided, the ViewModel's :ts:`$primaryKey` is used instead. Uses the instance's :ts:`$params` object for the :ref:`DataSourceStandardParameters`.

:ts:`$save: ItemApiState`
    An :ref:`API Caller <VueApiCallers>` for the ``/save`` endpoint. Uses the instance's :ts:`$params` object for the :ref:`DataSourceStandardParameters`.

    This caller is used for both manually-triggered saves in custom code and for auto-saves. If the :ref:`VueViewModelsValidation` report any errors when the caller is invoked, an error will be thrown.

    When a save creates a new record and a new primary key is returned from the server, any entities attached to the current ViewModel via a collection navigation property will have their foreign keys set to the new primary key. This behavior, combined with the usage of deep auto-saves, allows for complex object graphs to be constructed even before any model in the graph has been created.

    Saving behavior can be further customized with :ts:`$loadResponseFromSaves` and :ts:`$saveMode`, listed below.

:ts:`$loadResponseFromSaves: boolean`
    Default :ts:`true` - controls if a ViewModel will be loaded with the data from the model returned by the ``/save`` endpoint when saved with the :ts:`$save` API caller. There is seldom any reason to disable this.

:ts:`$saveMode: "surgical" | "whole"`
    Configures which properties of the model are sent to the server during a save.

    :ts:`"surgical"` (default)
        By default, only dirty properties (and always the primary key) are sent to the server when performing a save. 
        
        This improves the handling of concurrent changes being made by multiple users against different fields of the same entity at the same time - specifically, it prevents a user with a stale value of some field X from overwriting a more recent value of X in the database when the user is only making changes to some other property Y and has no intention of changing X. 
        
        Save mode :ts:`"surgical"` doesn't help when multiple users are editing field X at the same time - if such a scenario is applicable to your application, you must implement `more advanced handling of concurrency conflicts <https://docs.microsoft.com/en-us/ef/core/saving/concurrency>`_.

        .. warning:: 

            Surgical saves require DTOs on the server that are capable of determining which of their properties have been set by the model binder, as surgical saves are sent from the client by entirely omitting properties from the ``x-www-form-urlencoded`` body that is sent to the server.

            The :ref:`GenDTOs` implement the necessary logic for this; however, any :ref:`CustomDTOs` you have written are unlikely to be implementing the same behavior. For :ref:`CustomDTOs`, either implement the same pattern that can be seen in the :ref:`GenDTOs`, or use save mode :ts:`"whole"` instead.

    :ts:`"whole"`
        All serializable properties of the object are sent back to the server with every save. 
        

:ts:`$delete: ItemApiState`
    An :ref:`API Caller <VueApiCallers>` for the ``/delete`` endpoint. Uses the instance's :ts:`$params` object for the :ref:`DataSourceStandardParameters`.

    If the object was loaded as a child of a collection, it will be removed from that collection upon being deleted. Note that ViewModels currently only support tracking of a single parent collection, so if an object is programatically added to additional collections, it will only be removed from one of them upon delete.

:ts:`$params: DataSourceParameters`
    An object containing the :ref:`DataSourceStandardParameters` to be used for the :ts:`$load`, :ts:`$save`, and :ts:`$delete` API callers.

:ts:`$dataSource: DataSource`
    Getter/setter wrapper around :ts:`$params.dataSource`. Takes an instance of a :ref:`Data Source <DataSources>` class :ref:`generated in the Model Layer <VueModelsDataSource>`.

:ts:`$includes: string | null`
    Getter/setter wrapper around :ts:`$params.includes`. See :ref:`Includes` for more information.


.. _VueViewModelsAutoSave:

Auto-save & Dirty Flags
......................

:ts:`$startAutosave(vue: Vue, options: AutoSaveOptions<this> = {})`
    Starts auto-saving of the instance when its savable data properties become dirty. Saves are performed with the :ts:`$save` :ref:`API Caller <VueApiCallers>` (documented below) and will not be performed if the ViewModel has any validation errors - see :ref:`VueViewModelsValidation` below.

    Requires a reference to a Vue instance in order to manage lifetime (auto-save hooks will be destroyed when the Vue component provided is destroyed). Options are as follows:

    .. code-block:: typescript

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

:ts:`$stopAutosave()`
    Turns off auto-saving of the instance. Does not recursively disable auto-saves on related instances if :ts:`deep` was used when auto-save was enabled.

:ts:`$getPropDirty(propName: string): boolean`
    Returns true if the given property is flagged as dirty.

:ts:`$setPropDirty(propName: string, dirty: boolean = true, triggerAutosave = true)`
    Manually set the dirty flag of the given property to the desired state. This seldom needs to be done explicitly, as mutating a property will automatically flag it as dirty.

    If :ts:`dirty` is true and :ts:`triggerAutosave` is false, auto-save (if enabled) will not be immediately triggered for this specific flag change. Note that a future change to any other property's dirty flag will still trigger a save of all dirty properties.

:ts:`$isDirty: boolean`
    Getter/setter that summarizes the model's property-level dirty flags. Returns true if any properties are dirty.

    When set to false, all property dirty flags are cleared. When set to true, all properties are marked as dirty.

:ts:`$loadCleanData(source: {} | TModel)`
    Loads data from the provided model into the current ViewModel, and then clears all dirty flags.

    Data is loaded recursively into all related ViewModel instances, preserving existing instances whose primary keys match the incoming data.

    If auto-save is enabled, only non-dirty properties are updated. This prevents user input that is pending a save from being overwritten by the response from an auto-save ``/save`` request.
    
:ts:`$loadDirtyData(source: {} | TModel)`
    Same as :ts:`$loadCleanData`, but does not clear any existing dirty flags, nor does it clear any dirty flags that will be set while mutating the data properties of any ViewModel instance that gets loaded.


.. _VueViewModelsValidation:

Rules/Validation
..........
:ts:`$addRule(prop: string | Property, identifier: string, rule: (val: any) => true | string)`
    Add a custom validation rule to the ViewModel for the spcified property. :ts:`identifier` should be a short, unique slug that describes the rule; it is not displayed in the UI, but is used if you wish to later remove the rule with :ts:`$removeRule()`.

    The function you provide should take a single argument that contains the current value of the property, and should either return :ts:`true` to indicate that the validation rule has succeeded, or a string that will be displayed as an error message to the user.

    Any failing validation rules on a ViewModel will prevent that ViewModel's :ts:`$save` caller from being invoked.

:ts:`$removeRule(prop: string | Property, identifier: string)`
    Remove a validation rule from the ViewModel for the spcified property with the specified identifier.

    This can be used to remove from the ViewModel instance either a rule that was provided by the generated :ref:`VueMetadata`, or a custom rule that was added by :ts:`$addRule`. Reference your generated metadata file `metadata.g.ts` to see any generated rules and the identifiers they use.

:ts:`$getRules(prop: string | Property)`
    Returns an array of active rule functions for the specified property, or :ts:`undefined` if the property has no active validation rules.

:ts:`$getErrors(prop?: string | Property): Generator<string>`
    Returns a `generator <https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator>`_ that provides all error messages for either a specific property (if provided) or the entire model (if no prop argument is provided).

    .. tip:: You can obtain an array from a generator with :ts:`Array.from(vm.$getErrors())` or :ts:`[...vm.$getErrors()]`

:ts:`readonly $hasError: boolean`
    Indicates if any propertioes have validation errors.


Generated Members
.................

Method Callers
    For each of the instance :ref:`ModelMethods` of the type, an :ref:`API Caller <VueApiCallers>` will be generated.

:ts:`addTo*()`
    For each :ref:`collection navigation property <ModelProperties>`, a method is generated that will create a new instance of the ViewModel for the collected type, add it to the collection, and then return the new object.
    
Many-to-many helper collections
    For each :ref:`collection navigation property <ModelProperties>` annotated with :ref:`ManyToMany`, a getter-only property is generated that returns a collection of the object on the far side of the many-to-many relationship. Nulls are filtered from this collection.

|



.. _VueListViewModels:

ListViewModels
--------------

The following members can be found on the generated ListViewModels, exported from `viewmodels.g.ts` as :ts:`*TypeName*ListViewModel`.

Data Properties
...............

:ts:`$items`
    Collection holding the results of the last successful invocation of the :ts:`$load` :ref:`API Caller <VueApiCallers>`.

Parameters & API Callers
........................

:ts:`$params: DataSourceParameters`
    An object containing the :ref:`DataSourceStandardParameters` to be used for the :ts:`$load` and :ts:`$count` API callers.

:ts:`$load: ListApiState`
    An :ref:`API Caller <VueApiCallers>` for the ``/list`` endpoint. Uses the instance's :ts:`$params` object for the :ref:`DataSourceStandardParameters`.

    Results are available in the :ts:`$items` property. The :ts:`result` property of the :ts:`$load` API Caller contains the raw results and is not recommended for use in general development - :ts:`$items` should always be prefered.

:ts:`$count: ItemApiState`
    An :ref:`API Caller <VueApiCallers>` for the ``/count`` endpoint. Uses the instance's :ts:`$params` object for the :ref:`DataSourceStandardParameters`.

    The result is available in :ts:`$count.result` - this API Caller does not interact with other properties on the ListViewModel like :ts:`$pageSize` or :ts:`$pageCount`.

:ts:`readonly $hasPreviousPage: boolean`, :ts:`readonly $hasNextPage: boolean`
    Properties which indicate if :ts:`$page` can be decremented or incremented, respectively. :ts:`$pageCount` and :ts:`$page` are used to make this determination.

:ts:`$previousPage()`, :ts:`$nextPage()`
    Methods that will decrement or increment :ts:`$page`, respectively. Each does nothing if there is no previous or next page as returned by :ts:`$hasPreviousPage` and :ts:`$hasNextPage`.

:ts:`$page: number`
    Getter/setter wrapper for :ts:`$params.page`. Controls the page that will be requested on the next invocation of :ts:`$load`.

:ts:`$pageSize: number`
    Getter/setter wrapper for :ts:`$params.pageSize`. Controls the page that will be requested on the next invocation of :ts:`$load`.

:ts:`readonly $pageCount: number`
    Shorthand for :ts:`$load.pageCount` - returns the page count reported by the last successful invocation of :ts:`$load`.

Auto-Load
.........

:ts:`$startAutoLoad(vue: Vue, options: AutoLoadOptions<this> = {})`
    Starts auto-loading of the list as changes to its parameters occur. Loads are performed with the :ts:`$load` :ref:`API Caller <VueApiCallers>`.

    Requires a reference to a Vue instance in order to manage lifetime (auto-load hooks will be destroyed when the Vue component provided is destroyed). Options are as follows:

    .. code-block:: typescript

        { 
            /** Time, in milliseconds, to debounce loads for.  */
            wait?: number;

            /** A function that will be called before loading that can return false to prevent a load. 
            */
            predicate?: (viewModel: TThis) => boolean;
        }

:ts:`$stopAutoLoad()`
    Manually turns off auto-loading of the instance.


Generated Members
.................

Method Callers
    For each of the static :ref:`ModelMethods` on the type, an :ref:`API Caller <VueApiCallers>` will be created.





|


Service ViewModels
------------------

The following members can be found on the generated Service ViewModels, exported from `viewmodels.g.ts` as :ts:`*ServiceName*ViewModel`.

Generated Members
.................

Method Callers
    For each method of the :ref:`Service <Services>`, an :ref:`API Caller <VueApiCallers>` will be created.
