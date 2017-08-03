using System;
using Coalesce.Domain;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coalesce.Web.Tests
{
    public class DatabaseFixtureLocalDb : IDisposable
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceWebTest;Trusted_Connection=True;";

        public DatabaseFixtureLocalDb()
        {
            ReflectionRepository.AddContext<DbContext>();
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseSqlServer(ConnectionString);
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


    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixtureLocalDb>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}