# Admin Pages

Coalesce provides pre-built admin pages that offer complete CRUD (Create, Read, Update, Delete) functionality for your data models with minimal setup. These pages are powered by the [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md) and [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md) components and automatically adapt to your data model structure.

## Functionality Overview

### Admin Table Page

The [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md) provides a full-featured page for interacting with a [ListViewModel](/stacks/vue/layers/viewmodels.md) that includes:

- **Browse Records**: View all records in a paginated, sortable table
- **Search**: Search within the model's [searchable properties](/modeling/model-components/attributes/search.md)
- **Filter Data**: Apply advanced filters to narrow down results by specific property values ([read more](/modeling/model-components/data-sources.md#member-applylistpropertyfilter))
- **Bulk Edit**: Toggle into edit mode to modify multiple records directly in the table, with either automatic or manual saves (configured via the `autoSave` prop).
- **Create New Items**: Add new records, either inline (in edit mode), or in the editor page.
- **Delete Records**: Delete individual records with confirmation
- **Execute Methods**: Execute the [static methods](/modeling/model-components/methods.md#static-methods) defined on your model

### Admin Editor Page  

The [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md) provides a page for creating/editing a single [ViewModel](/stacks/vue/layers/viewmodels.md) instance that includes:

- **Edit Records**: Create new records or modify existing ones
- **Auto-save**: Save changes automatically as you type, or manually when needed (configured via the `autoSave` prop)
- **Execute Methods**: Execute the [instance methods](/modeling/model-components/methods.md#instance-methods) defined on your model
- **Seamless Navigation**: Navigate to the admin pages for related records and collections

Both pages automatically create the appropriate ViewModels based on the `type` prop and are designed to be routed to directly with [vue-router](https://router.vuejs.org/).

### Audit Log Page

Coalesce also provides [c-admin-audit-log-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-audit-log-page.md) for browsing audit logs when using the [audit logging](/topics/audit-logging.md) feature. Its view is similar to the table page, but with content and layout optimized for viewing audit logs.

## Adding Descriptions

You can enhance your admin pages by adding descriptions to your models, properties, and methods using standard .NET attributes. These descriptions will automatically appear throughout the admin pages to provide context and guidance to users. Add these descriptions with either the `[Display]` or `[Description]` attribute.

```csharp
[Description("Represents a person in the system with contact information and relationships.")]
public class Person
{
    [Display(Name = "Legal First Name", Description = "The person's full legal first name as it appears on official documents.")]
    public string FirstName { get; set; }
    
    [Description("Primary email address used for system notifications and correspondence.")]
    public string Email { get; set; }

    [Display(Description = "Sends a welcome email to the person with their account details.")]
    public void SendWelcomeEmail() { /* ... */ }
    
    [Description("Generates a comprehensive report of every person's activity history.")]
    public static PersonReport GenerateActivityReport(int personId) { /* ... */ }
}
```


## Default Router Configuration

The [Coalesce project template](/stacks/vue/getting-started.md) includes pre-configured routes for admin pages:

```typescript
import { CAdminTablePage, CAdminEditorPage } from 'coalesce-vue-vuetify3';

const router = new Router({
  routes: [
    {
      path: '/admin/:type',
      name: 'coalesce-admin-list',
      component: CAdminTablePage,
      props: true,
    },
    {
      path: '/admin/:type/edit/:id?',
      name: 'coalesce-admin-item',
      component: CAdminEditorPage,
      props: true,
    },
  ]
});
```

The route names `coalesce-admin-list` and `coalesce-admin-item` are used internally by other Coalesce components to produce routes. For example, [c-admin-display](/stacks/vue/coalesce-vue-vuetify/components/c-admin-display.md) uses these route names to generate links to related objects. 

You can customize the paths any way you like, as long as the routes retain these names and accept the `type` and `id` parameters. The `type` parameter expects the PascalCase name of your model.

### Example URLs

- **List View**: `/admin/Person` - Shows a table of all Person records
- **Create View**: `/admin/Person/edit` - Form to create a new Person
- **Edit View**: `/admin/Person/edit/123` - Form to edit Person with ID 123


## Customizing Admin Components

You can replace the default input and display components used by admin pages for specific model properties or method parameters. This allows you to provide richer, domain-specific UI — such as custom editors, formatted displays, or icon-based status indicators — while keeping the standard admin pages for everything else.

### Configuration

Pass an `adminOverrides` option to `createCoalesceVuetify()`. The `input` and `display` maps use metadata `Value` objects (model properties, method parameters, or method return values from your generated `$metadata`) as keys and Vue components as values.

```ts
import { createCoalesceVuetify } from 'coalesce-vue-vuetify3';
import $metadata from '@/metadata.g';
import MyStatusInput from '@/components/MyStatusInput.vue';
import MyStatusDisplay from '@/components/MyStatusDisplay.vue';

const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
  adminOverrides: {
    input: new Map([
      [$metadata.types.Case.props.status, MyStatusInput],
    ]),
    display: new Map([
      [$metadata.types.Case.props.status, MyStatusDisplay],
    ]),
  },
});
```

### Where overrides apply

| Surface | Input override | Display override |
|---|---|---|
| [c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md) — editable properties | ✅ Replaces `c-input` | ✅ Replaces `c-admin-display` (read-only mode) |
| [c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md) — method parameters | ✅ Replaces `c-input` | — |
| [c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md) — method return value | — | ✅ Replaces `c-display` |
| [c-table](/stacks/vue/coalesce-vue-vuetify/components/c-table.md) with `admin` prop — columns | ✅ Replaces `c-input` (editable mode) | ✅ Replaces `c-admin-display` |

### Custom input component

A custom input component is rendered in place of `c-input`. It receives the same `model` and `for` props that `c-input` accepts, plus any extra props that the admin surface passes (e.g. `density`, `variant`):

```vue
<template>
  <!-- Wrap c-input to get all the standard behaviour, then override slots -->
  <c-input v-bind="$attrs" :model="model" :for="props.for">
    <template #selection>
      <MyStatusDisplay :model="model" />
    </template>
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :title="undefined">
        <MyStatusDisplay :value="item.strValue" />
      </v-list-item>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import type { Property } from 'coalesce-vue';
import type { Case } from '@/models.g';

const props = defineProps<{
  model: Case;
  for: Property;
}>();
</script>
```

### Custom display component

A custom display component is rendered in place of `c-admin-display` or `c-display`. For model properties and method parameters it receives `model` and `for` props. For method return values it receives a `modelValue` prop (used with `v-model`) like `c-display` does.

Design your display component to handle both cases when keying on a property that may also appear as a method return:

```vue
<template>
  <span class="my-status-display">
    <v-icon size="small" :color="config.color">{{ config.icon }}</v-icon>
    {{ label }}
  </span>
</template>

<script setup lang="ts">
import type { Case } from '@/models.g';
import $metadata from '@/metadata.g';
import { computed } from 'vue';

const props = defineProps<{
  /** Provided when used as a property display (c-admin-editor, c-table). */
  model?: Case;
  /** Provided when used as a method return display (c-admin-method). */
  modelValue?: string;
}>();

const statusConfigs: Record<string, { icon: string; color: string }> = {
  Open:             { icon: 'fa fa-circle-dot',   color: '#1976D2' },
  InProgress:       { icon: 'fa fa-spinner',       color: '#F57C00' },
  Resolved:         { icon: 'fa fa-circle-check',  color: '#388E3C' },
  Cancelled:        { icon: 'fa fa-circle-xmark',  color: '#D32F2F' },
};

const key = computed(() => props.modelValue ?? props.model?.status);

const config = computed(() => statusConfigs[key.value] ?? { icon: 'fa fa-circle', color: 'inherit' });

const label = computed(() =>
  $metadata.enums.Statuses.valueLookup[key.value]?.displayName ?? String(key.value ?? '')
);
</script>
```

## Replacing Admin Pages Per Type

The [Customizing Admin Components](#customizing-admin-components) section above lets you swap individual input/display components within the standard admin pages. If you need to go further — replacing an entire admin page with a fully custom view for a specific model type — you can do so via the router.

Because vue-router matches routes in order, placing type-specific routes before the generic `coalesce-admin-list` / `coalesce-admin-item` catch-all routes causes them to be used for that type while all others continue using the default admin pages. You can also use this technique to attach route-level metadata, such as required permissions, to specific types.


```typescript
const router = new Router({
  routes: [
    // Custom admin pages (must come before generic routes)
    {
      path: '/admin/Person',
      component: () => import("./views/PersonList.vue"),
    },
    {
      path: '/admin/Person/edit/:id?',
      component: () => import("./views/PersonEditor.vue"),
      props: true,
    },
    
    // Generic admin routes (catch-all for other types)
    {
      path: '/admin/:type',
      name: 'coalesce-admin-list',
      component: CAdminTablePage,
      props: true,
    },
    {
      path: '/admin/:type/edit/:id?',
      name: 'coalesce-admin-item',
      component: CAdminEditorPage,
      props: true,
    },
  ]
});
```

