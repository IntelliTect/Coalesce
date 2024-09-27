using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

internal class TestDbContext : DbContext, IAuditLogDbContext<TestAuditLog>
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
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

    public DateTimeOffset? NullableValueType { get; set; }

    public bool BoolProp { get; set; }

    public SecurityPermissionLevels[]? EnumArray { get; set; }
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
