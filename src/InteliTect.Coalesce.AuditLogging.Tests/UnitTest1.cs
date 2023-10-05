using IntelliTect.Coalesce.AuditLogging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InteliTect.Coalesce.AuditLogging.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }
    }
}

internal class TestObjectChange : ObjectChange<string> { }

internal class TestDbContext : DbContext, IAuditLogContext<string, TestObjectChange, ObjectChangeProperty>
{
    public DbSet<TestObjectChange> ObjectChanges => Set<TestObjectChange>();

    public DbSet<ObjectChangeProperty> ObjectChangeProperties => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCoalesceAuditLogs(this);
    }
}
