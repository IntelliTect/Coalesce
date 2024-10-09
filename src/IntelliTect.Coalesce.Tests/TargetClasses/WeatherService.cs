using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    [Coalesce, Service]
    public interface IWeatherService
    {
        Task<WeatherData> GetWeatherAsync(TestDbContext.AppDbContext parameterDbContext, Location location, DateTimeOffset? dateTime, SkyConditions? conditions);

        Task<ItemResult<IFile>> FileUploadDownload(IFile file);
    }

    public class WeatherService : IWeatherService
    {
        private readonly TestDbContext.AppDbContext db;

        public WeatherService(TestDbContext.AppDbContext db)
        {
            this.db = db;
        }

        public async Task<ItemResult<IFile>> FileUploadDownload(IFile file) => new(file);

        public WeatherData GetWeather(TestDbContext.AppDbContext parameterDbContext, Location location, DateTimeOffset? dateTime)
            => new WeatherData { TempFahrenheit = 42, Humidity = db.Cases.Count(), Location = location };

        public async Task<WeatherData> GetWeatherAsync (TestDbContext.AppDbContext parameterDbContext, Location location, DateTimeOffset? dateTime, SkyConditions? conditions)
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
}
