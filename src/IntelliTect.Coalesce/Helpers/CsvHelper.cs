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
        private static void Configure<T>(ClassMap map)
        {
            map.ReferenceMaps.Clear();

            //http://stackoverflow.com/questions/1571022/how-would-i-know-if-a-property-is-a-generic-collection
            var membersToRemove = map.MemberMaps
                .Where(p => {
                    // Remove non-properties. Coalesce only exposes properties.
                    if (p.Data.Member.MemberType != System.Reflection.MemberTypes.Property) return true;

                    var memberType = new ReflectionTypeViewModel(p.Data.Member.MemberType());
                    return memberType.IsA(typeof(ICollection<>));
                })
                .ToArray();

            foreach (var pm in membersToRemove)
            {
                map.MemberMaps.Remove(pm);
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
