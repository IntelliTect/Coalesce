using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using IntelliTect.Coalesce.Utilities;
using System.Reflection;
using System.Globalization;

namespace IntelliTect.Coalesce.Helpers.Search
{
    public class SearchableValueProperty : SearchableProperty
    {
        public SearchableValueProperty(PropertyViewModel prop) : base(prop)
        {
        }

        public enum ParseFlags
        {
            HaveYear = 0x01,
            HaveMonth = 0x02,
            HaveDay = 0x04,
            HaveHour = 0x08,
            HaveMinute = 0x10,
            HaveSecond = 0x20,

            HaveDate = 0x7,
            HaveTime = 0x38,
            HaveDateTime = 0x3F,
        }

        public static Dictionary<string, ParseFlags> DateFormats = new Dictionary<string, ParseFlags>
        {
            { "MMM yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MMMM yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MMM, yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MMMM, yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MMM-yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MMMM-yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MMM/yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MMMM/yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "yyyy MM", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "yyyy M", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "yyyy-MM", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "yyyy-M", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "yyyy/MM", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "yyyy/M", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MM/yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "M/yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "MM-yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },
            { "M-yyyy", ParseFlags.HaveYear | ParseFlags.HaveMonth },

            //{ DateTimeFormatInfo.CurrentInfo.MonthDayPattern, ParseFlags.HaveDate },
            //{ DateTimeFormatInfo.CurrentInfo.FullDateTimePattern, ParseFlags.HaveDateTime },
            //{ DateTimeFormatInfo.CurrentInfo.LongDatePattern, ParseFlags.HaveDate },
            //{ DateTimeFormatInfo.CurrentInfo.LongTimePattern, ParseFlags.HaveTime },
            //{ DateTimeFormatInfo.CurrentInfo.RFC1123Pattern, ParseFlags.HaveDateTime },
            //{ DateTimeFormatInfo.CurrentInfo.ShortDatePattern, ParseFlags.HaveDate },
            //{ DateTimeFormatInfo.CurrentInfo.ShortTimePattern, ParseFlags.HaveTime },
            //{ DateTimeFormatInfo.CurrentInfo.SortableDateTimePattern, ParseFlags.HaveDateTime },
            //{ DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern, ParseFlags.HaveDateTime },
        };

        public override IEnumerable<(PropertyViewModel property, string statement)> GetLinqDynamicSearchStatements(ClaimsPrincipal user, string propertyParent, string rawSearchTerm)
        {
            if (!Property.SecurityInfo.IsReadable(user))
            {
                yield break;
            }

            var propertyAccessor = propertyParent == null
                ? Property.Name
                : $"{propertyParent}.{Property.Name}";

            var propertyClrType = Type.GetType(Property.Type.FullName, false);

            if (Property.Type.IsDate)
            {
                string DateLiteral(DateTime date) =>
                    $"{Property.PureType.Name}({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second} {(Property.Type.IsDateTimeOffset ? ", TimeSpan(0)" : "")})";

                DateTime dt;
                foreach (var formatInfo in DateFormats)
                {
                    if (DateTime.TryParseExact(rawSearchTerm, formatInfo.Key, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out dt))
                    {
                        dt = dt.ToUniversalTime();

                        if (formatInfo.Value == (ParseFlags.HaveYear | ParseFlags.HaveMonth))
                        {
                            yield return (
                                Property,
                                $"({propertyAccessor} >= {DateLiteral(dt)} && {propertyAccessor} < {DateLiteral(dt.AddMonths(1))})"
                            );
                        }
                        yield break;
                    }
                }

                // We didn't find any specific format above.
                // Try general date parsing for either eaching by day or by minute.
                if (DateTime.TryParse(rawSearchTerm, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out dt))
                {
                    TimeSpan range;
                    if (dt.TimeOfDay == TimeSpan.Zero)
                    {
                        // No time component (or time was midnight - this is a limitation we're willing to take for simplicity).
                        // Search by day.
                        dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Kind);
                        range = TimeSpan.FromDays(1);
                    }
                    else if (dt.Minute == 0 && dt.Second == 0)
                    {
                        // Time Component present, but without minutes or seconds.
                        // Search for the entire hour.
                        dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind);
                        range = TimeSpan.FromHours(1);
                    }
                    else
                    {
                        // Time Component present. Search to the minute.
                        dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
                        range = TimeSpan.FromMinutes(1);
                    }
                    dt = dt.ToUniversalTime();

                    yield return (
                        Property,
                        $"({propertyAccessor} >= {DateLiteral(dt)} && {propertyAccessor} < {DateLiteral(dt.Add(range))})"
                    );
                    yield break;
                }
            }
            else if (Property.Type.IsEnum)
            {
                var enumValuePair = Property.Type.EnumValues
                    .FirstOrDefault(kvp => string.Equals(kvp.Value, rawSearchTerm, StringComparison.InvariantCultureIgnoreCase));
                
                // If the input string mapped to a valid enum value, search by the int value of that enum value.
                if (enumValuePair.Value != null)
                {
                    yield return (Property, $"({propertyAccessor} == \"{enumValuePair.Value}\")");
                }
            }
            else if (Property.Type.IsNumber)
            {
                var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(propertyClrType);
                // This allows us to check if the conversion is valid without exceptions
                // (in our code, anyway - the default implementation of this is just a try catch anyway)
                if (typeConverter.IsValid(rawSearchTerm))
                {
                    var numberValue = typeConverter.ConvertFromString(rawSearchTerm);
                    yield return (Property, $"({propertyAccessor} == {numberValue})");
                }
            }
            else if (Property.Type.IsString)
            {
                var term = rawSearchTerm.EscapeStringLiteralForLinqDynamic();
                yield return (Property, $"({propertyAccessor} != null && {propertyAccessor}.{string.Format(Property.SearchMethodCall, term)})");
            }
            else
            {

            }
        }
    }
}
