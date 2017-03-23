using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;

namespace IntelliTect.Coalesce.Helpers
{
    /// <summary>
    /// Wrapper class for CSvReader so we don't expose this to the other tiers and take a dependency.
    /// </summary>
    public static class CsvHelper
    {
        private static void Configure<T>(CsvClassMap map)
        {
            map.ReferenceMaps.Clear();

            //http://stackoverflow.com/questions/1571022/how-would-i-know-if-a-property-is-a-generic-collection
            Type tColl = typeof(ICollection<>);
            foreach (var pm in map.PropertyMaps.Where(p => {
                    Type t = p.Data.Property.PropertyType;
                    return t.IsGenericType && tColl.IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                        t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == tColl);
                }).ToArray())
            {
                map.PropertyMaps.Remove(pm);
            }
        }

        public static string CreateCsv<T>(IEnumerable<T> list)
        {
            var textWriter = new StringWriter();
            var csv = new CsvWriter(textWriter);
            var map = csv.Configuration.AutoMap<T>();
            Configure<T>(map);
            csv.Configuration.RegisterClassMap(map);
            csv.WriteRecords(list);
            return textWriter.ToString();         
        }

        public static IEnumerable<T> ReadCsv<T>(string text, bool hasHeader)
        {
            var textReader = new StringReader(text);
            var csv = new CsvReader(textReader);
            var map = csv.Configuration.AutoMap<T>();
            Configure<T>(map);
            csv.Configuration.RegisterClassMap(map);
            csv.Configuration.HasHeaderRecord = hasHeader;
            var records = csv.GetRecords<T>().ToList();
            return records;
        }
    }
}
