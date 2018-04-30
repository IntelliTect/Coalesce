using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coalesce.Domain.Services
{
    [Coalesce, Service]
    public interface IWeatherService
    {
        WeatherData GetWeather(AppDbContext parameterDbContext, Location location, DateTimeOffset? dateTime);
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
    }

    public class Location
    {
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

    }

    public class WeatherData
    {
        public double TempFahrenheit { get; set; }

        public double Humidity { get; set; }
        
        public Location Location { get; set; }
    }
}
