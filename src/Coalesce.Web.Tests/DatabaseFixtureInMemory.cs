using Coalesce.Domain;
using Intellitect.ComponentModel.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Web.Tests
{
    public class DatabaseFixtureInMemory : IDisposable
    {
        public DbContext Db { get; private set; }

        public DatabaseFixtureInMemory()
        {
            ReflectionRepository.AddContext<DbContext>();
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseInMemoryDatabase();
            Db = new DbContext(dbOptionBuilder.Options);
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
