
# Services

In a Coalesce application, you are likely to end up with a need for some API endpoints that aren't closely tied with your regular data model. While you could stick [Static Methods](/modeling/model-components/methods.md#static-methods) on one of your entities, to do so is detrimental to the organization of your code.

Instead, Coalesce allows you to generate API Controllers and a TypeScript client from a service. A service, in this case, is nothing more than a C# class or an interface with methods on it, annotated with `[Coalesce, Service]`. An implementation of this class or interface must be injectable from your application's service container, so a registration in Program.cs is needed.

The instance methods of these services work just like other custom [Methods](/modeling/model-components/methods.md) in Coalesce, with one notable distinction: Instance methods don't operate on an instance of a model, but instead on a dependency injected instance of the service.

## Generated Code

For each external type found in your application's model, Coalesce will generate:

* An API controller with endpoints that correspond to the service's instance methods.
* A TypeScript client containing the members outlined in [Methods](/modeling/model-components/methods.md) for invoking these endpoints.


## Example Service

An example of a service might look something like this:

``` c#
[Coalesce, Service]
public interface IWeatherService
{
    WeatherData GetWeather(string zipCode);
}
```

With an implementation:

``` c#
public class WeatherService : IWeatherService
{
    public WeatherService(AppDbContext db)
    {
        this.db = db;
    }

    public WeatherData GetWeather(string zipCode)
    {
        // Assuming some magic HttpGet method that works as follows...
        var response = HttpGet("http://www.example.com/api/weather/" + zipCode);
        return response.Body.SerializeTo<WeatherData>();
    }

    // This method is not exposed because it is not defined on the interface
    public void MethodThatIsNotExposed() {  }
}
```

And a registration:

``` c#
// In Program.cs
builder.Services.AddCoalesce<AppDbContext>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
```

## Using Interfaces With Services
Interfaces annotated with `[Coalesce, Service]` will automatically expose all methods on that interface. Your interfaces should precisely define the service you intend to expose through Coalesce. Any members you do not want to expose should not be included in the interface.

Although it is not required to use an interface (you can generate endpoints directly from the implementation), it is highly recommended. Interfaces improve testability and reduce the risk of inadvertently changing the signature of a published API.

If you choose to generate directly from the implementation, annotate the class itself with `[Coalesce, Service]` rather than the interface. Unlike interfaces, each method you want to expose on the class must be explicitly annotated with the `[Coalesce]` attribute.

``` c#
[Coalesce, Service]
public class WeatherService
{
    public WeatherService(AppDbContext db)
    {
        this.db = db;
    }

    [Coalesce]
    public WeatherData GetWeather(string zipCode)
    {
        // Assuming some magic HttpGet method that works as follows...
        var response = HttpGet("http://www.example.com/api/weather/" + zipCode);
        return response.Body.SerializeTo<WeatherData>();
    }

    // This method is not exposed because it lacks the [Coalesce] attribute
    private void MethodThatIsNotExposed() { }
}
```