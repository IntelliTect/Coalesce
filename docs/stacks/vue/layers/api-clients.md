# API Client Layer

<!-- MARKER:summary -->

The API client layer, generated as `api-clients.g.ts`, exports a class for each API controller that was generated for your data model. These classes are stateless and provide one method for each API endpoint. This includes both the standard set of endpoints created for [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md), as well as any custom [Methods](/modeling/model-components/methods.md) on the aforementioned types, as well as any methods on your [Services](/modeling/model-types/services.md).

<!-- MARKER:summary-end -->

The API clients provided by Coalesce are based on [axios](https://github.com/axios/axios). All API clients used a shared axios instance, exported from `coalesce-vue` as `AxiosClient`. This instance can be used to configure all HTTP requests made by Coalesce, including things like attaching [interceptors](https://axios-http.com/docs/interceptors) to modify the requests being made, or configuring [defaults](https://axios-http.com/docs/config_defaults).

As with all the layers, the [source code of coalesce-vue](https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/api-client.ts) is also a great supplement to this documentation.

[[toc]]

## Concepts 

### API Client
    
A class, generated for each controller-backed type in your data model as `<ModelName>ApiClient` and exported from `api-clients.g.ts` containing one method for each API endpoint.

Each method on the API client takes in the regular parameters of the method as you would expect, as well as an optional `AxiosRequestConfig` parameter at the end that can be used to provide additional configuration for the single request, if needed.

For the methods that correspond to the standard set of CRUD endpoints that Coalesce provides (``get``, ``list``, ``count``, ``save``, ``delete``), an additional parameter `parameters` is available that accepts the set of [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters) appropriate for the endpoint.

Each method returns a `Promise<AxiosResponse<TApiResult>>` where `TApiResult` is either `ItemResult`, `ItemResult<T>`, or `ListResult<T>`, depending on the return type of the API endpoint. `AxiosResponse` is the [response object from axios](https://axios-http.com/docs/res_schema), containing the `TApiResult` in its `data` property, as well as other properties like `headers`. The returned type `T` is automatically converted into valid [Model implementations](/stacks/vue/layers/models.md) for you.

### API Callers/API States

A stateful function for invoking an API endpoint, created with the `$makeCaller` function on an API Client. API Callers provide a wide array of functionality that is useful for working with API endpoints that are utilized by a user interface.

Because they are such an integral part of the overall picture of `coalesce-vue`, they have [their own section below](/stacks/vue/layers/api-clients.md) where they are explained in much greater detail.


## API Callers

API Callers (typed with the name `ApiState` in `coalesce-vue`, sometimes also referred to as "loaders" or "invokers") are stateful functions for invoking an API endpoint, created with the `$makeCaller` function on an API Client. A summary of features:

#### Endpoint Invocation
Each API Caller is itself a function, so it can be invoked to trigger an API request to the server.
#### State management
API Callers contain properties about the last request made, including things like ``wasSuccessful``, ``isLoading``, ``result``, and more.
#### Concurrency Management
Using `setConcurrency(mode)`, you can configure how each individual caller handles what happens when multiple requests are made simultaneously
#### Argument Binding
API Callers can be created so that they have an `args` object that can be bound to, using `.invokeWithArgs()` to make a request using those arguments as the API endpoint's parameters. The API Callers created for the [ViewModel Layer](/stacks/vue/layers/viewmodels.md) are all created this way.


### Creating and Invoking an API Caller

API Callers can be created with the `$makeCaller` method of an API Client. The way in which it was created affects how it is invoked, as the parameters that the caller accepts are defined when it is created. 

::: tip
During typical development, it is unlikely that you'll need to make a custom API Caller - the ones created for you on the generated [ViewModel Layer](/stacks/vue/layers/viewmodels.md) will usually suffice. However, creating your own can allow for some more advanced functionality.
:::

Some examples:

``` ts
// Preamble for all the examples below:
import { PersonApiClient } from '@/api-clients.g';
const client = new PersonApiClient;
```

A caller that takes no additional parameters:

``` ts
const caller = client.$makeCaller(
    "item", 
    c => c.namesStartingWith("A")
);

await caller();
console.log(caller.result)
```

A caller that takes custom parameters:

``` ts
const caller = client.$makeCaller(
    methods => methods.namesStartingWith, 
    (c, str: string) => c.namesStartingWith(str)
);

await caller("Rob");
console.log(caller.result)
```

A caller that has an args object that can be bound to. This is how the generated API Callers in the [ViewModel Layer](/stacks/vue/layers/viewmodels.md) are created:

``` ts
const caller = client.$makeCaller("item", 
    // The parameter-based version is always required, even if it won't be used.
    (c, str: string) => c.namesStartingWith(str),
    // A function which creates a blank instance of the args object.
    // All props should be initialized (i.e. not undefined) to work with Vue's reactivity.
    () => ({str: null as string | null, }),
    // The function that accepts the args object and uses it:
    (c, args) => c.namesStartingWith(args.str)
);

caller.args.str = "Su";
await caller.invokeWithArgs();
console.log(caller.result)
```

A caller that performs multiple async operations:

``` ts
const deleteFirstNameStartingWith = client.$makeCaller(
    "item",
    async (c, str: string) => {
        const namesResult = await c.namesStartingWith(str)
        return await c.deletePersonByName(namesResult.data.object[0])
    }
);

await caller("Rob");
console.log(caller.result)
```

The first parameter, `resultType`, can either be one of `"item"` or `"list"`, indicating whether the method returns a `ItemResult` or `ListResult` (examples #1 and #3 above). It can also be a function which accepts the set of method metadata for the API Client and which returns the specific method metadata (example #2 above), or it can be a direct reference to a specific method metadata object.


### Properties

The following state properties can be found on API Caller instances. These properties are useful for binding to in a user interface to display errors, results, or indicators of progress.

#### All Callers

<Prop def="isLoading: boolean" lang="ts" />

True if there is currently a request pending for the API Caller.
    
<Prop def="wasSuccessful: boolean | null" lang="ts" />

A boolean indicating if the last request made was successful, or null if either no request has been made yet, or if a request has been made but has not yet completed.
    
<Prop def="message: string | null" lang="ts" />

An error message from the last request, if any. Will be set to null upon successful completion of a request.
    
<Prop def="hasResult: boolean" lang="ts" />

True if `result` is non-null. This prop is useful in performance-critical scenarios where checking `result` directly will cause an overabundance of re-renders in high-churn scenarios.

<Prop def="args: {}" lang="ts" />

Holds an object for the arguments of the function, and will be used if the caller is invoked with its `invokeWithArgs()` method. Useful for binding the arguments of a caller to inputs in a user interface.

Only exists if the caller was created with the option of being invoked with an args object as described in the sections above.

<Prop def="get url(): string" lang="ts" />

Returns the URL for the method's HTTP endpoint. Any parameters are sourced from the `args` object. Useful for binding file-returning HTTP GET methods directly to `image` or `video` HTML elements.

Only exists if the caller was created with the option of being invoked with an args object as described in the sections above.
    

#### ItemResult-based Callers

<Prop def="result: T | null" lang="ts" id="member-result-item" />

The principal data returned by the previous request. Will be set to null if the last response received returned no data (e.g. if the response was an error response)

<Prop def="validationIssues: ValidationIssue[] | null" lang="ts" />

Any validation issues returned by the previous request. This is never populated automatically by Coalesce, and is therefore is only used if you have written custom code to populate it in your [Behaviors](/modeling/model-components/behaviors.md) or [Methods](/modeling/model-components/methods.md).

#### ListResult-based Callers

<Prop def="result: Array<T> | null" lang="ts" id="member-result-list" />

The principal data returned by the previous request. Will be set to null if the last response received returned no data (e.g. if the response was an error response).


<Prop def="page, pageSize, pageCount, totalCount: number | null" lang="ts" id="members-pagination" />

Properties which contain the pagination information returned by the previous request.




### Concurrency Mode

<Prop def="setConcurrency(mode: 'disallow' | 'debounce' | 'cancel' | 'allow')" lang="ts" />

API callers have a `setConcurrency` method that allows you to customize how they behave when additional invocations are performed when there is already a request pending. There are four options available, with `"disallow"` being the default:

##### `"disallow"`

The default behavior - simply throws an error for any secondary invocations.

::: tip Note
Having `"disallow"` as the default prevents the unexpected behavior that can happen in a number of ways with the other modes:

- For requests that are performing data-mutating actions on the server, all other concurrency modes could lead to an unexpected end state of the data due to requests either being abandoned, cancelled, or potentially happening out-of-order.
- Throwing errors for multiple concurrent requests quickly surfaces issues during development where concurrent requests are not being correctly guarded against in a user interface - e.g. not disabling a "Save" or "Submit" button while the request is pending, which would otherwise lead to double-posts.
:::

##### `"debounce"`

When a secondary invocation is performed, enqueue it after the current pending invocation completes.

If additional invocations are performed while there is already an invocation enqueued and waiting, the already-enqueued invocation is abandoned and replaced by the most recent invocation attempt. The promise of the abandoned invocation will be resolved with `undefined` (it is NOT rejected).

##### `"cancel"`

When a secondary invocation is performed, cancel the current pending invocation. 

This completely aborts the request, propagating all the way back to the server where cancellation can be observed with [HttpContext.RequestAborted](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestaborted?view=aspnetcore-3.1). The promise of the cancelled invocation will be resolved with `undefined` (it is NOT rejected).

##### `"allow"`

When a secondary invocation is performed, always continue normally, sending the request to the server.

The state of the properties on the caller at any time will reflect the most recent response received from the server, which is never guaranteed to correlate with the most recent request made to the server - that is, requests are not guaranteed to complete in the order they were made. In particular, the `isLoading` property will be `false` after the first response comes back, even if the second response has not yet been received.

::: warning
For the reasons outlined above, it is generally not recommended to use `"allow"` unless you fully understand the drawbacks. This mode mirrors the legacy behavior of the Knockout stack for Coalesce.
:::


### Response Caching

Response caching on API Callers is a feature that will save API responses to persistent storage (`sessionStorage` or `localStorage`). The next time a matching request is made, the `result` property of the API Caller will be populated with that saved response, allowing for a faster time to interactivity and reduced repaints and shifting of elements as initial data loads after a page navigation. It does not _prevent_ any HTTP requests from being made, and does not affect the `Promise` returned from `invoke` or `invokeWithArgs`.

Common use cases include:
- Site-wide status or alert messages
- Server-provided configuration
- Dashboard data, like statistics or graphs

When a cached response is loaded, `result` is populated with that response's data, `wasSuccessful` and `hasResult` are set to `true`, and `onFulfilled` callbacks are invoked.

<Prop def="useResponseCaching(configuration?: ResponseCachingConfiguration | false)" lang="ts" />

Enables response caching on the API Caller. Only [HTTP GET methods](/modeling/model-components/attributes/controller-action.md) are supported, and [file-returning methods](/modeling/model-components/methods.md#file-downloads) are not supported. Call with `false` to disable caching after it was previously enabled. The available options are as follows:

@[import-md "start":"export type ResponseCachingConfiguration", "end":"\n};\n", "prepend":"``` ts", "append":"```"](../../../../src/coalesce-vue/src/api-client.ts)

### Other Methods

API Callers have a few other methods available as well:

<Prop def="cancel(): void" lang="ts" />

Manually cancel the current request. The promise of the cancelled invocation will be resolved with `undefined` (it is NOT rejected). If using concurrency mode `"allow"`, only the most recent invocation is cancelled.


<Prop def="onFulfilled((state: TInvoker) => void | Promise<any>): void" lang="ts" />

Add a callback to the caller to be invoked when a success response is received from the server.
If a promise is returned, this promise will be awaited and will delay the setting of the `isLoading` prop to `false` until it completes.


<Prop def="onRejected((state: TInvoker) => void | Promise<any>): void" lang="ts" />

Add a callback to the caller to be invoked when a failure response is received from the server.
If a promise is returned, this promise will be awaited and will delay the setting of the `isLoading` prop to `false` until it completes.


<Prop def="invoke(...args: TArgs)" lang="ts" />

The invoke function is a reference from the caller to itself. In other words, `caller.invoke === caller`. This mirrors the syntax of the Knockout generated method classes.


<Prop def="invokeWithArgs(args?: {})" lang="ts" />

If called a parameter, that parameter will be used as the args object. Otherwise, `caller.args` will be used.

Only exists if the caller was created with the option of being invoked with an args object as described in the sections above.


<Prop def="getResultObjectUrl(vue: Vue): string | undefined" lang="ts" />

If the method returns a file, this method will return an [Object URL](https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL) representing the value of the `result` prop. Requires a `Vue` instance to be provided in order to manage the lifecycle of the URL, since object URLs must be manually released to avoid memory leaks. When the provided Vue component is destroyed, the object URL will be destroyed.

Only exists if the caller was created with the option of being invoked with an args object as described in the sections above.
