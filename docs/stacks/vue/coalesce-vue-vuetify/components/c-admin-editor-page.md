# c-admin-editor-page

<!-- MARKER:summary -->
    
A page for a creating/editing single [ViewModel](/stacks/vue/layers/viewmodels.md) instance. Provides a [c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md) and a [c-admin-methods](/stacks/vue/coalesce-vue-vuetify/components/c-admin-methods.md) for the instance. Designed to be routed to directly with [vue-router](https://router.vuejs.org/).

<!-- MARKER:summary-end -->

## Examples

``` ts
// router.ts or main.ts

// WITHOUT Vuetify A la carte:
import { CAdminEditorPage } from 'coalesce-vue-vuetify';
// WITH Vuetify A-la-carte:
import { CAdminEditorPage } from 'coalesce-vue-vuetify/lib';

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
```

## Props

<Prop def="type: string" lang="ts" />

The PascalCase name of the type to be created/edited.

<Prop def="id?: number | string" lang="ts" />

The primary key of the item being edited. If null or not provided, the page will be creating a new instance of the provided `type` instead.



