using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class SqlServerAuditTests
{
    private const string SqlServerConnString = "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceAuditLoggingTests;Trusted_Connection=True;Timeout=5";

    [SkippableFact]
    public async Task WithSqlServer_UpdatesExistingRecordForLikeChanges()
    {
        // Arrange
        using TestDbContext db = BuildDbContext();

        var user = new AppUser { Name = "bob" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Name = "bob2";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Act/Assert
        user.Name = "bob3";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Assert
        var entry = Assert.Single(db.AuditLogs.Include(c => c.Properties).Where(c => c.State == AuditEntryState.EntityModified));

        var propChange = Assert.Single(entry.Properties);
        Assert.Equal(nameof(AppUser.Name), propChange.PropertyName);
        Assert.Equal("bob", propChange.OldValue);
        Assert.Equal("bob3", propChange.NewValue);
    }

    [SkippableFact]
    public async Task WithSqlServer_CreatesNewRecordForLikeChangesOutsideMergeWindow()
    {
        // Arrange
        using TestDbContext db = BuildDbContext();

        var user = new AppUser { Name = "bob" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Name = "bob2";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Make the original Updated entry old such that it is outside the merge window.
        db.AuditLogs.First(c => c.State == AuditEntryState.EntityModified).Date -= TimeSpan.FromMinutes(1);
        db.SaveChanges();

        // Act/Assert
        user.Name = "bob3";
        await db.SaveChangesAsync();
        Assert.Equal(3, db.AuditLogs.Count()); // Now two records for EntityUpdated
    }

    [SkippableFact]
    public async Task WithSqlServer_CreatesNewRecordForUnlikeChanges()
    {
        // Arrange
        using TestDbContext db = BuildDbContext();

        var user = new AppUser { Name = "bob" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Name = "bob2";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Act/Assert
        user.Name = "bob3";
        user.Title = "Associate";
        await db.SaveChangesAsync();

        Assert.Equal(3, db.AuditLogs.Count()); // Now two records for EntityUpdated
    }

    private static TestDbContext BuildDbContext()
    {
        var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
                    .UseSqlServer(SqlServerConnString)
                    .UseCoalesceAuditLogging<TestAuditLog>()
                    .Options);

        try
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        catch (SqlException ex) when (
            ex.Number == 53
            || ex.Message.Contains("Could not open a connection to SQL Server")
            || ex.Message.Contains("The server was not found or was not accessible")
        )
        {
            Skip.If(true, ex.Message);
        }
        catch (PlatformNotSupportedException ex) when (
            ex.Message.Contains("LocalDB is not supported on this platform")
        )
        {
            Skip.If(true, ex.Message);
        }
        return db;
    }
}
