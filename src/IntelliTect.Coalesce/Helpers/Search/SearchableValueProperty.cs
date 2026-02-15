using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using static IntelliTect.Coalesce.DataAnnotations.SearchAttribute;

namespace IntelliTect.Coalesce.Helpers.Search;

public class SearchableValueProperty : SearchableProperty
{
    public SearchableValueProperty(PropertyViewModel prop) : base(prop)
    {
    }

    [Flags]
    public enum ParseFlags
    {
        None = 0,
        HaveYear = 1 << 0,
        HaveMonth = 1 << 1,
        HaveDay = 1 << 2,
        HaveHour = 1 << 3,
        HaveMinute = 1 << 4,
        HaveSecond = 1 << 5,

        HaveDate = HaveYear | HaveMonth | HaveDay,
        HaveTime = HaveHour | HaveMinute | HaveSecond,
        HaveDateTime = HaveDate | HaveTime,
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
        { "yyyy", ParseFlags.HaveYear },
        { "yy", ParseFlags.HaveYear },

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

    public override IEnumerable<(PropertyViewModel property, Expression statement)> GetLinqSearchStatements(
        CrudContext context, Expression propertyParent, string rawSearchTerm)
    {
        if (!Property.SecurityInfo.IsFilterAllowed(context))
        {
            yield break;
        }

        var propType = Property.Type;
        var propertyAccessor = propertyParent.Prop(Property);

        if (propType.IsDate)
        {
            DateTime dt = default;
            TimeSpan? range = null;

            // Parse special formats that search entire years or month:
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
                    if (formatInfo.Value == (ParseFlags.HaveYear | ParseFlags.HaveMonth))
                    {
                        range = dt.AddMonths(1) - dt;
                        break;
                    }
                    else if (formatInfo.Value == (ParseFlags.HaveYear))
                    {
                        range = dt.AddYears(1) - dt;
                        break;
                    }
                }
            }

            // We didn't find any specific format above.
            // Try general date parsing for either searching by day or by minute.
            if (range == null && DateTime.TryParse(rawSearchTerm, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
            {
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
            }

            if (range != null)
            {
                Expression min, max;
                if (propType.IsDateTimeOffset)
                {
                    var offset = new DateTimeOffset(dt, context.TimeZone.GetUtcOffset(dt));

                    // convert to UTC, to support Postgres which doesn't allow non-zero offsets
                    offset = offset.ToUniversalTime();

                    min = offset.AsQueryParam(propType);
                    max = offset.Add(range.Value).AsQueryParam(propType);
                }
                else if (propType.NullableValueUnderlyingType.IsA<DateOnly>())
                {
                    var dateOnly = new DateOnly(dt.Year, dt.Month, dt.Day);
                    min = dateOnly.AsQueryParam(propType);
                    max = dateOnly.AddDays((int)range.Value.TotalDays).AsQueryParam(propType);
                }
                else
                {
                    min = dt.AsQueryParam(propType);
                    max = dt.Add(range.Value).AsQueryParam(propType);
                }

                yield return (
                    Property,
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(
                            propertyAccessor,
                            min
                        ),
                        Expression.LessThan(
                            propertyAccessor,
                            max
                        )
                    )
                );
            }
        }
        else if (propType.IsEnum)
        {
            var enumValuePair = propType.EnumValues
                .FirstOrDefault(kvp => string.Equals(kvp.Name, rawSearchTerm, StringComparison.OrdinalIgnoreCase));

            // If the input string mapped to a valid enum value, search by the int value of that enum value.
            if (enumValuePair != null)
            {
                yield return (Property, Expression.Equal(
                    propertyAccessor,
                    enumValuePair.Value.AsQueryParam(propType)
                ));
            }
        }
        else if (propType.IsNumber || propType.IsGuid || propType.IsUri)
        {
            var propertyClrType = propType.TypeInfo;
            var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(propertyClrType);
            // This allows us to check if the conversion is valid without exceptions
            // (in our code, anyway - the default implementation of this is just a try catch anyway)
            if (typeConverter.IsValid(rawSearchTerm))
            {
                var comparand = typeConverter.ConvertFromString(rawSearchTerm)!;
                yield return (Property, Expression.Equal(
                    propertyAccessor,
                    comparand.AsQueryParam(propType)
                ));
            }
        }
        else if (propType.IsString)
        {
            var expr = Expression.AndAlso(
                // Can only search non-null strings:
                Expression.NotEqual(
                    propertyAccessor,
                    Expression.Constant(null)
                ),
                Property.SearchMethod switch
                {
                    SearchMethods.EqualsNatural =>
                        propertyAccessor.Call(MethodInfos.StringEquals, rawSearchTerm.AsQueryParam()),

                    // All our "unnatural" search operations perform a ToLower().
                    // The value of this is questionable considering default collation
                    // in SQL server is case-insensitive, but at least for now this behavior
                    // remains to ensure maximum compatibility.
                    // Altering this behavior is considered in https://github.com/IntelliTect/Coalesce/issues/328
                    _ =>
                        propertyAccessor
                            .Call(MethodInfos.StringToLower)
                            .Call(Property.SearchMethod switch
                            {
                                SearchMethods.Contains => MethodInfos.StringContains,
                                SearchMethods.BeginsWith => MethodInfos.StringStartsWith,
                                SearchMethods.Equals => MethodInfos.StringEquals,
                                _ => throw new NotSupportedException()
                            }, rawSearchTerm.ToLower().AsQueryParam()),
                }
            );

            yield return (Property, expr);
        }
    }
}
