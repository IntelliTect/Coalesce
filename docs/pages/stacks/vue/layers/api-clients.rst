.. _VueApiClients:

API Client Layer
================

The API client layer, generated as `api-clients.g.ts`, exports a class for each API controller that was generated for your data model. These classes are stateless, and provide one method for each API endpoint. This includes both the standard set of endpoints created for :ref:`EntityModels` and :ref:`CustomDTOs`, as well as any custom :ref:`ModelMethods` on the aforementioned types, as well as any methods on your :ref:`Services`.

The API clients provided by Coalesce are based on `axios <https://github.com/axios/axios>`_. All API clients used a shared axios instance, exported from `coalesce-vue` as :ts:`AxiosClient`. This instance can be used to configure all HTTP requests made by Coalesce, including things like attaching `interceptors <https://github.com/axios/axios#interceptors>`_ to modify the requests being made, or configuring `defaults <https://github.com/axios/axios#config-defaults>`_.

As with all the layers, the `source code of coalesce-vue <https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/api-client.ts>`_ is also a great supplement to this documentation.

.. contents:: Contents
    :local:

Concepts 
--------

API Client
    A class, generated for each controller-backed type in your data model as :ts:`<ModelName>ApiClient` and exported from `api-clients.g.ts` containing one method for each API endpoint.

    Each method on the API client takes in the regular parameters of the method as you would expect, as well as an optional :ts:`AxiosRequestConfig` parameter at the end that can be used to provide additional configuration for the single request, if needed.

    For the methods that correspond to the standard set of CRUD endpoints that Coalesce provides (``get``, ``list``, ``count``, ``save``, ``delete``), an additional parameter :ts:`parameters` is available that accepts the set of :ref:`DataSourceStandardParameters` appropriate for the endpoint.

    Each method returns a :ts:`Promise<AxiosResponse<TApiResult>>` where :ts:`TApiResult` is either :ts:`ItemResult`, :ts:`ItemResult<T>`, or :ts:`ListResult<T>`, depending on the return type of the API endpoint. :ts:`AxiosResponse` is the `response object from axios <https://github.com/axios/axios#response-schema>`_, containing the :ts:`TApiResult` in its :ts:`data` property, as well as other properties like :ts:`headers`. The returned type :ts:`T` is automatically converted into valid :ref:`Model implementations <VueModels>` for you.

API Callers/API States
    A stateful function for invoking an API endpoint, created with the :ts:`$makeCaller` function on an API Client. API Callers provide a wide array of functionality that is useful for working with API endpoints that are utilized by a user interface.

    Because they are such an integral part of the overall picture of `coalesce-vue`, they have :ref:`their own section below <VueApiCallers>` where they are explained in much greater detail.



.. _VueApiCallers: 

API Callers
-----------

API Callers (typed with the name :ts:`ApiState` in `coalesce-vue`, sometimes also referred to as API Invokers) are stateful functions for invoking an API endpoint, created with the :ts:`$makeCaller` function on an API Client. 

A summary of features:

Endpoint Invocation
    Each API Caller is itself a function, so it can be invoked to trigger an API request to the server.
State management
    API Callers contain properties about the last request made, including things like ``wasSuccessful``, ``isLoading``, ``result``, and more.
Concurrency Management
    Using :ts:`$setConcurrency(mode)`, you can configure how each individual caller handles what happens when multiple requests are made simultaneously
Argument Binding
    API Callers can be created so that they have an :ts:`args` object that can be bound to, using :ts:`.invokeWithArgs()` to make a request using those arguments as the API endpoint's parameters. The API Callers created for the :ref:`VueViewModels` are all created this way.


Creating and Invoking API Caller
................................

API Callers can be created with the :ts:`$makeCaller` method of an API Client. The way in which it was created affects how it is invoked, as the parameters that the caller accepts are defined when it is created. 

.. tip:: 

    During typical development, it is unlikely that you'll need to make a custom API Caller - the ones created for you on the generated :ref:`VueViewModels` will usually suffice. However, creating your own can allow for some more advanced functionality.

Some examples:

.. code-block:: typescript

    // Preamble for all the examples below:
    import { PersonApiClient } from '@/api-clients.g';
    const client = new PersonApiClient;

A caller that takes no additional parameters:

.. code-block:: typescript

    const caller = client.$makeCaller(
        "item", 
        c => c.namesStartingWith("A")
    );

    await caller();
    console.log(caller.result)

A caller that takes custom parameters:

.. code-block:: typescript

    const caller = client.$makeCaller(
        methods => methods.namesStartingWith, 
        (c, str: string) => c.namesStartingWith(str)
    );

    await caller("Rob");
    console.log(caller.result)

A caller that has an args object that can be bound to. This is how the generated API Callers in the :ref:`VueViewModels` are created:

.. code-block:: typescript

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

A caller that performs multiple async operations:

.. code-block:: typescript

    const deleteFirstNameStartingWith = client.$makeCaller(
        "item",
        async (c, str: string) => {
            const namesResult = await c.namesStartingWith(str)
            return await c.deletePersonByName(namesResult.data.object[0])
        }
    );

    await caller("Rob");
    console.log(caller.result)

The first parameter, :ts:`resultType`, can either be one of :ts:`"item"` or :ts:`"list"`, indicating whether the method returns a :csharp:`ItemResult` or :csharp:`ListResult` (examples #1 and #3 above). It can also be a function which accepts the set of method metadata for the API Client and which returns the specific method metadata (example #2 above), or it can be a direct reference to a specific method metadata object.


Properties
..........

The following state properties can be found on API Caller instances. These properties are useful for binding to in a user interface to display errors, results, or indicators of progress.

All Callers
'''''''''''

:ts:`isLoading: boolean`
    True if there is currently a request pending for the API Caller.
    
:ts:`wasSuccessful: boolean | null`
    A boolean indicating if the last request made was successful, or null if either no request has been made yet, or if a request has been made but has not yet completed.
    
:ts:`message: string | null`
    An error message from the last request, if any. Will be set to null upon successful completion of a request.
    
:ts:`hasResult: boolean`
    True if :ts:`result` is non-null. This prop is useful in performance-critical scenarios where checking :ts:`result` directly will cause an overabundance of re-renders in high-churn scenarios.

:ts:`args: {}`
    Holds an object for the arguments of the function, and will be used if the caller is invoked with its :ts:`invokeWithArgs()` method. Useful for binding the arguments of a caller to inputs in a user interface.

    Only exists if the caller was created with the option of being invoked with an args object as described in the sections above.

ItemResult-based Callers
''''''''''''''''''''''''

:ts:`result: T | null`
    The principal data returned by the previous request. Will be set to null if the last response received returned no data (e.g. if the response was an error response)

:ts:`validationIssues: ValidationIssue[] | null`
    Any validation issues returned by the previous request. This is never populated automatically by Coalesce, and is therefore is only used if you have written custom code to populate it in your :ref:`Behaviors` or :ref:`ModelMethods`.

ListResult-based Callers
''''''''''''''''''''''''

:ts:`result: Array<T> | null`
    The principal data returned by the previous request. Will be set to null if the last response received returned no data (e.g. if the response was an error response).

:ts:`page, pageSize, pageCount, totalCount: number | null`
    Properties which contain the pagination information returned by the previous request.

Concurrency Mode
................

API callers have a :ts:`setConcurrency(mode: string)` method that allows you to customize how they behave when additional invocations are performed when there is already a request pending. There are four options avaiable, with :ts:`"disallow"` being the default:

:ts:`"disallow"`
    The default behavior - simply throws an error for any secondary invocations.

:ts:`"debounce"`
    When a secondary invocation is performed, enqueue it after the current pending invocation completes.

    If additional invocations are performed while there is already an invocation enqueued and waiting, the already-enqueued invocation is abandoned and replaced by the most recent invocation attempt. The promise of the abandoned invocation will be resolved with :ts:`undefined` (it is NOT rejected).

:ts:`"cancel"`
    When a secondary invocation is performed, cancel the current pending invocation. 

    This completely aborts the request, propagating all the way back to the server where cancellation can be observed with `HttpContext.RequestAborted <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestaborted?view=aspnetcore-3.1>`_. The promise of the cancelled invocation will be resolved with :ts:`undefined` (it is NOT rejected).

:ts:`"allow"`
    When a secondary invocation is performed, always continue normally, sending the request to the server.

    The state of the properties on the caller at any time will reflect the most recent response received from the server, which is never guaranteed to correlate with the most recent request made to the server - that is, requests are not guaranteed to complete in the order they were made. In particular, the :ts:`isLoading` property will be :ts:`false` after the first response comes back, even if the second response has not yet been received.

    For these reasons, it is generally not recommended to use :ts:`"allow"` unless you fully understand the drawbacks. This mode mirrors the legacy behavior of the Knockout stack for Coalesce.

.. note::

    Having :ts:`"disallow"` as the default prevents the unexpected behavior that can happen in a number of ways with the other modes:
    
    - For requests that are performing data-mutating actions on the server, all other concurrency modes could lead to an unexpected end state of the data due to requests either being abandoned, cancelled, or potentially happening out-of-order.
    - Throwing errors for multiple concurrent requests quickly surfaces issues during development where such concurrent requests are not being correctly guarded against in a user interface - e.g. not disabling a "Save" or "Submit" button while the request is pending, which would otherwise lead to double-posts.

Other Methods
.............

API Callers have a few other methods available as well:

:ts:`cancel()`
    Manually cancel the current request. The promise of the cancelled invocation will be resolved with :ts:`undefined` (it is NOT rejected). If using concurrency mode :ts:`"allow"`, only the most recent invocation is cancelled.

:ts:`onFulfilled((state: TInvoker) => void | Promise<any>)`
    Add a callback to the caller to be invoked when a success response is received from the server.
    If a promise is returned, this promise will be awaited and will delay the setting of the :ts:`isLoading` prop to :ts:`false` until it completes.

:ts:`onRejected((state: TInvoker) => void | Promise<any>)`
    Add a callback to the caller to be invoked when a failure response is received from the server.
    If a promise is returned, this promise will be awaited and will delay the setting of the :ts:`isLoading` prop to :ts:`false` until it completes.

:ts:`invoke(...args: TArgs)`
    The invoke function is a reference from the caller to itself - that is, :ts:`caller.invoke === caller`. This mirrors the syntax of the Knockout generated method classes.

:ts:`invokeWithArgs(args?: {})`
    If called a parameter, that parameter will be used as the args object. Otherwise, :ts:`caller.args` will be used.

    Only exists if the caller was created with the option of being invoked with an args object as described in the sections above.
