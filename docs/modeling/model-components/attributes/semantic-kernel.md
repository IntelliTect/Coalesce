# [SemanticKernel]

`IntelliTect.Coalesce.SemanticKernelAttribute`

Configures [Microsoft Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) plugin generation for CRUD models, data sources, data source parameters, and methods. When applied, Coalesce generates Semantic Kernel [plugin classes](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/?pivots=programming-language-csharp) that expose your application's functionality as AI-callable functions.

This attribute can be applied to:
- **[Data Sources](/modeling/model-components/data-sources.md)**: Exposes data source operations (get, list) as Semantic Kernel functions
  - **Data Source Parameters**: Provides semantic descriptions for data source parameters
- **[Methods](/modeling/model-components/methods.md)**: Exposes custom methods as Semantic Kernel functions
  - **Method Parameters**: Provides semantic descriptions for parameters on custom methods
- **[CRUD Models](/modeling/model-types/crud.md)**: Generates CUD operations (save, delete) as Semantic Kernel functions

## Example Usage

### On Data Sources

```csharp
[SemanticKernel("Retrieves products available for sale")]
public class ProductDataSource : StandardDataSource<Product, AppDbContext>
{
    public ProductDataSource(CrudContext<AppDbContext> context) : base(context) { }

    [SemanticKernel("Filter by product category")]
    public string Category { get; set; }
}
```

### On Methods

```csharp
public class Person
{
    [Coalesce]
    [SemanticKernel("Changes a person's first name, and optionally assigns a title if they don't yet have one.")]
    public ItemResult<Person> ChangeFirstName(AppDbContext db, string firstName, Titles? title)
    {
        FirstName = firstName;
        Title ??= title;
        db.SaveChanges();
        return this;
    }
}
```

### On Model Classes

Place `SemanticKernelAttribute` on a CRUD model class to generate a kernel function for the type's save and delete endpoints (configurable). Think cautiously before enabling these - LLMs can sometimes behave unpredictably.

```csharp
[SemanticKernel(
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

## Properties

<Prop def="public required string Description { get; set; }" />

**Required.** Gets or sets the description of this item for Semantic Kernel. This description informs the AI about the item's purpose and helps it decide when to use the function. Provide clear, descriptive text that explains what the function does and when it should be used. This is not directly shown to an end user, but an end user *could* construct a prompt to the LLM to expose this.

<Prop def="public bool SaveEnabled { get; set; }" />

**CRUD classes only.** Controls whether the Save operation for the class is exposed through Semantic Kernel. When `true`, generates a save function that can create new instances or update existing ones. Respects all other attribute-based security and Behaviors customization. Default is `false`.

<Prop def="public bool DeleteEnabled { get; set; }" />

**CRUD classes only.** Controls whether the Delete operation for the class is exposed through Semantic Kernel. When `true`, generates a delete function that can remove existing instances. Respects all other attribute-based security and Behaviors customization. Default is `false`.

## Registration

Generated kernel plugins are automatically registered in your dependency injection container when using the Coalesce template. If you did not use the [AI Chat](/topics/template-features.md#ai-chat) option of the Coalesce template (which already includes the following code), you can register your plugins like so in Program.cs:

```csharp
// Automatic registration in Program.cs (template-generated code)
foreach (var pluginType in Assembly.GetExecutingAssembly().GetTypes()
    .Where(t => t.BaseType?.Name == "KernelPluginBase`1"))
{
    services.AddScoped(sp => KernelPluginFactory.CreateFromType(pluginType, pluginType.Name, sp));
}
```

Once registered, you can construct a `Kernel` with any number of these plugins. 

```csharp
IServiceProvider sp; // Injected

var kernel = new Kernel(sp, [.. sp.GetServices<KernelPlugin>()]);
```

Then, you can use the Kernel with Semantic Kernel features like `IChatCompletionService` or `ChatCompletionAgent`. Learn more about Semantic Kernel plugins in the [Microsoft documentation](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/?pivots=programming-language-csharp).
