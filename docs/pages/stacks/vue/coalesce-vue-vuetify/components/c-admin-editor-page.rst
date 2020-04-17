.. _c-admin-editor-page:

c-admin-editor-page
===================

.. MARKER:summary
    
A page for a creating/editing single :ref:`ViewModel <VueInstanceViewModels>` instance. Provides a :ref:`c-admin-editor` and a :ref:`c-admin-methods` for the instance. Designed to be routed to directly with vue-router_.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

.. code-block:: vue

    // router.ts or main.ts

    import { CAdminEditorPage } from 'coalesce-vue-vuetify';

    const router = new Router({
        // ...
        routes: [
            // ... other routes
            {
                path: '/admin/:type/edit/:id?',
                name: 'coalesce-admin-item',
                component: CAdminEditorPage,
                props: true,
            },
        ]
    })

Props
-----

:ts:`type: string`
    The PascalCase name of the type to be created/edited.

:ts:`id?: number | string`
    The primary key of the item being edited. If null or not provided, the page will be creating a new instance of the provided :ts:`type` instead.



