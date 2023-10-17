# c-admin-audit-log-page

<!-- MARKER:summary -->
    
A full-featured page for interacting with Coalesce's [Audit Logging](/topics/audit-logging.md). Presents a view similar to [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md) with content optimized for viewing audit log records. Designed to be routed to directly with [vue-router](https://router.vuejs.org/).

Currently only supports Vue3.

<!-- MARKER:summary-end -->

## Examples

``` ts
const router = new Router({
  // ...
  routes: [
    // ... other routes
    {
      path: '/admin/audit-logs',
      component: CAdminAuditLogPage,
      props: {
        type: 'ObjectChange'
      }
    },
  ]
})
```

## Props

<Prop def="type: string" lang="ts" />

The PascalCase name of your `IObjectChange` implementation.

<Prop def="list?: ListViewModel" lang="ts" />

An optional [ListViewModel](/stacks/vue/layers/viewmodels.md) that will be used if provided instead of the one the component will create automatically from the provided `type` prop.

<Prop def="color: string" lang="ts" />

A Vuetify color name to be applied to the toolbar at the top of the page.


## Slots

<Prop def="row-detail: { item: ObjectChangeViewModel }" lang="ts" />

A slot that can be used to replace the entire content of the Detail column on the page.

<Prop def="row-detail-append: { item: ObjectChangeViewModel }" lang="ts" />

A slot that can be used to append additional content to the Detail column on the page.