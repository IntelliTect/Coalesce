using Coalesce.Domain;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Web.Tests
{
    public class DatabaseFixtureInMemory : IDisposable
    {
        public AppDbContext Db { get; private set; }

        public DatabaseFixtureInMemory()
        {
            ReflectionRepository.Global.AddAssembly<DbContext>();
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseInMemoryDatabase("InMemoryTestDb");
            Db = new AppDbContext(dbOptionBuilder.Options);
            // Wipe the database out first;
            //Db.Database.EnsureDeleted();
            // Add some data to it.
            SampleData.Initialize(Db);
        }

        public void Dispose()
        {
            // Do something once at the end.

        }
    }
}
