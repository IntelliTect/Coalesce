using System;
using Microsoft.EntityFrameworkCore;

namespace Coalesce.Domain.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceDomainTest;Trusted_Connection=True;"
            );
            Db = new AppDbContext(dbOptionBuilder.Options);
            // Wipe the database out first;
            Db.Database.EnsureDeleted();
            // Add some data to it.
            SampleData.Initialize(Db);
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