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


## Overriding Admin Pages

You can create custom admin pages for specific types while keeping the default pages for others. This is useful when you need specialized functionality for certain models. 

Add routes that match before the generic admin routes to override specific types. You can also use this technique to override the metadata for certain routes - for example, to attach required permissions to a route.

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

