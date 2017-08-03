using System;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;

namespace Coalesce.Domain.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceDomainTest;Integrated Security=True;"
            );
            Db = new AppDbContext(dbOptionBuilder.Options);
            // Wipe the database out first;
            Db.Database.EnsureDeleted();
            // Add some data to it.
            SampleData.Initialize(Db);

            ReflectionRepository.AddContext<DbContext>();
        }

        public AppDbContext Db { get; }

        public void Dispose()
        {
            if (Db == null) return;
            Db.Database.EnsureDeleted();
            Db.Dispose();
        }
    }
}