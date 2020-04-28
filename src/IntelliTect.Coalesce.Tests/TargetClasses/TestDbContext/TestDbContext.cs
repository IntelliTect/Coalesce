using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    [Coalesce]
    public class TestDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CaseProduct> CaseProducts { get; set; }

        public DbSet<ComplexModel> ComplexModels { get; set; }
        public DbSet<Test> Tests { get; set; }

        public TestDbContext() : this(Guid.NewGuid().ToString()) { }

        public TestDbContext(string memoryDatabaseName)
            : base(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(memoryDatabaseName).Options)
        { }

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        { }
    }
}
