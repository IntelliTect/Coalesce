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
        WeatherData GetWeather(DateTimeOffset? dateTime);
    }

    public class WeatherService : IWeatherService
    {
        private readonly AppDbContext db;

        public WeatherService(AppDbContext db)
        {
            this.db = db;
        }

        public WeatherData GetWeather(DateTimeOffset? dateTime) => new WeatherData { TempFahrenheit = 42, Humidity = db.Cases.Count() };
    }

    public class WeatherData
    {
        public double TempFahrenheit { get; set; }

        public double Humidity { get; set; }
    }
}
