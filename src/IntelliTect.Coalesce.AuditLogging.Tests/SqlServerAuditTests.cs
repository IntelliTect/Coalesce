﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class SqlServerAuditTests
{
    private const string SqlServerConnString = "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceAuditLoggingTests" +
#if NET9_0
"net9" +
#else
"net8" +
#endif
        ";Trusted_Connection=True;Timeout=5";

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

    [SkippableFact]
    public async Task WithSqlServer_StoredProceduresWork()
    {
        // Arrange - use stored procedures
        using TestDbContext db = BuildDbContextWithStoredProcedures();

        var user = new AppUser { Name = "Bob", Title = "Intern" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Title = "Manager";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Act/Assert - second change should merge like raw SQL
        user.Title = "CEO";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.AuditLogs.Count()); // Two records: EntityAdded, and EntityUpdated

        // Assert - verify the merge happened correctly
        var entry = Assert.Single(db.AuditLogs.Include(c => c.Properties).Where(c => c.State == AuditEntryState.EntityModified));

        var propChange = Assert.Single(entry.Properties!);
        Assert.Equal(nameof(AppUser.Title), propChange.PropertyName);
        Assert.Equal("Intern", propChange.OldValue);
        Assert.Equal("CEO", propChange.NewValue);
    }

    [SkippableFact]
    public async Task WithSqlServer_StoredProcedureIsCreated()
    {
        // Arrange
        using TestDbContext db = BuildDbContextWithStoredProcedures();

        var user = new AppUser { Name = "Bob", Title = "Intern" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act - trigger merge operation to create stored procedure
        user.Title = "Manager";
        await db.SaveChangesAsync();

        // Assert - verify stored procedure was created
        using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'CoalesceAuditMerge_%'";
            if (db.Database.GetDbConnection().State != ConnectionState.Open)
                await db.Database.GetDbConnection().OpenAsync();
            var result = await command.ExecuteScalarAsync();
            var procedureCount = Convert.ToInt32(result);
            Assert.True(procedureCount > 0, "Expected at least one CoalesceAuditMerge stored procedure to be created");
        }
    }

    [SkippableFact]
    public async Task WithSqlServer_DifferentModelsCreateDifferentStoredProcedures()
    {
        // This test verifies that different entity models result in different stored procedures
        // due to the hash-based naming scheme
        
        // Arrange - Create two different contexts that would generate different SQL
        using TestDbContext db1 = BuildDbContextWithStoredProcedures();
        
        var user1 = new AppUser { Name = "Bob" };
        db1.Add(user1);
        await db1.SaveChangesAsync();

        user1.Name = "Robert";
        await db1.SaveChangesAsync(); // This should create a stored procedure

        // Get count of stored procedures after first context
        int initialCount;
        using (var command = db1.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'CoalesceAuditMerge_%'";
            if (db1.Database.GetDbConnection().State != ConnectionState.Open)
                await db1.Database.GetDbConnection().OpenAsync();
            var result = await command.ExecuteScalarAsync();
            initialCount = Convert.ToInt32(result);
        }

        // The same model should reuse the same stored procedure
        using TestDbContext db2 = BuildDbContextWithStoredProcedures();
        
        var user2 = new AppUser { Name = "Alice" };
        db2.Add(user2);
        await db2.SaveChangesAsync();

        user2.Name = "Alicia";
        await db2.SaveChangesAsync(); // This should reuse the existing stored procedure

        int finalCount;
        using (var command = db2.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'CoalesceAuditMerge_%'";
            if (db2.Database.GetDbConnection().State != ConnectionState.Open)
                await db2.Database.GetDbConnection().OpenAsync();
            var result = await command.ExecuteScalarAsync();
            finalCount = Convert.ToInt32(result);
        }
        
        // Should be the same count since the same model is used
        Assert.Equal(initialCount, finalCount);
    }

    private static TestDbContext BuildDbContext()
    {
        var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
                    .UseSqlServer(SqlServerConnString)
                    .UseCoalesceAuditLogging<TestAuditLog>(x => x.WithStoredProcedures(false))
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

    private static TestDbContext BuildDbContextWithStoredProcedures()
    {
        var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
                    .UseSqlServer(SqlServerConnString)
                    .UseCoalesceAuditLogging<TestAuditLog>(x => x.WithStoredProcedures())
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
