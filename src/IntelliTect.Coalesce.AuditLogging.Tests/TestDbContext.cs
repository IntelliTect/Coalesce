using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

internal class TestDbContext : DbContext, IAuditLogContext<TestAuditLog>
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TestAuditLog> AuditLogs => Set<TestAuditLog>();
    public DbSet<AuditLogProperty> AuditLogProperties => Set<AuditLogProperty>();

    public bool SuppressAudit { get; set; }
}

class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Title { get; set; }
}

internal class TestAuditLog : DefaultAuditLog
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public string? CustomField1 { get; set; }
    public string? CustomField2 { get; set; }
}
