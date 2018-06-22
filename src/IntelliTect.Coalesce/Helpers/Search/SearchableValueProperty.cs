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

        public override IEnumerable<(PropertyViewModel property, string statement)> GetLinqDynamicSearchStatements(
            ClaimsPrincipal user, TimeZoneInfo timeZone, string propertyParent, string rawSearchTerm)
        {
            if (!Property.SecurityInfo.IsReadable(user))
            {
                yield break;
            }

            var propType = Property.Type;
            var propertyAccessor = propertyParent == null
                ? Property.Name
                : $"{propertyParent}.{Property.Name}";

            if (propType.IsDate)
            {
                string DateLiteral(DateTime date)
                {
                    if (propType.IsDateTimeOffset && date.Kind != DateTimeKind.Utc)
                        throw new ArgumentException("datetimeoffset comparand must be a UTC date.");

                    return $"{Property.PureType.Name}" +
                        $"({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second}" +
                        $"{(propType.IsDateTimeOffset ? " , TimeSpan(0)" : "")})";
                }


#pragma warning disable IDE0018 // Inline variable declaration. Invalid suggestion - this variable is used more than once.
                DateTime dt;
#pragma warning restore IDE0018 // Inline variable declaration
                foreach (var formatInfo in DateFormats)
                {
                    if (DateTime.TryParseExact(
                        rawSearchTerm, 
                        formatInfo.Key, 
                        CultureInfo.CurrentCulture, 
                        DateTimeStyles.AllowWhiteSpaces, 
                        out dt
                    ))
                    {
                        if (propType.IsDateTimeOffset)
                            dt = TimeZoneInfo.ConvertTimeToUtc(dt, timeZone);

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
                // Try general date parsing for either searching by day or by minute.
                if (DateTime.TryParse(rawSearchTerm, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
                {
                    TimeSpan range;
                    if (dt.TimeOfDay == TimeSpan.Zero)
                    {
                        // No time component (or time was midnight - this is a limitation we're willing to take for simplicity).
                        // Search by day.
                        dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Unspecified);
                        range = TimeSpan.FromDays(1);
                    }
                    else if (dt.Minute == 0 && dt.Second == 0)
                    {
                        // Time Component present, but without minutes or seconds.
                        // Search for the entire hour.
                        dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Unspecified);
                        range = TimeSpan.FromHours(1);
                    }
                    else
                    {
                        // Time Component present. Search to the minute.
                        dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, DateTimeKind.Unspecified);
                        range = TimeSpan.FromMinutes(1);
                    }

                    if (propType.IsDateTimeOffset)
                        dt = TimeZoneInfo.ConvertTimeToUtc(dt, timeZone);

                    yield return (
                        Property,
                        $"({propertyAccessor} >= {DateLiteral(dt)} && {propertyAccessor} < {DateLiteral(dt.Add(range))})"
                    );
                    yield break;
                }
            }
            else if (propType.IsEnum)
            {
                var enumValuePair = propType.EnumValues
                    .FirstOrDefault(kvp => string.Equals(kvp.Value, rawSearchTerm, StringComparison.InvariantCultureIgnoreCase));
                
                // If the input string mapped to a valid enum value, search by the int value of that enum value.
                if (enumValuePair.Value != null)
                {
                    yield return (Property, $"({propertyAccessor} == \"{enumValuePair.Value}\")");
                }
            }
            else if (propType.IsNumber || propType.IsGuid)
            {
                var propertyClrType = propType.TypeInfo;
                var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(propertyClrType);
                // This allows us to check if the conversion is valid without exceptions
                // (in our code, anyway - the default implementation of this is just a try catch anyway)
                if (typeConverter.IsValid(rawSearchTerm))
                {
                    var comparand = typeConverter.ConvertFromString(rawSearchTerm);
                    if (propType.IsGuid)
                    {
                        comparand = $"\"{comparand}\"";
                    }
                    yield return (Property, $"({propertyAccessor} == {comparand})");
                }
            }
            else if (propType.IsString)
            {
                // Theoretical support for EF.Functions.Like. Not yet supported in DynamicLinq.
                // https://github.com/StefH/System.Linq.Dynamic.Core/issues/105
                /*
                 switch (Property.SearchMethod)
                {
                    case DataAnnotations.SearchAttribute.SearchMethods.BeginsWith:
                        yield return (Property, $"({propertyAccessor} != null && EF.Functions.Like({propertyAccessor}, \"{term}%\")");
                        break;
                    case DataAnnotations.SearchAttribute.SearchMethods.Contains:
                        yield return (Property, $"({propertyAccessor} != null && EF.Functions.Like({propertyAccessor}, \"%{term}%\")");
                        break;
                    default:
                        throw new NotImplementedException();
                }
                 * */
                var term = rawSearchTerm.EscapeStringLiteralForLinqDynamic();
                yield return (Property, $"({propertyAccessor} != null && {propertyAccessor}.{string.Format(Property.SearchMethodCall, term)})");
            }
            else
            {

            }
        }
    }
}
