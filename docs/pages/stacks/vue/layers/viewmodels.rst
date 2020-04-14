.. _VueViewModels:

ViewModel Layer
================

The API client layer, generated as `viewmodels.g.ts`, exports for each :ref:`EntityModels` and :ref:`CustomDTOs` in your data model both a ViewModel class representing a single instance of the type, and a ListViewModel class that is used to interact with the list functionality in Coalesce. Additionally, each :ref:`Service <Services>` also has a ViewModel class generated.

These classes provide a wide array of functionality that is useful when interacting with your data model through a user interface.

.. contents:: Contents
    :local:

ViewModels 
----------

Data Properties
...............

Model Properties
    Each ViewModel class implements the corresponding interface from the :ref:`VueModels`, meaning that the ViewModel has a data property for each :ref:`Property <ModelProperties>` on the model. Object-typed properties will be typed as the corresponding generated ViewModel.

    Changing the value of a property will automatically flag that property as dirty. See sections below on how property dirty flags are used.

:ts:`$metadata: ModelType`
    The metadata object from the :ref:`VueMetadata` layer for the type represented by the ViewModel.

:ts:`$primaryKey: string | number`
    A getter/setter property that wraps the primary key of the model. Used to interact with the primary key of any ViewModel in a polymorphic way.

:ts:`$display(prop?: string | Property): string`
    Returns a string representation of the object, or one of its properties if specified, suitable for display.

:ts:`constructor(initialDirtyData?: {} | TModel | null)` (Constructor)
    Create a new instance of the ViewModel, loading it with the given initial data (if any) and flagging any loaded properties as dirty (see below).

:ts:`$stableId: number`
    An immutable number that is unique among all ViewModel instances, regardless of type.

    Useful for uniquely identifying instances with ``:key="vm.$stableId"`` in a Vue component, especially for instances that lack a primary key.

Auto-save & Dirty Flags
......................

:ts:`$startAutosave(vue: Vue, options: AutoSaveOptions<this> = {})`
    Starts auto-saving of the instance when its savable data properties become dirty. Saves are performed with the :ts:`$save` :ref:`API Caller <VueApiCallers>` (documented below) and will not be performed if the ViewModel has any validation errors - see :ref:`VueViewModelsValidation` below.

    Requires a reference to a Vue instance in order to manage lifetime (auto-save hooks will be destroyed when the Vue component provided is destroyed).

    Options are as follows:

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

Parameters & API Callers
........................

:ts:`$params: DataSourceParameters`
    An object containing the :ref:`DataSourceStandardParameters` to be used for the :ts:`$load`, :ts:`$save`,and :ts:`$delete` API callers.

:ts:`$dataSource: DataSource`
    Getter/setter wrapper around :ts:`$params.dataSource`. Takes an instance of a :ref:`Data Source <DataSources>` class :ref:`generated in the Model Layer <VueModelsDataSource>`.

:ts:`$includes: string | null`
    Getter/setter wrapper around :ts:`$params.includes`. See :ref:`Includes` for more information.

:ts:`$load: ItemApiState`
    An :ref:`API Caller <VueApiCallers>` for the ``/load`` endpoint. Accepts an optional :ts:`id` argument - if not provided, the ViewModel's :ts:`$primaryKey` is used instead. Uses the instance's :ts:`$params` object for the :ref:`DataSourceStandardParameters`.

:ts:`$save: ItemApiState`
    An :ref:`API Caller <VueApiCallers>` for the ``/save`` endpoint. Uses the instance's :ts:`$params` object for the :ref:`DataSourceStandardParameters`.

    This caller is used for both manually-triggered saves in custom code and for auto-saves. If the :ref:`VueViewModelsValidation` report any errors, an error will be thrown.

    When a save creates a new record and a new primary key is returned from the server, any entities attached to the current ViewModel via a collection navigation property will have their foreign keys set to the new primary key. This behavior, combined with the usage of deep auto-saves, allows for complex object graphs to be constructed even before any model in the graph has been created.

    Saving behavior can be further customized with :ts:`$loadResponseFromSaves` and :ts:`$saveMode`, listed below.

:ts:`$loadResponseFromSaves: boolean`
    Default :ts:`true` - controls if a ViewModel will be loaded with the data from the model returned by the ``/save`` endpoint when saved with the :ts:`$save` API caller. There is seldom any reason to disable this.

:ts:`$saveMode: "surgical" | "whole"`
    Configures which properties of the model are sent to the server during a save.

    :ts:`"whole"`
        All serializable properties of the object are sent back to the server with every save. 

    :ts:`"surgical"` (default)
        By default, only dirty properties (and always the primary key) are sent to the server when performing a save. 
        
        This improves the handling of concurrent changes being made by multiple users against different fields of the same entity at the same time - specifically, it prevents a user with stale data for some field X in their browser from overwriting a more recent value of X in the database when the user is only making changes to some other property Y and has no intention of changing X. 
        
        Save mode :ts:`"surgical"` doesn't help when multiple users are editing field X at the same time - if such a scenario is applicable to your application, you must implement `more advanced handling of concurrency conflicts <https://docs.microsoft.com/en-us/ef/core/saving/concurrency>`_.

        .. warning:: 

            Surgical saves require DTOs on the server that are capable of determining which of their properties have been set by the model binder, as surgical saves are sent from the client by entirely omitting properties from the ``x-www-form-urlencoded`` body that is sent to the server.

            The :ref:`GenDTOs` implement the necessary logic for this; however, any :ref:`CustomDTOs` you have written are unlikely to be implementing the same behavior. For :ref:`CustomDTOs`, either implement the same pattern that can be seen in the :ref:`GenDTOs`, or use save mode :ts:`"whole"` instead.
        

:ts:`$delete: ItemApiState`

.. warning::

    This page is a work in progress and is not yet complete!
    
.. _VueViewModelsValidation:

Rules/Validation
..........
:ts:`$addRule`
:ts:`$removeRule`
:ts:`$getRules`
:ts:`$getErrors`
:ts:`$hasError`

ListViewModels
--------------

Data Properties
..................
$items

Parameters & API Callers
........................

:ts:`$params`
:ts:`$load`
:ts:`$count`
:ts:`$hasNextPage`
:ts:`$hasPreviousPage`
:ts:`$previousPage`
:ts:`$nextPage`
:ts:`$page`
:ts:`$pageSize`
:ts:`$pageCount`

Auto Load
..................
:ts:`$startAutoLoad`
:ts:`$stopAutoLoad`



Service ViewModels
------------------