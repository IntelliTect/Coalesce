# API Clients

<!-- MARKER:summary -->

Coalesce's generated TypeScript API clients provide a straightforward, stateless, strongly-typed interface for making HTTP requests to your backend APIs. The API clients are generated in `api-clients.g.ts` and export a class for each API-exposed type in your data model. These classes provide one method for each API endpoint, including both the standard set of endpoints created for [CRUD Models](/modeling/model-types/crud.md), as well as any custom [Methods](/modeling/model-components/methods.md) on the aforementioned types, as well as any methods on your [Services](/modeling/model-types/services.md).

<!-- MARKER:summary-end -->

The API clients provided by Coalesce are based on [axios](https://github.com/axios/axios). All API clients used a shared axios instance, exported from `coalesce-vue` as `AxiosClient`. This instance can be used to configure all HTTP requests made by Coalesce, including things like attaching [interceptors](https://axios-http.com/docs/interceptors) to modify the requests being made, or configuring [defaults](https://axios-http.com/docs/config_defaults).

## API Clients
    
API Clients are classes, generated for each controller-backed type in your data model as `<ModelName>ApiClient` and exported from `api-clients.g.ts` containing a method for each API endpoint.

Each method on the API client takes in the regular parameters of the method as you would expect, as well as an optional `AxiosRequestConfig` parameter at the end that can be used to provide additional configuration for the single request, if needed.

For the methods that correspond to the standard set of CRUD endpoints that Coalesce provides (``get``, ``list``, ``count``, ``save``, ``bulkSave``, ``delete``), an additional parameter `parameters` is available that accepts the set of [Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters) appropriate for the endpoint.

Each method returns a `Promise<AxiosResponse<TApiResult>>` where `TApiResult` is either `ItemResult`, `ItemResult<T>`, or `ListResult<T>`, depending on the return type of the API endpoint. `AxiosResponse` is the [response object from axios](https://axios-http.com/docs/res_schema), containing the `TApiResult` in its `data` property, as well as other properties like `headers`. The returned type `T` is automatically converted into valid [Model implementations](/stacks/vue/layers/models.md) for you.

#### API Client Configuration Methods

API Clients also provide configuration methods that can be chained to modify the behavior of all requests made through that client instance:

### $useSimultaneousRequestCaching() {#usesimultaneousrequestcaching}

Enables simultaneous request caching for all requests made through this API Client instance.

- **Type**

  ```ts
  $useSimultaneousRequestCaching(): ApiClient
  ```

- **Details**

  When multiple identical GET requests are made at the same time, they will be handled with the same AJAX request instead of making duplicate HTTP calls. This method returns the API Client instance to allow for method chaining.

- **Example**

  ```ts
  const client = new PersonApiClient().$useSimultaneousRequestCaching();
  ```
  ```ts
  const list = new PersonListViewModel()
  list.$apiClient.$useSimultaneousRequestCaching();
  ```

### $useRefResponse() {#userefresponse}

Enables or disables reference response handling for all requests made through this API Client instance.

- **Type**

  ```ts
  $useRefResponse(enable: boolean = true): ApiClient
  ```

- **Details**

  When enabled, requests will include the `Accept: application/json+ref` header to use System.Text.Json reference preservation handling, which can significantly reduce response sizes by deduplicating identical objects. This method returns the API Client instance to allow for method chaining.

- **Example**

  ```ts
  const client = new PersonApiClient().$useRefResponse();
  ```
  ```ts
  const list = new PersonListViewModel()
  list.$apiClient.$useRefResponse();
  ```

### $makeCaller() {#makecaller}

Creates a new [API Caller](#api-callers) for invoking an API endpoint with optional parameter binding and state management.

::: tip
During typical development, it is unlikely that you'll need to make a custom API Caller - the ones created for you on the generated [ViewModels](/stacks/vue/layers/viewmodels.md) will usually suffice. However, creating your own can allow for some more advanced functionality.
:::

- **Type**

  ```ts
  $makeCaller<TResult>(
    resultType: "item" | "list" | MethodFunction,
    methodFunction: (...args: any[]) => Promise<AxiosResponse<TResult>>
  ): ApiCaller<TResult>

  $makeCaller<TResult, TArgs>(
    resultType: "item" | "list" | MethodFunction,
    methodFunction: (...args: any[]) => Promise<AxiosResponse<TResult>>,
    argsFactory: () => TArgs,
    argsMethodFunction: (client: ApiClient, args: TArgs) => Promise<AxiosResponse<TResult>>
  ): ApiCaller<TResult> & { args: TArgs }
  ```

- **Details**

  Creates an API Caller that provides state management and concurrency control for API endpoint invocations. The first parameter `resultType` can be "item", "list", or a function reference to method metadata. The second parameter is the method that will be invoked.

  For callers that need argument binding, provide an `argsFactory` function that creates a blank args object, and an `argsMethodFunction` that accepts the args object.



- **Example**

  A caller that takes no additional parameters:

  ```ts
  import { PersonApiClient } from '@/api-clients.g';
  const client = new PersonApiClient();

  const caller = client.$makeCaller(
      "item", 
      c => c.namesStartingWith("A")
  );

  await caller();
  console.log(caller.result)
  ```

  A caller that takes custom parameters:

  ```ts
  const caller = client.$makeCaller(
      methods => methods.namesStartingWith, 
      (c, str: string) => c.namesStartingWith(str)
  );

  await caller("Rob");
  console.log(caller.result)
  ```

  A caller that has an args object that can be bound to:

  ```ts
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

  ```ts
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

## API Callers

API Callers (typed with the name `ApiState` in `coalesce-vue`, sometimes also referred to as "loaders") are stateful functions for invoking an API endpoint, created with the `$makeCaller` function on an API Client. A summary of features:

- **Endpoint Invocation**

  Each API Caller is itself a function, so it can be invoked to trigger an API request to the server.
  
- **State management**

  API Callers contain properties about the last request made, including things like ``wasSuccessful``, ``isLoading``, ``result``, and more. [Read More](#isloading).

- **Concurrency Management**

  Using `setConcurrency(mode)`, you can configure how each individual caller handles what happens when multiple requests are made simultaneously. [Read More](#setconcurrency).

- **Argument Binding**

  API Callers can be created so that they have an `args` object that can be bound to, using `.invokeWithArgs()` to make a request using those arguments as the API endpoint's parameters. The API Callers created for the generated [ViewModels](/stacks/vue/layers/viewmodels.md) are all created this way.

The following are all the properties and methods available on an API Caller:

### isLoading {#isloading}

```ts class-wrap
readonly isLoading: boolean
```

True if a request is currently outstanding and waiting for a response from the server.

### wasSuccessful {#wassuccessful}

```ts class-wrap
readonly wasSuccessful: boolean | null
```

A boolean indicating if the last request was successful. `null` if no request has been made yet or if a request has been made but has not yet completed.

### message {#message}

```ts class-wrap
readonly message: string | null
```

A message from the last response. Typically an error message if the last request failed, but messages can also be provided with successful `ApiResult` responses in your [custom methods](/modeling/model-components/methods.md).

### hasResult {#hasresult}

```ts class-wrap
readonly hasResult: boolean
```

True if `result` is non-null. This is also true for void-returning endpoints that were successful.

### args {#args}

Holds an object for the arguments of the function, and will be used if the caller is invoked with its `invokeWithArgs()` method. Useful for binding the arguments of a caller to inputs in a user interface.

Only exists if the caller was created with an args object using the 4-parameter overload of [`$makeCaller()`](#makecaller).

### url {#url}

```ts class-wrap
readonly url: string
```

Returns the URL for the method's HTTP endpoint. Any parameters are sourced from the `args` object. Useful for binding file-returning HTTP GET methods directly to `image` or `video` HTML elements.

Only exists if the caller was created with an args object using the 4-parameter overload of [`$makeCaller()`](#makecaller).


### result {#result-item}

```ts class-wrap
// ItemResult callers
readonly result: T | null

// ListResult callers
readonly result: T[] | null
```

The principal data returned by last response. Will be set to null if the last response received returned no data (e.g. if the response was an error response)

### validationIssues {#validationissues}

```ts class-wrap
readonly validationIssues: ValidationIssue[] | null
```

ItemResult-based callers only. Any validation issues returned by the last response.


### page/pageSize/pageCount/totalCount {#pagination-properties}

```ts class-wrap
readonly page: number | null
readonly pageSize: number | null
readonly pageCount: number | null
readonly totalCount: number | null
```

ListResult-based callers only. Properties containing pagination information returned by the last response.


### setConcurrency() {#setconcurrency}

Configures how the API Caller handles multiple simultaneous invocations.

- **Type**

  ```ts
  setConcurrency(mode: 'disallow' | 'debounce' | 'cancel' | 'allow'): void
  ```

- **Details**

  API callers have a `setConcurrency` method that allows you to customize how they behave when additional invocations are performed when there is already a request pending. There are four options available, with `"disallow"` being the default:

  **`"disallow"`** (default): Simply throws an error for any secondary invocations.

  ::: tip Note
  Having `"disallow"` as the default prevents the unexpected behavior that can happen in a number of ways with the other modes:

  - For requests that are performing data-mutating actions on the server, all other concurrency modes could lead to an unexpected end state of the data due to requests either being abandoned, cancelled, or potentially happening out-of-order.
  - Throwing errors for multiple concurrent requests quickly surfaces issues during development where concurrent requests are not being correctly guarded against in a user interface - e.g. not disabling a "Save" or "Submit" button while the request is pending, which would otherwise lead to double-posts.
  :::

  **`"debounce"`**: When a secondary invocation is performed, enqueue it after the current pending invocation completes. If additional invocations are performed while there is already an invocation enqueued and waiting, the already-enqueued invocation is abandoned and replaced by the most recent invocation attempt. The promise of the abandoned invocation will be resolved with `undefined` (it is NOT rejected).

  **`"cancel"`**: When a secondary invocation is performed, cancel the current pending invocation. This completely aborts the request, propagating all the way back to the server where cancellation can be observed with [HttpContext.RequestAborted](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestaborted). The promise of the cancelled invocation will be resolved with `undefined` (it is NOT rejected).

  **`"allow"`**: When a secondary invocation is performed, always continue normally, sending the request to the server. The state of the properties on the caller at any time will reflect the most recent response received from the server, which is never guaranteed to correlate with the most recent request made to the server - that is, requests are not guaranteed to complete in the order they were made. In particular, the `isLoading` property will be `false` after the first response comes back, even if the second response has not yet been received.

  ::: warning
  For the reasons outlined above, it is generally not recommended to use `"allow"` unless you fully understand the drawbacks. This mode mirrors the legacy behavior of the Knockout stack for Coalesce.
  :::

- **Example**

  ```ts
  const caller = client.$makeCaller("item", c => c.getSomeData());
  caller.setConcurrency("debounce");
  ```


### useResponseCaching() {#useresponsecaching}

Enables response caching on the API Caller.

- **Type**

  ```ts
  useResponseCaching(configuration?: ResponseCachingConfiguration | false): void
  ```

  @[import-md "start":"export type ResponseCachingConfiguration", "end":"\n};\n", "prepend":"``` ts", "append":"```"](../../../../src/coalesce-vue/src/api-client.ts)

- **Details**

  Response caching on API Callers is a feature that will save API responses to persistent storage (`sessionStorage` or `localStorage`). The next time a matching request is made, the `result` property of the API Caller will be populated with that saved response, allowing for a faster time to interactivity and reduced repaints and shifting of elements as initial data loads after a page navigation. It does not _prevent_ any HTTP requests from being made, and does not affect the `Promise` returned from `invoke` or `invokeWithArgs`.

  Common use cases include:
  - Site-wide status or alert messages
  - Server-provided configuration
  - Dashboard data, like statistics or graphs

  When a cached response is loaded, `result` is populated with that response's data, `wasSuccessful` and `hasResult` are set to `true`, and `onFulfilled` callbacks are invoked.

  Only [HTTP GET methods](/modeling/model-components/attributes/controller-action.md) are supported, and [file-returning methods](/modeling/model-components/methods.md#file-downloads) are not supported. Call with `false` to disable caching after it was previously enabled.

- **Example**

  ```ts
  const caller = client.$makeCaller("item", c => c.getConfiguration());
  caller.useResponseCaching({
    storage: 'localStorage',
    duration: 3600000 // 1 hour
  });
  ```

### useSimultaneousRequestCaching() {#usesimultaneousrequestcaching-caller}

Enables simultaneous request caching on the API Caller.

- **Type**

  ```ts
  useSimultaneousRequestCaching(): void
  ```

- **Details**

  When multiple identical GET requests are made at the same time across all API Client instances, they will be handled with the same AJAX request instead of making duplicate HTTP calls to the server. This can improve performance and reduce server load when the same endpoint is called multiple times in quick succession.

  This feature only applies to HTTP GET methods and works across all instances of API clients and callers that have this feature enabled.

- **Example**

  ```ts
  const caller = client.$makeCaller("list", c => c.list());
  caller.useSimultaneousRequestCaching();
  ```

### useRefResponse() {#userefresponse-caller}

Enables or disables reference response handling on the API Caller.

- **Type**

  ```ts
  useRefResponse(enable: boolean = true): void
  ```

- **Details**

  When enabled, requests will include the `Accept: application/json+ref` header to use System.Text.Json reference preservation handling, which can significantly reduce response sizes by deduplicating identical objects in the response payload.

  This is particularly useful for endpoints that return complex object graphs with repeated references to the same entities. The resulting `Model` and `ViewModel` instances on the client will be automatically deduplicated.

  Call with `false` to disable reference response handling after it was previously enabled.

- **Example**

  ```ts
  const caller = client.$makeCaller("item", c => c.getWithNestedData(id));
  caller.useRefResponse();
  
  // To disable:
  caller.useRefResponse(false);
  ```

### cancel() {#cancel}

Manually cancels the current request.

- **Type**

  ```ts
  cancel(): void
  ```

- **Details**

  Manually cancel the current request. The promise of the cancelled invocation will be resolved with `undefined` (it is NOT rejected). If using concurrency mode `"allow"`, only the most recent invocation is cancelled.

### onFulfilled() {#onfulfilled}

Adds a callback to be invoked when a success response is received.

- **Type**

  ```ts
  onFulfilled(callback: (state: TInvoker) => void | Promise<any>): void
  ```

- **Details**

  Add a callback to the caller to be invoked when a success response is received from the server. If a promise is returned, this promise will be awaited and will delay the setting of the `isLoading` prop to `false` until it completes.

- **Example**

  ```ts
  const caller = client.$makeCaller("item", c => c.getData());
  caller.onFulfilled(state => {
    console.log('Request succeeded:', state.result);
  });
  ```

### onRejected() {#onrejected}

Adds a callback to be invoked when a failure response is received.

- **Type**

  ```ts
  onRejected(callback: (state: TInvoker) => void | Promise<any>): void
  ```

- **Details**

  Add a callback to the caller to be invoked when a failure response is received from the server. If a promise is returned, this promise will be awaited and will delay the setting of the `isLoading` prop to `false` until it completes.

- **Example**

  ```ts
  const caller = client.$makeCaller("item", c => c.getData());
  caller.onRejected(state => {
    console.error('Request failed:', state.message);
  });
  ```

### invoke() {#invoke}

Invokes the endpoint with provided arguments.

- **Type**

  ```ts
  invoke(...args: TArgs): Promise<AxiosResponse<TResult>>
  ```

- **Details**

  Invokes the endpoint with provided args. The invoke is a reference from the caller to itself. In other words, `caller.invoke === caller`.

### confirmInvoke() {#confirminvoke}

Invokes the endpoint with user confirmation.

- **Type**

  ```ts
  confirmInvoke(message: string, ...args: TArgs): Promise<AxiosResponse<TResult>>
  ```

- **Details**

  Similar to `invoke`, but prompts for confirmation from the user (via `window.confirm`) with the provided message.

- **Example**

  ```ts
  const deleteCaller = client.$makeCaller("item", (c, id: number) => c.delete(id));
  await deleteCaller.confirmInvoke("Are you sure you want to delete this item?", itemId);
  ```

### invokeWithArgs() {#invokewithargs}

Invokes the endpoint using the caller's args object.

- **Type**

  ```ts
  invokeWithArgs(args?: TArgs): Promise<AxiosResponse<TResult>>
  ```

- **Details**

  Invokes the endpoint with the specified args, defaulting to `caller.args` if the `args` parameter is not provided. Only exists if the caller was created with an args object using the 4-parameter overload of [`$makeCaller()`](#makecaller).

### confirmInvokeWithArgs() {#confirminvokewithargs}

Invokes the endpoint using the caller's args object with user confirmation.

- **Type**

  ```ts
  confirmInvokeWithArgs(message: string, args?: TArgs): Promise<AxiosResponse<TResult>>
  ```

- **Details**

  Similar to `invokeWithArgs`, but prompts for confirmation from the user (via `window.confirm`) with the provided message.

### getResultObjectUrl() {#getresultobjecturl}

Returns an Object URL for file-returning methods.

- **Type**

  ```ts
  getResultObjectUrl(vue?: Vue): string | undefined
  ```

- **Details**

  If the method returns a file, this method will return an [Object URL](https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL) representing the value of the `result` prop. 

  Accepts a `Vue` instance in order to manage the lifecycle of the URL, since object URLs must be manually released to avoid memory leaks. When the provided Vue component is destroyed, the object URL will be destroyed. If called inside the component template, the Vue instance can be acquired automatically.

  ::: tip Prefer using the `url` property
  For most use cases, it's preferable to use the [`url`](#url) property instead of `getResultObjectUrl()`. The `url` property provides the direct endpoint URL which can be used directly in HTML elements like `<img>` or `<video>` without needing to manage object URL lifecycle. Only use `getResultObjectUrl()` when you need to programmatically invoke the endpoint and then access the file result as a blob.
  :::

- **Example**

  ```ts
  import { PersonViewModel } from '@/viewmodels.g';

  const person = new PersonViewModel();
  const instance = getCurrentInstance();
  
  // Provides a result URL suitable for use in e.g. an <img> element 
  // for a programmatically invoked file result.
  const avatarUrl = computed(() => person.getAvatar.getResultObjectUrl(instance));
  
  // Load the avatar file
  person.getAvatar(person.personId);
  ```
