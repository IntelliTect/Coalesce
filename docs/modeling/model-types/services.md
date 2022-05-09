
.. _Services:

Services
--------

In a Coalesce, you are fairly likely to end up with a need for some API endpoints that aren't closely tied with your regular data model. While you could stick static :ref:`ModelMethods` on one of your entities, this solution just leads to a jumbled mess of functionality all over your data model that doesn't belong there.

Instead, Coalesce allows you to generate API Controllers and a TypeScript client from a service. A service, in this case, is nothing more than a C# class or an interface with methods on it, annotated with `[Coalesce,Service]`. An implementation of this class or interface must be injectable from your application's service container, so a registration in Startup.cs is needed.

The instance methods of these services conform exactly to the specifications outlined in :ref:`ModelMethods` with a few exceptions:

    * TypeScript functions for invoking the endpoint have no `reload: boolean` parameter.
    * Instance methods don't operate on an instance of some model with a known key, but instead on an injected instance of the service.

Generated Code
==============

For each external type found in your application's model, Coalesce will generate:

    * An API controller with endpoints that correspond to the service's instance methods.
    * A TypeScript client containing the members outlined in :ref:`ModelMethods` for invoking these endpoints.


Example Service
================

An example of a service might look something like this:

.. code-block:: c#

    [Coalesce, Service]
    public interface IWeatherService
    {
        WeatherData GetWeather(string zipCode);
    }

.. code-block:: c#

    public class WeatherData
    {
        public double TempFahrenheit { get; set; }

        // ... Other properties as desired 
    }

With an implementation:

.. code-block:: c#

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

        public void MethodThatIsntExposedBecauseItIsntOnTheExposedInterface() {  }
    }

And a registration:

.. code-block:: c#

    public class Startup 
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCoalesce<AppDbContext>();
            services.AddScoped<IWeatherService, WeatherService>();
        }
    }


While it isn't required that an interface for your service exist - you can generate directly from the implementation, it is highly recommended that an interface be used. Interfaces increase testability and reduce risk of accidentally changing the signature of a published API, among other benefits.