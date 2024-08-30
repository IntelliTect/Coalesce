using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Coalesce.Starter.Vue.Data.Test;

public class AppDbContextForSqlite(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
        // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToStringConverter>();
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyToDateTimeConverter>();
    }

    public class DateOnlyToDateTimeConverter : ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyToDateTimeConverter()
            : base(
                dateOnly => dateOnly.ToDateTime(new TimeOnly()),
                dateTime => DateOnly.FromDateTime(dateTime)
            )
        { }
    }
}
