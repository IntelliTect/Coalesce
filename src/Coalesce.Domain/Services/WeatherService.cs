using IntelliTect.Coalesce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coalesce.Domain.Services
{
    [Coalesce, Service]
    public interface IWeatherService
    {
        WeatherData GetWeather();
    }

    public class WeatherService : IWeatherService
    {
        private readonly AppDbContext db;

        public WeatherService(AppDbContext db)
        {
            this.db = db;
        }

        public WeatherData GetWeather() => new WeatherData { TempFahrenheit = 42, Humidity = db.Cases.Count() };
    }

    public class WeatherData
    {
        public double TempFahrenheit { get; set; }

        public double Humidity { get; set; }
    }
}
