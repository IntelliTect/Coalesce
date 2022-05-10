.. _c-admin-display:

c-admin-display
===============

.. MARKER:summary
    
Behaves the same as [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md), except any collection navigation properties will be rendered as links to an admin list page, and any models will be rendered as a link to an admin item page. 

.. MARKER:summary-end

Links for collections are resolved from vue-router_ with a route name of ``coalesce-admin-list``, a ``type`` route param containing the name of the collection's type, and a query parameter ``filter.<foreign key name>`` with a value of the primary key of the owner of the collection. This route is expected to resolve to a [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md), which is setup by default by the template outlined in [Getting Started with Vue](/stacks/vue/getting-started.md).

Links for single models are resolved from vue-router_ with a route name of ``coalesce-admin-item``, a ``type`` route param containing the name of the model's type, and a ``id`` route param containing the object's primary key. This route is expected to resolve to a [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md), which is setup by default by the template outlined in [Getting Started with Vue](/stacks/vue/getting-started.md).

[[toc]]


Examples
--------

``` vue-html

    <!-- Renders regularly as text: -->
    <c-admin-display :model="person" for="firstName" />

    <!-- Renders as a link to an item: -->
    <c-admin-display :model="person" for="company" />

    <!-- Renders as a link to a list: -->
    <c-admin-display :model="person" for="casesAssigned" />


```

Props
-----

Same as [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md).

Slots
-----

Same as [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md).


