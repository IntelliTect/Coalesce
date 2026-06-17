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

Pass an `adminOverrides` option to `createCoalesceVuetify()`. It accepts an array of `[metadata, override]` pairs, where metadata is a `Value` object from your generated `$metadata` (a model property, method parameter, or method return value), and override is an object with optional `input` and/or `display` component replacements.

```ts
import { createCoalesceVuetify } from 'coalesce-vue-vuetify3';
import $metadata from '@/metadata.g';
import MyStatusInput from '@/components/MyStatusInput.vue';
import MyStatusDisplay from '@/components/MyStatusDisplay.vue';

const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
  adminOverrides: [
    [$metadata.types.Case.props.status, {
      input: MyStatusInput,
      display: MyStatusDisplay,
    }],
  ],
});
```

| Surface | Input override | Display override |
|---|---|---|
| [c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md) — editable properties | ✅ Replaces `c-input` | ✅ Replaces `c-admin-display` (read-only mode) |
| [c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md) — method parameters | ✅ Replaces `c-input` | — |
| [c-admin-method](/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.md) — method return value | — | ✅ Replaces `c-admin-display` |
| [c-table](/stacks/vue/coalesce-vue-vuetify/components/c-table.md) with `admin` prop — columns | ✅ Replaces `c-input` (editable mode) | ✅ Replaces `c-admin-display` |

### Input overrides

A custom input component is rendered in place of `c-input`. It receives `model` and `for` props, plus any extra props that the admin surface passes (e.g. `density`, `variant`). The `for` prop is the resolved metadata value for the overridden surface: for editable model fields this is a `Property`, and for method parameters it is a `Value` metadata object, not a string:

```vue
<template>
  <!-- Wrap c-input to get all the standard behavior, then override slots -->
  <c-input :model="model" for="status">
    <template #selection>
      <MyStatusDisplay :model-value="model.status" />
    </template>
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :title="undefined">
        <MyStatusDisplay :model-value="item.value" />
      </v-list-item>
    </template>
  </c-input>
</template>

<script setup lang="ts">
import MyStatusDisplay from './MyStatusDisplay.vue';
import type { Case } from '@/models.g';

const props = defineProps<{
  model: Case;
}>();
</script>
```

### Display overrides

A custom display component is rendered in place of `c-admin-display` or `c-display`. It always receives a `modelValue` prop containing the property's current value, plus `model` and `for` props when rendering a model property (in `c-admin-editor` and `c-table`).

```vue
<template>
  <span class="my-status-display">
    <v-icon size="small" :color="statusConfig.color" class="mr-1">{{ statusConfig.icon }}</v-icon>
    <span :style="{ color: statusConfig.color }">{{ statusConfig.label }}</span>
  </span>
</template>

<script setup lang="ts">
import { Statuses } from '@/models.g';
import $metadata from '@/metadata.g';
import { computed } from 'vue';

const props = defineProps<{
  modelValue?: Statuses | null;
}>();

const statusStyles: Record<Statuses, { icon: string; color: string }> = {
  [Statuses.Open]:             { icon: 'fa fa-circle-dot',   color: '#1976D2' },
  [Statuses.InProgress]:       { icon: 'fa fa-spinner',      color: '#F57C00' },
  [Statuses.Resolved]:         { icon: 'fa fa-circle-check', color: '#388E3C' },
  [Statuses.ClosedNoSolution]: { icon: 'fa fa-ban',          color: '#757575' },
  [Statuses.Cancelled]:        { icon: 'fa fa-circle-xmark', color: '#D32F2F' },
};

const fallback = { icon: 'fa fa-circle', color: 'inherit', label: '' };

const statusConfig = computed(() => {
  const key = props.modelValue;
  if (!key) return fallback;
  const style = statusStyles[key];
  return {
    icon: style?.icon ?? fallback.icon,
    color: style?.color ?? fallback.color,
    label: $metadata.enums.Statuses.valueLookup[key]?.displayName ?? key,
  };
});
</script>
```

## Extending Admin Pages

You can inject additional content into admin page components on a per-type basis using `adminExtensions`. Pass an array of `[key, extension]` pairs to `createCoalesceVuetify()`, where the key is either a model type from `$metadata` or `"*"` for a global default.

### Configuration

```ts
import { createCoalesceVuetify } from 'coalesce-vue-vuetify3';
import $metadata from '@/metadata.g';
import MyCaseToolbarActions from '@/components/MyCaseToolbarActions.vue';
import MyGlobalToolbarActions from '@/components/MyGlobalToolbarActions.vue';

const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
  adminExtensions: [
    // Global default for all types without a specific override
    ["*", {
      tableToolbarActions: MyGlobalToolbarActions,
    }],
    // Type-specific override (takes precedence over "*")
    [$metadata.types.Case, {
      tableToolbarActions: MyCaseToolbarActions,
    }],
  ],
});
```

### Extension fields

| Field | Surface | Props | Description |
|---|---|---|---|
| `tableToolbarActions` | [c-admin-table-toolbar](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-toolbar.md) | `list: ListViewModel` | Component rendered after the built-in buttons (Create, Reload, Edit) |
| `editorToolbarActions` | [c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md) | `model: ViewModel` | Component rendered in the editor toolbar, before Save/Delete/Reload |
| `editorActions` | [c-admin-editor](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.md) | `model: ViewModel` | Component rendered in the editor's footer actions area |
| `tableRowActions` | [c-admin-table](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table.md) | `model: ViewModel`, `list: ListViewModel` | Component rendered in each row's actions column, before the built-in Edit/Delete buttons |
| `tablePageHeader` | [c-admin-table-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.md) | `list: ListViewModel` | Component rendered before the table |
| `editorPageHeader` | [c-admin-editor-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.md) | `model: ViewModel` | Component rendered before the editor |

### Resolution order

When resolving an extension, a type-specific entry (e.g. `$metadata.types.Case`) takes precedence over `"*"`. If neither is found, nothing is rendered.

## Replacing Admin Pages

If the above customization and extension options aren't enough, you can replace an entire admin page with a fully custom view for a specific model type via the router.

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

