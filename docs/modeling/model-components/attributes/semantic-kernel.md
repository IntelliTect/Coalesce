# [SemanticKernel] 

<Beta />

`IntelliTect.Coalesce.SemanticKernelAttribute`

Enables and configures the generation of [Microsoft Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) plugins for CRUD models, data sources, and custom methods. These generated [plugin classes](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/?pivots=programming-language-csharp) expose your application's functionality as functions that are callable by AI agents and other LLM applications.

This attribute can be applied to:
- **[CRUD Models](/modeling/model-types/crud.md)**: Generates selected CRUD operations as Semantic Kernel functions
- **[Data Sources](/modeling/model-components/data-sources.md)**: Exposes data source operations (get, list) as Semantic Kernel functions
  - **Data Source Parameters**: Provides semantic descriptions for data source parameters
- **[Methods](/modeling/model-components/methods.md)**: Exposes custom methods as Semantic Kernel functions
  - **Method Parameters**: Provides semantic descriptions for parameters on custom methods

Coalesce itself does not directly provide the means to consume these kernel plugins. However, the project template has an [option](/topics/template-features.md#ai-chat) to include a simple AI Chat assistant as an introduction to consuming Semantic Kernel plugins and functions. For other use cases, consult the [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) documentation.

## Example Usage

### On Model Classes

Place `SemanticKernelAttribute` on a CRUD model class to generate kernel functions for the type's save and delete endpoints (configurable). Think cautiously before enabling save and delete - LLMs can sometimes behave unpredictably.

```csharp
[SemanticKernel(
    Description = "A person represents an employee at AcmeCorp.",
    DefaultDataSourceEnabled = true,
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

### On Data Sources

```csharp
[SemanticKernel("Retrieves products available for sale")]
public class ProductDataSource : StandardDataSource<Product, AppDbContext>
{
    public ProductDataSource(CrudContext<AppDbContext> context) : base(context) { }

    [Coalesce, SemanticKernel("Filter by product category")]
    public string Category { get; set; }
}
```

### On Methods

```csharp
public class Person
{
    // Can also annotate with [Coalesce] to expose via HTTP.
    [SemanticKernel("Changes a person's first name, and optionally updates their title.")]
    public ItemResult<Person> ChangeFirstName(
      AppDbContext db, 
      string firstName, 
      [SemanticKernel("A new title for the person. Provide null to leave the title unchanged.")]
      string? newTitle = null
    )
    {
        FirstName = firstName;
        if (newTitle != null)
        {
            Title = newTitle;
        }
        db.SaveChanges();
        return this;
    }
}
```

## Properties

<Prop def="public string Description { get; set; }" />

Gets or sets the description of this item for Semantic Kernel. This description informs the AI about the item's purpose and helps it decide when to use the function. Provide clear, descriptive text that explains what the function does and when it should be used. This is not directly shown to an end user, but an end user *could* construct a prompt to the LLM to expose this.

When describing model classes, the description should begin similar to the form "A TypeName is ..."

<Prop def="public bool DefaultDataSourceEnabled { get; set; }" />

**CRUD models only.** Controls whether the model's default data source is exposed for get and list operations through Semantic Kernel. You can also expose specific custom data sources by annotating the data source class directly with `[SemanticKernelAttribute]`.

<Prop def="public bool SaveEnabled { get; set; }" />

**CRUD models only.** Controls whether the Save operation for the class is exposed through Semantic Kernel. When `true`, generates a save function that can create new instances or update existing ones. Respects all other attribute-based security and Behaviors customization. Default is `false`.

<Prop def="public bool DeleteEnabled { get; set; }" />

**CRUD models only.** Controls whether the Delete operation for the class is exposed through Semantic Kernel. When `true`, generates a delete function that can remove existing instances. Respects all other attribute-based security and Behaviors customization. Default is `false`.

## Registration

Generated kernel plugins are automatically registered in your dependency injection container when using the Coalesce template. If you did not use the [AI Chat](/topics/template-features.md#ai-chat) option of the Coalesce template (which already includes the following code), you can register your plugins like so in Program.cs:

```csharp
// Dynamic registration in Program.cs (template-generated code)
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
