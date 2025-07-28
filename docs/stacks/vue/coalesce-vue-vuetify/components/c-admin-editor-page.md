# c-admin-editor-page

<!-- MARKER:summary -->
    
A page for a creating/editing single [ViewModel](/stacks/vue/layers/viewmodels.md) instance. Provides a [c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md) and a [c-admin-methods](/stacks/vue/coalesce-vue-vuetify/components/c-admin-methods.md) for the instance. Designed to be routed to directly with [vue-router](https://router.vuejs.org/).

<!-- MARKER:summary-end -->

Check out [Admin Pages](/stacks/vue/admin-pages.md) for a full overview of the admin pages in Coalesce.

## Examples

``` ts
import { CAdminEditorPage } from 'coalesce-vue-vuetify3';

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

<Prop def="autoSave?: 'auto' | boolean = 'auto'" lang="ts" />

Controls whether auto-save is used for the item. If `auto` (the default), auto-saves are used as long as the type has no [init-only properties](/modeling/model-components/properties.md#init-only-properties).


