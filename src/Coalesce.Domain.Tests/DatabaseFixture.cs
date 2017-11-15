using System;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Xunit;

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

            ReflectionRepository.Global.AddContext<DbContext>();
        }

        public AppDbContext Db { get; }

        public void Dispose()
        {
            if (Db == null) return;
            Db.Database.EnsureDeleted();
            Db.Dispose();
        }
    }

    [CollectionDefinition("Domain Test Database Collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}