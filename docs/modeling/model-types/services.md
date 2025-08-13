
# Services

In a Coalesce application, you are likely to end up with a need for some API endpoints that aren't closely tied with your regular data model. While you could stick [Static Methods](/modeling/model-components/methods.md#static-methods) on one of your entities, doing so can be detrimental to the organization of your code.

Instead, Coalesce lets you generate API Controllers and a TypeScript client from a service. A service, in this case, is nothing more than a C# class or an interface with methods on it, annotated with `[Coalesce, Service]`. An implementation of this class or interface must be injectable from your application's service container, so a registration in Program.cs is needed.

The instance methods of these services work just like other custom [Methods](/modeling/model-components/methods.md) in Coalesce, with one notable distinction: Instance methods don't operate on an instance of a model, but instead on a dependency injected instance of the service.


## Generated Code

For each service in your application's model, Coalesce will generate:

* An API controller with endpoints that correspond to the service's instance methods.
* A TypeScript client containing the members outlined in [Methods](/modeling/model-components/methods.md) for invoking these endpoints.
* <Beta/> A [Semantic Kernel Plugin](/modeling/model-components/attributes/semantic-kernel.md), if any individual methods are annotated with `[SemanticKernel]`.


## Defining Services

Coalesce services are instantiated from your applications dependency injection container when invoked. The type annotated with `[Coalesce, Service]` must be resolvable from that container. For example:

``` c#
// In Program.cs
builder.Services.AddScoped<IWeatherService, WeatherService>();
```

### Interfaces
Interfaces annotated with `[Coalesce, Service]` will automatically expose all methods on that interface. Your interfaces should precisely define the service you intend to expose through Coalesce. Any members you do not want to expose should not be included in the interface.

Although it is not required to use an interface (you can generate endpoints directly from the implementation), it is highly recommended. Interfaces improve testability and reduce the risk of inadvertently changing the signature of a published API.

You can customize the security and other behavior of each method with [ExecuteAttribute](/modeling/model-components/attributes/execute.md).

``` c#
[Coalesce, Service]
public interface IWeatherService
{
    ItemResult<WeatherData> GetWeather(string zipCode);
}
```

### Implementations
If you choose to generate directly from the implementation instead of an interface, annotate the class itself with `[Coalesce, Service]` rather than the interface. Unlike interfaces, each method you want to expose on the class must be explicitly annotated with the `[Coalesce]` attribute.

``` c#
[Coalesce, Service]
public class WeatherService(AppDbContext db)
{
    [Coalesce]
    public ItemResult<WeatherData> GetWeather(string zipCode)
    {
        // Assuming some magic HttpGet method that works as follows...
        var response = HttpGet("http://www.example.com/api/weather/" + zipCode);
        return response.Body.SerializeTo<WeatherData>();
    }

    // This method is not exposed because it lacks the [Coalesce] attribute
    public void MethodThatIsNotExposed() { }
}
```

## Semantic Kernel

<Beta/> Services can be enhanced with [Semantic Kernel integration](/modeling/model-components/attributes/semantic-kernel.md) by annotating individual methods with the `[SemanticKernel]` attribute. This allows your Coalesce services to be automatically registered as Semantic Kernel plugins, making them available for use in AI workflows and function calling scenarios.

When a service method is annotated with `[SemanticKernel]`, Coalesce will generate a Semantic Kernel plugin that exposes that method as a function that can be called by AI models or other Semantic Kernel components. Each method must be individually annotated because a semantic description of each function must be provided to the model.

``` c#
[Coalesce, Service]
public interface IWeatherService
{
    [SemanticKernel("Returns the current weather conditions for a zip code")]
    ItemResult<WeatherData> GetWeather(string zipCode);
    
    [SemanticKernel("Returns the daily future weather forecast for a zip code")]
    ItemResult<WeatherForecast> GetForecast(string zipCode, int days);
}
```

The generated Semantic Kernel plugin will automatically handle parameter validation, serialization, and integration with the Semantic Kernel framework, allowing AI models to call your service methods as part of their reasoning and execution flow.