# c-admin-table-page

<!-- MARKER:summary -->
    
A full-featured page for interacting with a [ListViewModel](/stacks/vue/layers/viewmodels.md). Provides a [c-admin-table](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table.md) and a [c-admin-methods](/stacks/vue/coalesce-vue-vuetify/components/c-admin-methods.md) for the list. Designed to be routed to directly with [vue-router](https://router.vuejs.org/).

<!-- MARKER:summary-end -->

## Examples

``` ts
// router.ts or main.ts

// WITHOUT Vuetify A la carte:
import { CAdminTablePage } from 'coalesce-vue-vuetify';
// WITH Vuetify A-la-carte:
import { CAdminTablePage } from 'coalesce-vue-vuetify/lib';

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
```

## Props

<Prop def="type: string" lang="ts" />

The PascalCase name of the type to be listed.

<Prop def="list?: ListViewModel" lang="ts" />

An optional [ListViewModel](/stacks/vue/layers/viewmodels.md) that will be used if provided instead of the one the component will otherwise create automatically.
