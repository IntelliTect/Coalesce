# [KernelPlugin]

`IntelliTect.Coalesce.KernelPluginAttribute`

Configures Microsoft Semantic Kernel function generation for entities, data sources, data source parameters, and methods. When applied, Coalesce generates Semantic Kernel plugin classes that expose your application's functionality as AI-callable functions.

This attribute can be applied to:
- **[CRUD Models](/modeling/model-types/crud.md)**: Generates CUD operations (save, delete) as Semantic Kernel functions
- **[Data Sources](/modeling/model-components/data-sources.md)**: Exposes data source operations (get, list) as Semantic Kernel functions
  - **Data Source Parameters**: Provides semantic descriptions for data source parameters
- **[Methods](/modeling/model-components/methods.md)**: Exposes custom methods as Semantic Kernel functions
  - **Method Parameters**: Provides semantic descriptions for parameters on custom methods

## Example Usage

### On Entity Classes

```csharp
[KernelPlugin(
    Description = "A person in the organization.",
    SaveEnabled = true,
    DeleteEnabled = true
)]
public class Person
{
    public int PersonId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
```

### On Methods

```csharp
public class Person
{
    [Coalesce]
    [KernelPlugin("Changes a person's first name, and optionally assigns a title if they don't yet have one.")]
    public ItemResult<Person> ChangeFirstName(AppDbContext db, string firstName, Titles? title)
    {
        this.FirstName = firstName;
        Title ??= title;
        db.SaveChanges();
        return this;
    }
}
```

### On Data Sources

```csharp
[KernelPlugin("Retrieves products available for sale")]
[DefaultDataSource]
public class ProductDataSource : StandardDataSource<Product, AppDbContext>
{
    public ProductDataSource(CrudContext<AppDbContext> context) : base(context) { }

    [KernelPlugin("Filter by product category")]
    public string Category { get; set; }
}
```

## Properties

<Prop def="public required string Description { get; set; }" />

**Required.** Gets or sets the description of this item for Semantic Kernel. This description informs the AI about the function's purpose and helps it decide when to use the function. Provide clear, descriptive text that explains what the function does and when it should be used.

<Prop def="public bool SaveEnabled { get; set; }" />

**Entity classes only.** Controls whether the Save operation for this entity is exposed through Semantic Kernel. When `true`, generates a save function that can create new instances or update existing ones. Respects all other attribute-based security and Behaviors customization. Default is `false`.

<Prop def="public bool DeleteEnabled { get; set; }" />

**Entity classes only.** Controls whether the Delete operation for this entity is exposed through Semantic Kernel. When `true`, generates a delete function that can remove existing instances. Respects all other attribute-based security and Behaviors customization. Default is `false`.

## Generated Functions

When applied to entity classes, Coalesce automatically generates the following Semantic Kernel functions:

- **get_{entity}**: Retrieves a single entity by its primary key
- **list_{entity}**: Lists entities with support for searching, paging, and field selection
- **save_{entity}**: Creates new or updates existing entities (when `SaveEnabled = true`)
- **delete_{entity}**: Deletes existing entities (when `DeleteEnabled = true`)

All generated functions respect existing security attributes, behaviors, and data source configurations.

## Registration

Generated kernel plugins are automatically registered in your dependency injection container when using the Coalesce template. The AI Chat feature uses these plugins to provide intelligent assistance within your application.

```csharp
// Automatic registration in Program.cs (template-generated code)
foreach (var pluginType in Assembly.GetExecutingAssembly().GetTypes()
    .Where(t => t.BaseType?.Name == "KernelPluginBase`1"))
{
    services.AddScoped(sp => KernelPluginFactory.CreateFromType(pluginType, pluginType.Name, sp));
}
```

## Usage with AI Chat

Once configured, the generated functions become available to the AI chat agent and can be called automatically based on user requests. The AI uses the provided descriptions to understand when and how to use each function.

For more information about AI Chat setup, see [AI Chat Template Feature](/topics/template-features.md#ai-chat).
