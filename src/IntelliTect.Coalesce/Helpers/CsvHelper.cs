using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;

namespace IntelliTect.Coalesce.Helpers
{
    /// <summary>
    /// Wrapper class for CSvReader so we don't expose this to the other tiers and take a dependency.
    /// </summary>
    public static class CsvHelper
    {
        public static string CreateCsv<T>(IEnumerable<T> list)
        {
            var textWriter = new StringWriter();
            var csv = new CsvWriter(textWriter);
            csv.WriteRecords(list);
            return textWriter.ToString();         
        }

        public static IEnumerable<T> ReadCsv<T>(string text, bool hasHeader)
        {
            var textReader = new StringReader(text);
            var csv = new CsvReader(textReader);
            csv.Configuration.HasHeaderRecord = hasHeader;
            var records = csv.GetRecords<T>().ToList();
            return records;
        }
    }
}
