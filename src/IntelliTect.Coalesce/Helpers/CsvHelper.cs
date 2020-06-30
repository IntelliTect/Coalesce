using CsvHelper;
using CsvHelper.Configuration;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IntelliTect.Coalesce.Helpers
{
    /// <summary>
    /// Wrapper class for CsvReader so we don't expose this to the other tiers and take a dependency.
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// CsvHelper's ClassMap class is abstract for no good reason.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class GenericMap<T> : ClassMap<T> { }

        private static ClassMap CreateMap<T>()
        {
            var vm = ReflectionRepository.Global.GetClassViewModel<T>() ?? throw new InvalidOperationException("Class has no ClassViewModel");

            var map = new GenericMap<T>();

            foreach (var prop in vm.ClientProperties)
            {
                if (!prop.Type.IsCollection && !prop.Type.IsPOCO)
                {
                    map.Map(typeof(T), prop.PropertyInfo);
                }
            }

            return map;
        }

        public static string CreateCsv<T>(IEnumerable<T> list)
        {
            var textWriter = new StringWriter();
            var csv = new CsvWriter(textWriter);
            csv.Configuration.RegisterClassMap(CreateMap<T>());
            csv.Configuration.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[] { "M/d/yyyy hh:mm:ss tt zzz" };
            csv.Configuration.TypeConverterOptionsCache.GetOptions<DateTimeOffset>().Formats = new[] { "M/d/yyyy hh:mm:ss tt zzz" };
            csv.WriteRecords(list);
            return textWriter.ToString();
        }

        public static IEnumerable<T> ReadCsv<T>(string text, bool hasHeader)
        {
            var textReader = new StringReader(text);
            var csv = new CsvReader(textReader);
            csv.Configuration.RegisterClassMap(CreateMap<T>());
            csv.Configuration.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[] { "M/d/yyyy hh:mm:ss tt zzz" };
            csv.Configuration.TypeConverterOptionsCache.GetOptions<DateTimeOffset>().Formats = new[] { "M/d/yyyy hh:mm:ss tt zzz" };
            csv.Configuration.HasHeaderRecord = hasHeader;
            return csv.GetRecords<T>().ToList();
        }
    }
}
