# Vue Model Layer

<!-- MARKER:summary -->

The model layer, generated as `models.g.ts`, contains a set of TypeScript interfaces that represent each client-exposed type in your data model. Each interface contains all the [Properties](/modeling/model-components/properties.md) of that type, as well as a `$metadata` property that references the [metadata](/stacks/vue/layers/metadata.md) object for that type. Enums and [Data Sources](/modeling/model-components/data-sources.md) are also represented in the model layer.

<!-- MARKER:summary-end -->

The model layer also includes a TypeScript class for each type that can be used to easily instantiate a valid implementation of its corresponding interface. However, it is not necessary for the classes to be used, and all parts of Coalesce that interact with the model layer don't perform any `instanceof` checks against models - the `$metadata` property is used to determine type identity.


## Concepts 

The model layer is fairly simple - the only main concept it introduces on top of the [Metadata Layer](/stacks/vue/layers/metadata.md) is the notion of interfaces and enums that mirror the C# types in your data model. As with the [Metadata Layer](/stacks/vue/layers/metadata.md), the [source code of coalesce-vue](https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/model.ts) is a great documentation supplement to this page.

### Model

An interface describing an instance of a class type from your application's data model. All Model interfaces contain members for all the [Properties](/modeling/model-components/properties.md) of that type, as well as a `$metadata` property that references the metadata object for that type.



### DataSource

A class-based representation of a [Data Source](/modeling/model-components/data-sources.md) containing properties for any of the [Custom Parameters](/modeling/model-components/data-sources.md#custom-parameters) of the data source, as well as a `$metadata` property that references the metadata object for the data source.

Data sources are generated as concrete classes in a namespace named `DataSources` that is nested inside a namespace named after their parent model type. For example:

``` ts
import { Person } from '@/models.g'

const dataSource = new Person.DataSources.NamesStartingWith;
dataSource.startsWith = "A";
// Provide the dataSource to an API Client or a ViewModel...
```

## Model Functions

The following functions exported from ``coalesce-vue`` can be used with your Models or View Models when building custom pages or components.

### useBindToQueryString() {#usebindtoquerystring}

Binds a value on an object, or the value of a ref, to the query string.

- **Type**

  ```ts
  function useBindToQueryString(obj: {}, key: string, options?: BindToQueryStringOptions): void
  function useBindToQueryString(ref: Ref<any>, queryKey: string): void
  function useBindToQueryString(ref: Ref<any>, options: BindToQueryStringOptions): void

  interface BindToQueryStringOptions {
    mode?: 'push' | 'replace'
    parse?: (value: string) => any
    stringify?: (value: any) => string
  }
  ```

- **Details**

  When the object's value changes, the query string will be updated using [vue-router](https://router.vuejs.org/). When the query string changes, the object's value will be updated.

  The query string will be updated using either `router.push` or `router.replace` depending on the value of parameter `mode`.

  If the query string contains a value when this is called, the object will be updated with that value immediately.

  If the object being bound to has `$metadata`, information from that metadata will be used to serialize and parse values to and from the query string. Otherwise, the `stringify` option (default: `String(value)`) will be used to serialize the value, and the `parse` option (if provided) will be used to parse the value from the query string.

- **Example**

  <CodeTabs name="vue">
  <template #options>

  ```ts
  import { bindToQueryString } from 'coalesce-vue';

  // In the 'created' Vue lifecycle hook on a component:
  created() {
    // Bind pagination information to the query string:
    bindToQueryString(this, this.listViewModel.$params, 'pageSize', { parse: parseInt });

    // Assuming the component has an 'activeTab' data member:
    bindToQueryString(this, this, 'activeTab');
  }
  ```

  </template>
  <template #setup>

  ```ts
  import { useBindToQueryString } from 'coalesce-vue';

  // Bind pagination information to the query string:
  const list = new PersonListViewModel();
  useBindToQueryString(list.$params, 'pageSize', { parse: parseInt });

  const activeTab = ref("1")
  useBindToQueryString(activeTab, 'activeTab');
  ```

  </template>
  </CodeTabs>


### useBindKeyToRouteOnCreate() {#usebindkeytorouteoncreate}

Replace the current URL with one that includes the primary key when a model is created.

- **Type**

  ```ts
  function useBindKeyToRouteOnCreate(
    model: Model<ModelType>, 
    routeParamName: string = 'id', 
    keepQuery: boolean = false
  ): void
  ```

- **Details**

  When `model` is created (i.e. its primary key becomes non-null), replace the current URL with one that includes the primary key for the route parameter named by `routeParamName`.

  The query string will not be kept when the route is changed unless `true` is given to `keepQuery`.

  The route will be replaced directly via the [HTML5 History API](https://developer.mozilla.org/en-US/docs/Web/API/History_API) such that [vue-router](https://router.vuejs.org/) will not observe the change as an actual route change, preventing the current view from being recreated if a path-based key is being used on the application's `<router-view>` component.

- **Example**

  First, define your router with a route that has an optional ID parameter:

  ```ts
  // router.ts
  const router = createRouter({
    routes: [
      {
        path: '/person/:id?',
        component: () => import('@/views/Person.vue'),
        props: true
      }
    ]
  })
  ```

  Then use `useBindKeyToRouteOnCreate` in your component:

  <CodeTabs name="vue">
  <template #options>

  ```ts
  // views/Person.vue
  import { bindKeyToRouteOnCreate } from 'coalesce-vue';

  // In the 'created' Vue lifecycle hook on a component:
  created() {
    if (this.id) {
      this.person.$load(this.id);
    } else {
      // When a new person is saved, the URL will change from:
      // /person → /person/123 (where 123 is the new person's ID)
      bindKeyToRouteOnCreate(this, this.person);
    }
  }
  ```

  </template>
  <template #setup>

  ```ts
  // views/Person.vue
  import { useBindKeyToRouteOnCreate } from 'coalesce-vue';

  const props = defineProps<{id?: number}>();
  const person = new PersonViewModel();
  
  if (props.id) {
    person.$load(props.id);
  } else {
    // When a new person is saved, the URL will change from:
    // /person → /person/123 (where 123 is the new person's ID)
    useBindKeyToRouteOnCreate(person);
  }
  ```

  </template>
  </CodeTabs>


## Advanced Model Functions

The following functions exported from ``coalesce-vue`` can be used with your models. 

::: tip Note
These functions are used to implement the [higher-order layers](/stacks/vue/overview.md) in the Vue stack. 

While you're absolutely free to use them in your own code and can rely on their interface and behavior to remain consistent, you will find that you seldom need to use them directly - that's why we've split them into their own section here in the documentation.
:::

### convertToModel() {#converttomodel}

Convert any JavaScript value into a valid implementation of the value or type described by metadata.

- **Type**

  ```ts
  function convertToModel(value: any, metadata: Value | ClassType): any
  ```

- **Details**

  Given any JavaScript value `value`, convert it into a valid implementation of the value or type described by `metadata`.

  For metadata describing a primitive or primitive-like value, the input will be parsed into a valid implementation of the correct JavaScript type. For example, for `metadata` that describes a boolean, a string `"true"` will return a boolean `true`, and ISO 8601 date strings will result in a JavaScript `Date` object. 

  For metadata describing a type, the input object will be mutated into a valid implementation of the appropriate model interface. Missing properties will be set to null, and any descendent properties of the provided object will be recursively processed with `convertToModel`.

  If any values are encountered that are fundamentally incompatible with the requested type described by the metadata, an error will be thrown.

### mapToModel() {#maptomodel}

Create a new object that is a valid implementation of the value or type described by metadata.

- **Type**

  ```ts
  function mapToModel(value: any, metadata: Value | ClassType): any
  ```

- **Details**

  Performs the same operations as `convertToModel`, except that any objects encountered will not be mutated - instead, a new object or array will always be created.

### mapToDto() {#maptodto}

Map a value to a representation suitable for JSON serialization.

- **Type**

  ```ts
  function mapToDto(value: any, metadata: Value | ClassType): any
  ```

- **Details**

  Maps the input to a representation suitable for JSON serialization.

  Will not serialize child objects or collections whose metadata includes `dontSerialize`. Will only recurse to a maximum depth of 3.

### modelDisplay() {#modeldisplay}

Return a string representing a model suitable for display in a user interface.

- **Type**

  ```ts
  function modelDisplay(model: Model, options?: DisplayOptions): string
  ```

- **Details**

  Returns a string representing the `model` suitable for display in a user interface.

  Uses the `displayProp` defined on the object's metadata. If no `displayProp` is defined, the object will be displayed as JSON. The display prop on a model can be defined in C# with [[ListText]](/modeling/model-components/attributes/list-text.md).

  See [DisplayOptions](#displayoptions) for available options.

### propDisplay() {#propdisplay}

Return a string representing a property of a model suitable for display.

- **Type**

  ```ts
  function propDisplay(model: Model, prop: Property | string, options?: DisplayOptions): string
  ```

- **Details**

  Returns a string representing the specified property of the given object suitable for display in a user interface.

  The property can either be a string, representing one of the model's properties, or the actual `Property` metadata object of the property.

  See [DisplayOptions](#displayoptions) for available options.

### valueDisplay() {#valuedisplay}

Return a string representing a value suitable for display.

- **Type**

  ```ts
  function valueDisplay(value: any, metadata: Value, options?: DisplayOptions): string
  ```

- **Details**

  Returns a string representing the given value (described by the given metadata).

  See [DisplayOptions](#displayoptions) for available options.


## DisplayOptions

The following options are available to functions in coalesce-vue that render a value or object for display:


@[import-md "start":"export interface DisplayOptions", "end":"\n}\n", "prepend":"``` ts", "append":"```"](../../../../src/coalesce-vue/src/model.ts)

::: tip Note
Dates rendered with the `formatDistanceToNow` function into a Vue component will not automatically be updated in realtime. If this is needed, you should use a strategy like using a [key](https://vuejs.org/api/built-in-special-attributes.html#key) that you periodically update to force a re-render.
:::


## Time Zones

In Coalesce Vue, all `DateTimeOffset`-based properties, for both inputs and display-only contexts, are by default formatted into the user's computer's system time zone. This is largely just a consequence of how the JavaScript Date type works. However, this behavior can be overridden by configuring a global default timezone, or by providing a time zone name to individual usages.

Fields with a type of `DateTime` are agnostic to time zone and UTC offset and so are not subject to any of the following rules.

For comprehensive information about working with dates and timezones, see [Working with Dates](/topics/working-with-dates.md).

### setDefaultTimeZone() {#setdefaulttimezone}

Set the default time zone used by Coalesce.

- **Type**

  ```ts
  function setDefaultTimeZone(timeZoneName: string | null): void
  ```

- **Details**

  Sets the default time zone used by Coalesce. The time zone should be an [IANA Time Zone Database](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) name, e.g. `"America/Los_Angeles"`.

  The time zone provided here is used in the following ways:
  - It will be used as `DisplayOptions.format.timeZone` if no other value was provided for this option. This is used by functions [modelDisplay](#modeldisplay), [propDisplay](#propdisplay), and [valueDisplay](#valuedisplay), as well as the [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) component.
  - It will be used by [c-datetime-picker](/stacks/vue/coalesce-vue-vuetify/components/c-datetime-picker.md), used to both interpret the user input and display the selected date. This can also be set on individual component usages via the `timeZone` prop.
  - It will be used when serializing DateTimeOffset fields into JSON DTOs, representing the ISO 8601 date string in the specified time zone rather than in the user's computer's system time zone.

### getDefaultTimeZone() {#getdefaulttimezone}

Get the current configured default time zone.

- **Type**

  ```ts
  function getDefaultTimeZone(): string | null
  ```

- **Details**

  Returns the current configured default time zone. Default is `null`, falling back on the user's computer's system time zone.