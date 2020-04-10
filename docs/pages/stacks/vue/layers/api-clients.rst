

API Client Layer
================

The API client layer, generated as `api-clients.g.ts`, exports a class for each API controller that was generated for your data model. These classes are stateless, and provide one method for each API endpoint. This includes both the standard set of endpoints created for :ref:`EntityModels` and :ref:`CustomDTOs`, as well as any custom :ref:`ModelMethods` on the aforementioned types, as well as any methods on your :ref:`Services`.

The API clients provided by Coalesce are based on `axios <https://github.com/axios/axios>`_. All API clients used a shared axios instance, exported from `coalesce-vue` as :ts:`AxiosClient`. This instance can be used to configure all HTTP requests made by Coalesce, including things like attaching `interceptors <https://github.com/axios/axios#interceptors>`_ to modify the requests being made, or configuring `defaults <https://github.com/axios/axios#config-defaults>`_.

As with all the layers, the `source code of coalesce-vue <https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/api-client.ts>`_ is also a great supplement to this documentation.

Concepts 
--------

API Client
    A class, generated for each controller-backed type in your data model as :ts:`<ModelName>ApiClient` and exported from `api-clients.g.ts` containing one method for each API endpoint.

    Each method on the API client takes in the regular parameters of the method as you would expect, as well as an optional :ts:`AxiosRequestConfig` parameter at the end that can be used to provide additional configuration for the single request, if needed.

    For the methods that correspond to the standard set of CRUD endpoints that Coalesce provides (``get``, ``list``, ``count``, ``save``, ``delete``), an additional parameter :ts:`parameters` is available that accepts the set of :ref:`DataSourceStandardParameters` appropriate for the endpoint.

    Each method returns a :ts:`Promise<AxiosResponse<TApiResult>>` where :ts:`TApiResult` is either :ts:`ItemResult`, :ts:`ItemResult<T>`, or :ts:`ListResult<T>`, depending on the return type of the API endpoint. :ts:`AxiosResponse` is the `response object from axios <https://github.com/axios/axios#response-schema>`_, containing the :ts:`TApiResult` in its :ts:`data` property, as well as other properties like :ts:`headers`.

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


Creating an API Caller
......................

API Callers can be created with the :ts:`$makeCaller` method of an API Client. During typical development, it is unlikely that you'll need to make a custom API Caller - the ones created for you on the generated :ref:`VueViewModels` will usually suffice. However, creating your own can allow for some more advanced functionality. Some examples:

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

A caller that has an args object that can be bound to:

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

The first parameter, :ts:`resultType`, can either be one of :ts:`"item"` or :ts:`"list"`, indicating whether the method returns a :csharp:`ItemResult` or :csharp:`ListResult` (examples #1 and #3 above). It can also be a function which accepts the set of method metadata for the API Client and which returns the specific method metadata (example #2 above), or it can be a direct reference to specific method metadata.


.. warning::

    This page is a work in progress and is not yet complete!