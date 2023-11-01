using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System.Reflection;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

#if NET7_0_OR_GREATER
public class SqliteDateTimeOffsetExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;
    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        new EntityFrameworkRelationalServicesBuilder(services)
            .TryAdd<IMemberTranslatorPlugin, SqliteDateTimeOffsetMemberTranslatorPlugin>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "";

        public override int GetServiceProviderHashCode() => 0;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;
    }

    public class SqliteDateTimeOffsetMemberTranslatorPlugin(ISqlExpressionFactory fac, SqlExpressionFactoryDependencies deps) : IMemberTranslatorPlugin
    {
        public IEnumerable<IMemberTranslator> Translators
        {
            get
            {
                yield return new SqliteDateTimeOffsetMemberTranslator((SqliteSqlExpressionFactory)fac, deps);
            }
        }
    }
}

public class SqliteDateTimeOffsetMemberTranslator : IMemberTranslator
{
    private readonly SqliteSqlExpressionFactory _sqlExpressionFactory;
    private readonly SqlExpressionFactoryDependencies deps;
    private readonly RelationalTypeMapping? dateTimeOffsetMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteDateTimeOffsetMemberTranslator(SqliteSqlExpressionFactory sqlExpressionFactory, SqlExpressionFactoryDependencies deps)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        this.deps = deps;
        dateTimeOffsetMapping = deps.TypeMappingSource.FindMapping(typeof(DateTimeOffset), deps.Model!);
    }

    private static readonly Dictionary<string, string> DatePartMapping
        = new()
        {
            { nameof(DateTimeOffset.Year), "%Y" },
            { nameof(DateTimeOffset.Month), "%m" },
            { nameof(DateTimeOffset.DayOfYear), "%j" },
            { nameof(DateTimeOffset.Day), "%d" },
            { nameof(DateTimeOffset.Hour), "%H" },
            { nameof(DateTimeOffset.Minute), "%M" },
            { nameof(DateTimeOffset.Second), "%S" },
            { nameof(DateTimeOffset.DayOfWeek), "%w" }
        };

    // from https://github.com/ErikEJ/EntityFramework/blob/60eb40aa91d285d81a336df1a4d1b711ae06fe29/src/EFCore.Sqlite.Core/Query/Internal/SqliteDateTimeMemberTranslator.cs#L14
    public SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType == typeof(DateTimeOffset))
        {
            var memberName = member.Name;

            if (DatePartMapping.TryGetValue(memberName, out var datePart))
            {
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Strftime(
                        typeof(string),
                        datePart,
                        instance!,
                        new[] { ApplyStoredUtcOffset(instance) }),
                    returnType);
            }

            if (memberName == nameof(DateTimeOffset.Ticks))
            {
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Multiply(
                        _sqlExpressionFactory.Subtract(
                            _sqlExpressionFactory.Function(
                                "julianday",
                                new[] { instance!, ApplyStoredUtcOffset(instance) },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(double)),
                            _sqlExpressionFactory.Constant(1721425.5)), // NB: Result of julianday('0001-01-01 00:00:00')
                        _sqlExpressionFactory.Constant(TimeSpan.TicksPerDay)),
                    typeof(long));
            }

            if (memberName == nameof(DateTimeOffset.Millisecond))
            {
                return _sqlExpressionFactory.Modulo(
                    _sqlExpressionFactory.Multiply(
                        _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.Strftime(
                                typeof(string),
                                "%f",
                                instance!),
                            typeof(double)),
                        _sqlExpressionFactory.Constant(1000)),
                    _sqlExpressionFactory.Constant(1000));
            }

            var format = "%Y-%m-%d %H:%M:%f";
            SqlExpression timestring;
            var modifiers = new List<SqlExpression>();

            switch (memberName)
            {
                case nameof(DateTimeOffset.Now):
                    timestring = _sqlExpressionFactory.Constant("now");
                    modifiers.Add(_sqlExpressionFactory.Constant("localtime"));
                    break;

                case nameof(DateTimeOffset.UtcNow):
                    timestring = _sqlExpressionFactory.Constant("now");
                    format += " Z";
                    break;

                case nameof(DateTimeOffset.Date):
                    timestring = instance!;
                    modifiers.Add(ApplyStoredUtcOffset(instance));
                    modifiers.Add(_sqlExpressionFactory.Constant("start of day"));
                    break;

                case nameof(DateTimeOffset.DateTime):
                    timestring = instance!;
                    modifiers.Add(ApplyStoredUtcOffset(instance));
                    break;

                case nameof(DateTimeOffset.TimeOfDay):
                    format = "%H:%M:%f";
                    modifiers.Add(ApplyStoredUtcOffset(instance));
                    timestring = instance!;
                    break;

                default:
                    return null;
            }

            return _sqlExpressionFactory.Function(
                "rtrim",
                new SqlExpression[]
                {
                    _sqlExpressionFactory.Function(
                        "rtrim",
                        new SqlExpression[]
                        {
                            _sqlExpressionFactory.Strftime(
                                returnType,
                                format,
                                timestring,
                                modifiers),
                            _sqlExpressionFactory.Constant("0")
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, false },
                        returnType),
                    _sqlExpressionFactory.Constant(".")
                },
                nullable: true,
                argumentsPropagateNullability: new[] { true, false },
                returnType);
        }

        return null;
    }

    private SqlFunctionExpression ApplyStoredUtcOffset(SqlExpression? instance)
    {
        // SQLite will use the UTC offset stored in date strings and convert
        // the output of the sqlite date functions to UTC.
        // This operation will extract the UTC offset from the stored string
        // in order to apply it as a modifier ("±HH:MM") to sqlite's date functions,
        // thereby converting the UTC back into its original stored timezone.
        // See https://www.sqlite.org/lang_datefunc.html.

        //var typeMapping = _sqlExpressionFactory.
        if (instance.TypeMapping.Converter is DateTimeOffsetToBinaryConverter)
        {
            throw new NotImplementedException("need to bitshift the offset out of the number, and also translate `instance`");
        }

        return _sqlExpressionFactory.Function(
            "substr",
            new[] {
            instance, 
            // Substring the last 6 chars out of the date, which will include the "zzz" part 
            // of the date ("±HH:MM") as formatted with DateTimeOffset.ToString("...zzz")
            _sqlExpressionFactory.Constant(-6)
            },
            nullable: true,
            argumentsPropagateNullability: new[] { true, false },
            typeof(string)
        );
    }
}

#endif

internal class TestDbContext : DbContext, IAuditLogDbContext<TestAuditLog>
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

#if NET7_0_OR_GREATER
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new SqliteDateTimeOffsetExtension());
#endif
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToStringConverter>();
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ParentWithMappedListText> ParentWithMappedListTexts => Set<ParentWithMappedListText>();
    public DbSet<ParentWithUnMappedListText> ParentWithUnMappedListTexts => Set<ParentWithUnMappedListText>();

    public DbSet<TestAuditLog> AuditLogs => Set<TestAuditLog>();
    public DbSet<AuditLogProperty> AuditLogProperties => Set<AuditLogProperty>();

    public bool SuppressAudit { get; set; }
}

class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Title { get; set; }

    public string? Parent1Id { get; set; }
    public ParentWithMappedListText? Parent1 { get; set; }

    public string? Parent2Id { get; set; }
    public ParentWithUnMappedListText? Parent2 { get; set; }
}

class ParentWithMappedListText
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ListText]
    public string CustomListTextField { get; set; } = null!;
}

class ParentWithUnMappedListText
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string? Name { get; set; }

    [ListText]
    public string CustomListTextField => "Name:" + Name;
}

internal class TestAuditLog : DefaultAuditLog
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public string? CustomField1 { get; set; }
    public string? CustomField2 { get; set; }
}
