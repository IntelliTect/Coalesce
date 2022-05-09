.. _c-admin-table-page:

c-admin-table-page
==================

.. MARKER:summary
    
A full-featured page for interacting with a :ref:`ListViewModel <VueListViewModels>`. Provides a :ref:`c-admin-table` and a :ref:`c-admin-methods` for the list. Designed to be routed to directly with vue-router_.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: vue

    // router.ts or main.ts

    import { CAdminTablePage } from 'coalesce-vue-vuetify';

    const router = new Router({
        // ...
        routes: [
            // ... other routes
            {
                path: '/admin/:type',
                name: 'coalesce-admin-list',
                component: CAdminTablePage,
                props: true,
            },
        ]
    })

Props
-----

`type: string`
    The PascalCase name of the type to be listed.

`list?: ListViewModel`
    An optional :ref:`ListViewModel <VueListViewModels>` that will be used if provided instead of the one the component will otherwise create automatically.
