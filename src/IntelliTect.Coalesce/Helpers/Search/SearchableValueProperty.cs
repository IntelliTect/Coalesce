using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static IntelliTect.Coalesce.DataAnnotations.SearchAttribute;

namespace IntelliTect.Coalesce.Helpers.Search
{
    public class SearchableValueProperty : SearchableProperty
    {
        public SearchableValueProperty(PropertyViewModel prop) : base(prop)
        {
        }

        [Flags]
        public enum ParseFlags
        {
            None = 0,
            HaveYear   = 1 << 0,
            HaveMonth  = 1 << 1,
            HaveDay    = 1 << 2,
            HaveHour   = 1 << 3,
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
            var propertyAccessor = Expression.Property(propertyParent, Property.PropertyInfo);

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
                        min = Parameterize(offset);
                        max = Parameterize(offset.Add(range.Value));
                    }
                    else
                    {
                        min = Parameterize(dt);
                        max = Parameterize(dt.Add(range.Value));
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
                        Parameterize(enumValuePair.Value)
                    ));
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
                    var comparand = typeConverter.ConvertFromString(rawSearchTerm)!;
                    yield return (Property, Expression.Equal(
                        propertyAccessor,
                        Parameterize(comparand)
                    ));
                }
            }
            else if (propType.IsString)
            {
                // Theoretical support for EF.Functions.Like.
                // Could be added with expressions now that we don't use Linq.Dynamic.
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
                //var term = rawSearchTerm.EscapeStringLiteralForLinqDynamic();

                var expr = Expression.AndAlso(
                    // Can only search non-null strings:
                    Expression.NotEqual(
                        propertyAccessor,
                        Expression.Constant(null)
                    ),
                    Property.SearchMethod switch
                    {
                        SearchMethods.EqualsNatural =>
                            propertyAccessor.Call(StringEquals, Parameterize(rawSearchTerm)),

                        // All our "unnatural" search operations perform a ToLower().
                        // The value of this is questionable considering default collation
                        // in SQL server is case-insensitive, but at least for now this behavior
                        // remains to ensure maximum compatibility.
                        // Altering this behavior is considered in https://github.com/IntelliTect/Coalesce/issues/328
                        _ =>
                            propertyAccessor
                                .Call(StringToLower)
                                .Call(Property.SearchMethod switch
                                {
                                    SearchMethods.Contains => StringContains,
                                    SearchMethods.BeginsWith => StringStartsWith,
                                    SearchMethods.Equals => StringEquals,
                                    _ => throw new NotSupportedException()
                                }, Parameterize(rawSearchTerm.ToLower())),
                    }
                );

                yield return (Property, expr);
            }
            else
            {

            }
        }

        /// <summary>
        /// Create an expression representing a constant value in a way that will
        /// cause EF's to parameterize the value and cache the query.
        /// <see href="https://github.com/dotnet/efcore/issues/8909#issuecomment-313768471" />
        /// </summary>
        private Expression Parameterize(object var)
        {
            var type = var.GetType();
            var box = Activator.CreateInstance(typeof(StrongBox<>).MakeGenericType(type));
            ((IStrongBox)box!).Value = var;

            // This emulates what a variable capture from a closure looks like,
            // which EF will translate into a SQL query parameter.
            return Expression.Field(Expression.Constant(box), nameof(StrongBox<object>.Value));
        }

        private static readonly MethodInfo StringToLower
            = typeof(string).GetRuntimeMethod(nameof(string.ToLower), [])!;

        private static readonly MethodInfo StringEquals
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string)])!;

        private static readonly MethodInfo StringContains
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;

        private static readonly MethodInfo StringStartsWith
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;
    }
}
