
# Services

In a Coalesce application, you are likely to end up with a need for some API endpoints that aren't closely tied with your regular data model. While you could stick [Static Methods](/modeling/model-components/methods.md#static-methods) on one of your entities, do so is detrimental to the organization of your code.

Instead, Coalesce allows you to generate API Controllers and a TypeScript client from a service. A service, in this case, is nothing more than a C# class or an interface with methods on it, annotated with `[Coalesce, Service]`. An implementation of this class or interface must be injectable from your application's service container, so a registration in Startup.cs is needed.

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

    public void MethodThatIsNotExposedBecauseItIsNotOnTheExposedInterface() {  }
}
```

And a registration:

``` c#
public class Startup 
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCoalesce<AppDbContext>();
        services.AddScoped<IWeatherService, WeatherService>();
    }
}
```

While it isn't required that an interface for your service exist (you can generate directly from the implementation), it is highly recommended that an interface be used. Interfaces increase testability and reduce risk of accidentally changing the signature of a published API, among other benefits.