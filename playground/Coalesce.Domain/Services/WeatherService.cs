using IntelliTect.Coalesce;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain.Services;

[Coalesce, Service]
public interface IWeatherService
{
    [SemanticKernel("Returns weather data for the given location")]
    Task<WeatherData> GetWeatherAsync(AppDbContext parameterDbContext, Location location, DateTimeOffset? dateTime);
}

public class WeatherService : IWeatherService
{
    private readonly AppDbContext db;

    public WeatherService(AppDbContext db)
    {
        this.db = db;
    }


    public WeatherData GetWeather(AppDbContext parameterDbContext, Location location, DateTimeOffset? dateTime)
        => new WeatherData { TempFahrenheit = 42, Humidity = db.Cases.Count(), Location = location };

    public async Task<WeatherData> GetWeatherAsync (AppDbContext parameterDbContext, Location location, DateTimeOffset? dateTime)
    {
        await Task.Delay(2000);
        return GetWeather(parameterDbContext, location, dateTime);
    }
}

public class Location
{
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
}

public class WeatherData
{
    public double TempFahrenheit { get; set; }

    public double Humidity { get; set; }
    
    public Location? Location { get; set; }
}

public enum SkyConditions
{
    Cloudy,
    PartyCloudy,
    Sunny
}
