.. _VueModels:

Model Layer
===========

The model layer, generated as `models.g.ts`, contains a set of TypeScript interfaces that represent each client-exposed type in your data model. Each interface contains all the [Properties](/modeling/model-components/properties.md) of that type, as well as a `$metadata` property that references the [metadata](/stacks/vue/layers/metadata.md) object for that type. Enums and [Data Sources](/modeling/model-components/data-sources.md) are also represented in the model layer.

The model layer also includes a TypeScript class for each type that can be used to easily instantiate a valid implementation of its corresponding interface. However, it is not necessary for the classes to be used, and all parts of Coalesce that interact with the model layer don't perform any `instanceof` checks against models - the `$metadata` property is used to determine type identity.

[[toc]]


Concepts 
--------

The model layer is fairly simple - the only main concept it introduces on top of the [Metadata Layer](/stacks/vue/layers/metadata.md) is the notion of interfaces and enums that mirror the C# types in your data model. As with the [Metadata Layer](/stacks/vue/layers/metadata.md), the [source code of coalesce-vue](https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/model.ts) is a great documentation supplement to this page.

Model
.....

An interface describing an instance of a class type from your application's data model. All Model interfaces contain members for all the [Properties](/modeling/model-components/properties.md) of that type, as well as a `$metadata` property that references the metadata object for that type.


.. _VueModelsDataSource: 

DataSource
..........

A class-based representation of a [Data Source](/modeling/model-components/data-sources.md) containing properties for any of the [Custom Parameters](/modeling/model-components/data-sources.md) of the data source, as well as a `$metadata` property that references the metadata object for the data source.

Data sources are generated as concrete classes in a namespace named `DataSources` that is nested inside a namespace named after their parent model type. For example:

``` ts

    import { Person } from '@/models.g'

    const dataSource = new Person.DataSources.NamesStartingWith;
    dataSource.startsWith = "A";
    // Provide the dataSource to an API Client or a ViewModel...



```

Model Functions
---------------

The following functions exported from ``coalesce-vue`` can be used with your models:

`bindToQueryString(vue: Vue, obj: {}, key: string, queryKey: string = key, parse?: (v: any) => any, mode: 'push' | 'replace' = 'replace')`
    Binds property `key` of `obj` to query string parameter `queryKey`. When the object's value changes, the query string will be updated using vue-router_. When the query string changes, the object's value will be updated.

    The query string will be updated using either `router.push` or `router.replace` depending on the value of parameter `mode`.
    
    If the query string contains a value when this is called, the object will be updated with that value immediately. 

    If the object being bound to has `$metadata`, information from that metadata will be used to serialize and parse values to and from the query string. Otherwise, `String(value)` will be used to serialize the value, and the `parse` parameter (if provided) will be used to parse the value from the query string.

    ``` ts

        import { bindToQueryString } from 'coalesce-vue';

        // In the 'created' Vue lifecycle hook on a component:
        created() {
            // Bind pagination information to the query string:
            bindToQueryString(this, this.listViewModel.$params, 'pageSize', 'pageSize', v => +v);

            // Assuming the component has an 'activeTab' data member:
            bindToQueryString(this, this, 'activeTab');
        }
    

    ```

`bindKeyToRouteOnCreate(vue: Vue, model: Model<ModelType>, routeParamName: string = 'id', keepQuery: boolean = false)`
    When `model` is created (i.e. its primary key becomes non-null), replace the current URL with one that includes uses primary key for the route parameter named by `routeParamName`.

    The query string will not be kept when the route is changed unless `true` is given to `keepQuery`.


    ``` ts

        import { bindKeyToRouteOnCreate } from 'coalesce-vue';

        // In the 'created' Vue lifecycle hook on a component:
        created() {
            if (this.id) {
                this.viewModel.$load(this.id);
            } else {
                bindKeyToRouteOnCreate(this, this.viewModel);
            }
        }


    ```

    .. note::
        The route will be replaced directly via the [HTML5 History API](https://developer.mozilla.org/en-US/docs/Web/API/History_API) such that vue-router_ will not observe the change as an actual route change, preventing the current view from being recreated if a path-based key is being used on the application's `<router-view>` component.


Advanced Model Functions
------------------------

The following functions exported from ``coalesce-vue`` can be used with your models. 

.. note::

    These functions are used to implement the [higher-order layers](/stacks/vue/overview.md) in the Vue stack. 

    While you're absolutely free to use them in your own code and can rely on their interface and behavior to remain consistent, you will find that you seldom need to use them directly - that's why we've split them into their own section here in the documentation.

`convertToModel(value: any, metadata: Value | ClassType): any`
    Given any JavaScript value `value`, convert it into a valid implementation of the value or type described by `metadata`.

    For metadata describing a primitive or primitive-like value, the input will be parsed into a valid implementation of the correct JavaScript type. For example, for `metadata` that describes a boolean, a string `"true"` will return a boolean `true`, and ISO 8601 date strings will result in a JavaScript `Date` object. 

    For metadata describing a type, the input object will be mutated into a valid implementation of the appropriate model interface. Missing properties will be set to null, and any descendent properties of the provided object will be recursively processed with `convertToModel`.

    If any values are encountered that are fundamentally incompatible with the requested type described by the metadata, an error will be thrown.

`mapToModel(value: any, metadata: Value | ClassType): any`
    Performs the same operations as `convertToModel`, except that any objects encountered will not be mutated - instead, a new object or array will always be created.

`mapToDto(value: any, metadata: Value | ClassType): any`
    Maps the input to a representation suitable for JSON serialization.

    Will not serialize child objects or collections whose metadata includes `dontSerialize`. Will only recurse to a maximum depth of 3.

.. _VueModelDisplayFunctions:

`modelDisplay(model: Model, options?: DisplayOptions): string` 
    Returns a string representing the `model` suitable for display in a user interface.

    Uses the `displayProp` defined on the object's metadata. If no `displayProp` is defined, the object will be displayed as JSON. The display prop on a model can be defined in C# with [[ListText]](/modeling/model-components/attributes/list-text.md).

    See [DisplayOptions](/stacks/vue/layers/models.md) for available options.

`propDisplay(model: Model, prop: Property | string, options?: DisplayOptions): string`
    Returns a string representing the specified property of the given object suitable for display in a user interface.

    The property can either be a string, representing one of the model's properties, or the actual `Property` metadata object of the property.

    See [DisplayOptions](/stacks/vue/layers/models.md) for available options.
    
`valueDisplay(value: any, metadata: Value, options?: DisplayOptions): string`
    Returns a string representing the given value (described by the given metadata).

    See [DisplayOptions](/stacks/vue/layers/models.md) for available options.


.. _DisplayOptions:

DisplayOptions
--------------

The following options are available to functions in coalesce-vue that render a value or object for display:

`format`
    Options to be used when formatting a date. One of:

    `string`
        A [UTS#35](http://unicode.org/reports/tr35/tr35-dates.html#Date_Format_Patterns) date format string, to be passed to [date-fns's format function](https://date-fns.org/docs/format).

        Defaults to `"M/d/yyyy"` for date-only dates (specified with [[DateType]](/modeling/model-components/attributes/date-type.md)), or `"M/d/yyyy h:mm:ss aaa"` otherwise. 

    `{ distance: true; addSuffix?: boolean; includeSeconds?: boolean; }`
        Options to be passed to [date-fns's formatDistanceToNow function](https://date-fns.org/docs/formatDistanceToNow).

        .. note::
            Values rendered with `formatDistanceToNow` function into a Vue component will not automatically be updated in realtime. If this is needed, you should use a strategy like using a [key](https://vuejs.org/v2/api/#key) that you periodically update to force a re-render.

`collection: { enumeratedItemsMax?: number, enumeratedItemsSeparator?: string }`
    Options to be used when formatting a collection.
