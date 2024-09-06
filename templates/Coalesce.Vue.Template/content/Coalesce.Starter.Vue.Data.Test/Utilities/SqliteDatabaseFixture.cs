using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coalesce.Starter.Vue.Data.Test;

public class SqliteDatabaseFixture : IDisposable
{
    private readonly SqliteConnection _HoldOpenConnection;

    public DbContextOptions<AppDbContext> Options { get; }
        
    private static readonly ILoggerFactory _LoggerFac = LoggerFactory.Create(b =>
    {
        b.SetMinimumLevel(LogLevel.Error);
        b.AddConsole();
    });

    public SqliteDatabaseFixture()
    {
        // Use a unique database instance per test.
        var connString = $"Data Source=A{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        // Per https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases#shareable-in-memory-databases,
        // a connection must be kept open in order to preserve a particular in-memory SQLite instance.
        // EF doesn't hold connections open necessarily, so we'll do this ourselves.
        _HoldOpenConnection = new SqliteConnection(connString);
        _HoldOpenConnection.Open();

        var dbOptionBuilder = new DbContextOptionsBuilder<AppDbContext>();
        dbOptionBuilder.UseSqlite(connString);
        dbOptionBuilder.UseLoggerFactory(_LoggerFac);
        dbOptionBuilder.EnableDetailedErrors(true);
        dbOptionBuilder.EnableSensitiveDataLogging(true);

        Options = dbOptionBuilder.Options;
        using var db = new AppDbContextForSqlite(Options);

        db.Database.EnsureCreated();
        Seed();
    }

    public void Seed()
    {
        // Seed baseline test data, if desired.
    }

    public void Dispose()
    {
        _HoldOpenConnection.Dispose();
    }
}
