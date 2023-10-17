﻿using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

internal class TestDbContext : DbContext, IAuditLogContext<TestObjectChange>
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TestObjectChange> ObjectChanges => Set<TestObjectChange>();
    public DbSet<ObjectChangeProperty> ObjectChangeProperties => Set<ObjectChangeProperty>();

    public bool SuppressAudit { get; set; }
}

class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Title { get; set; }
}

internal class TestObjectChange : DefaultObjectChange
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public string? CustomField1 { get; set; }
    public string? CustomField2 { get; set; }
}