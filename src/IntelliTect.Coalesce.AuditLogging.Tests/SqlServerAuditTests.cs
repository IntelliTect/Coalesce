using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class SqlServerAuditTests
{
    private const string SqlServerConnString = "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceAuditLoggingTests;Trusted_Connection=True;Timeout=5";

    [SkippableFact]
    public async Task WithSqlServer_UpdatesExistingRecordForLikeChanges()
    {
        // Arrange
        using TestDbContext db = BuildDbContext();

        var user = new AppUser { Name = "Bob", Title = "Intern" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Title = "Manager";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Act/Assert
        user.Title = "CEO";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Assert
        var entry = Assert.Single(db.AuditLogs.Include(c => c.Properties).Where(c => c.State == AuditEntryState.EntityModified));

        var propChange = Assert.Single(entry.Properties!);
        Assert.Equal(nameof(AppUser.Title), propChange.PropertyName);
        Assert.Equal("Intern", propChange.OldValue);
        Assert.Equal("CEO", propChange.NewValue);
    }

    [SkippableFact]
    public async Task WithSqlServer_CreatesNewRecordForLikeChangesOutsideMergeWindow()
    {
        // Arrange
        using TestDbContext db = BuildDbContext();

        var user = new AppUser { Name = "Bob", Title = "Intern" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Title = "Manager";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Make the original Updated entry old such that it is outside the merge window.
        db.AuditLogs.First(c => c.State == AuditEntryState.EntityModified).Date -= TimeSpan.FromMinutes(1);
        db.SaveChanges();

        // Act/Assert
        user.Title = "CEO";
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

    [SkippableFact]
    public async Task WithSqlServer_CreatesNewRecordForUnmergableChanges()
    {
        // Arrange
        using TestDbContext db = BuildDbContext();

        var parent1 = new ParentWithMappedListText { CustomListTextField = "A" };
        var parent2 = new ParentWithMappedListText { CustomListTextField = "B" };
        var user = new AppUser { Name = "bob", Parent1 = parent1 };
        db.Add(user);
        db.Add(parent1);
        db.Add(parent2);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Parent1 = parent2;
        await db.SaveChangesAsync();

        Assert.Equal(4, db.AuditLogs.Count()); // 3 adds, 1 update

        // Act/Assert
        user.Parent1 = parent1;
        await db.SaveChangesAsync();

        // Now two records for EntityModified because Parent1Id is a foreign key
        // and is therefore not a default candidate for merges.
        Assert.Equal(2, db.AuditLogs.Count(l => l.State == AuditEntryState.EntityModified));
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
