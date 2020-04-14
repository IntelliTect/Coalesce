.. _VueModels:

Model Layer
===========

The model layer, generated as `models.g.ts`, contains a set of TypeScript interfaces that represent each client-exposed type in your data model. Each interface contains all the :ref:`ModelProperties` of that type, as well as a :ts:`$metadata` property that references the :ref:`metadata <VueMetadata>` object for that type. Enums and :ref:`DataSources` are also represented in the model layer.

The model layer also includes a TypeScript class for each type that can be used to easily instantiate a valid implementation of its corresponding interface. However, it is not necessary for the classes to be used, and all parts of Coalesce that interact with the model layer don't perform any `instanceof` checks against models - the :ts:`$metadata` property is used for this instead.


Concepts 
--------

The model layer is fairly simple - the only main concept it introduces on top of the :ref:`VueMetadata` is the notion of interfaces that mirror the C# types in your data model. As with the :ref:`VueMetadata`, the `source code of coalesce-vue <https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/model.ts>`_ provides the most authoritative source of truth about the model layer.

Model
    An interface describing an instance of a class type from your application's data model. All Model interfaces contain members for all the :ref:`ModelProperties` of that type, as well as a :ts:`$metadata` property that references the metadata object for that type.

.. _VueModelsDataSource:
DataSource
    A representation of a :ref:`Data Source <DataSources>`, containing properties for any of the :ref:`DataSourceParameters` of the data source, as well as a :ts:`$metadata` property that references the metadata object for the data source.
    
    Data sources are generated as concrete classes in a namespace named :ts:`DataSources` that is nested inside a namespace named after their parent model type. For example:

    .. code-block:: typescript

        import { Person } from '@/models.g'

        const dataSource = new Person.DataSources.NamesStartingWith;
        dataSource.startsWith = "A";
        // Provide the dataSource to an API Client or a ViewModel.
        


Model Functions
----------------------

The following functions exported from ``coalesce-vue`` can be used with your models:

:ts:`convertToModel(value: any, metadata: Value | ClassType)`
    Given any JavaScript value :ts:`value`, convert it into a valid implementation of the value or type described by :ts:`metadata`.

    For metadata describing a primitive or primitive-like value, the input will be parsed into a valid implementation of the correct JavaScript type. For example, for :ts:`metadata` that describes a boolean, a string :ts:`"true"` will return a boolean :ts:`true`, and ISO 8601 date strings will result in a JavaScript :ts:`Date` object. 

    For metadata describing a type, the input object will be mutated into a valid implementation of the appropriate model interface. Missing properties will be set to null, and any descendent properties of the provided object will be recursively processed with :ts:`convertToModel`.

    If any values are encountered that are fundamentally incompatible with the requested type described by the metadata, an error will be thrown.

:ts:`mapToModel(value: any, metadata: Value | ClassType)`
    Performs the same operations as :ts:`convertToModel`, except that any objects encountered will not be mutated - instead, a new object or array will always be created.

:ts:`mapToDto(value: any, metadata: Value | ClassType)`
    Maps the input to a representation suitable for JSON serialization.

    Will not serialize child objects or collections whose metadata includes `dontSerialize`. Will only recurse to a maximum depth of 3.

:ts:`modelDisplay(model: Model, options?: DisplayOptions)` 
    Returns a string representing the :ts:`model` suitable for display in a user interface.

    Uses the :ts:`displayProp` defined on the object's metadata. If no :ts:`displayProp` is defined, the object will be displayed as JSON. The display prop on a model can be defined in C# with :ref:`ListTextAttribute`.

    See :ref:`DisplayOptions` for available options.

:ts:`propDisplay(model: Model, prop: Property | string, options?: DisplayOptions)`
    Returns a string representing the specified property of the given object suitable for display in a user interface.

    The property can either be a string, representing one of the model's properties, or the actual :ts:`Property` metadata object of the property.

    See :ref:`DisplayOptions` for available options.
    
:ts:`valueDisplay(value: any, metadata: Value, options?: DisplayOptions)`
    Returns a string representing the given value (described by the given metadata).

    See :ref:`DisplayOptions` for available options.

:ts:`bindToQueryString(vue: Vue, obj: {}, key: string, queryKey: string = key, parse?: (v: any) => any, mode: 'push' | 'replace' = 'replace')`
    Binds property :ts:`key` of :ts:`obj` to query string parameter :ts:`queryKey`. When the object's value changes, the query string will be updated using `vue-router <https://router.vuejs.org/>`_. When the query string changes, the object's value will be updated.

    The query string will be updated using either :ts:`router.push` or :ts:`router.replace` depending on the value of parameter :ts:`mode`.
    
    If the query string contains a value when this is called, the object will be updated with that value immediately. 

    If the object being bound to has :ts:`$metadata`, information from that metadata will be used to serialize and parse values to and from the query string. Otherwise, :ts:`String(value)` will be used to serialize the value, and the :ts:`parse` parameter (if provided) will be used to parse the value from the query string.
    
:ts:`bindKeyToRouteOnCreate(vue: Vue, model: Model<ModelType>, routeParamName: string = 'id', keepQuery: boolean = false)`
    When :ts:`model` is created (i.e. its primary key becomes non-null), replace the current URL with one that includes uses primary key for the route parameter named by :ts:`routeParamName`.

    The query string will not be kept when the route is changed unless :ts:`true` is given to :ts:`keepQuery`.

    .. note::
        The route will be replaced directly via the `HTML5 History API <https://developer.mozilla.org/en-US/docs/Web/API/History_API>`_ such that `vue-router <https://router.vuejs.org/>`_ will not observe the change as an actual route change, preventing the current view from being recreated if a path-based key is being used on the application's :code:`<router-view>` component.


.. _DisplayOptions:

DisplayOptions
--------------

The following options are available to functions in coalesce-vue that render a value or object for display:

:ts:`format`
    Options to be used when formatting a date. One of:

    :ts:`string`
        A `UTS#35 <http://unicode.org/reports/tr35/tr35-dates.html>`_ date format string, to be passed to `date-fns's format function <https://date-fns.org/docs/format>`_.

        Defaults to :ts:`"M/d/yyyy"` for date-only dates (specified with :ref:`DateTypeAttribute`), or :ts:`"M/d/yyyy h:mm:ss aaa"` otherwise. 

    :ts:`{ distance: true; addSuffix?: boolean; includeSeconds?: boolean; }`
        Options to be passed to `date-fns's formatDistanceToNow function <https://date-fns.org/docs/formatDistanceToNow>`_.

        .. note::
            Values rendered with :ts:`formatDistanceToNow` function into a Vue component will not automatically be updated in realtime. If this is needed, you should use a strategy like using a `key <https://vuejs.org/v2/api/#key>`_ that you periodically update to force a re-render.

:ts:`collection: { enumeratedItemsMax?: number, enumeratedItemsSeparator?: string }`
    Options to be used when formatting a collection.
