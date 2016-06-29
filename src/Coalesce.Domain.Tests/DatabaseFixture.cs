using Coalesce.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain.Tests
{
    public class DatabaseFixture
    {
        public AppContext Db { get; private set; }

        public DatabaseFixture()
        {
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceDomainTest;Trusted_Connection=True;"
            );
            Db = new AppContext(dbOptionBuilder.Options);
            // Wipe the database out first;
            Db.Database.EnsureDeleted();
            // Add some data to it.
            SampleData.Initialize(Db);
        }
    }
}
