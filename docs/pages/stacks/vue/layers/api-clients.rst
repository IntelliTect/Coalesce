

API Client Layer
================

The API client layer, generated as `api-clients.g.ts`, exports a class for each API controller that was generated for your data model. These classes are stateless, and provide one method for each API endpoint. This includes both the standard set of endpoints created for :ref:`EntityModels` and :ref:`CustomDTOs`, as well as any custom :ref:`ModelMethods` on the aforementioned types, as well as any methods on your :ref:`Services`.

The API clients provided by Coalesce are based on `axios <https://github.com/axios/axios>`_. All API clients used a shared axios instance, exported from `coalesce-vue` as :ts:`AxiosClient`. This instance can be used to configure all HTTP requests made by Coalesce, including things like attaching `interceptors <https://github.com/axios/axios#interceptors>`_ to modify the requests being made, or configuring `defaults <https://github.com/axios/axios#config-defaults>`_.

Concepts 
--------

As with all layers :ref:`VueMetadata`, the `source code of coalesce-vue <https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/api-client.ts>`_ is also a great supplement to this documentation.

API Client
    A class, generated for each controller-backed type in your data model as :ts:`<ModelName>ApiClient` and exported from `api-clients.g.ts` containing one method for each API endpoint.

    Each method takes in one parameter for each 


.. warning::

    This page is a work in progress and is not yet complete!