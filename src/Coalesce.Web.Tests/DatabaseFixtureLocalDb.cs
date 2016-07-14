using Coalesce.Domain;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Web.Tests
{
    public class DatabaseFixtureLocalDb
    {
        private static object _lock = new object();
        public static bool IsInitialized = false;

        public AppDbContext Db { get; private set; }

        public const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceWebTest;Trusted_Connection=True;";

        public DatabaseFixtureLocalDb()
        {
            ReflectionRepository.AddContext<DbContext>();
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseSqlServer(ConnectionString);
            Db = new AppDbContext(dbOptionBuilder.Options);

            // Only set the database up once for all the tests.
            // Make sure we don't try to set it up at the same time.
            lock (_lock)
            {
                if (!IsInitialized)
                {
                    // Wipe the database out first;
                    Db.Database.EnsureDeleted();
                    // Add some data to it.
                    SampleData.Initialize(Db);
                    IsInitialized = true;
                }
            }
        }

        /// <summary>
        /// Get a new context.
        /// </summary>
        /// <returns></returns>
        public AppDbContext FreshDb()
        {
            var dbOptionBuilder = new DbContextOptionsBuilder();
            dbOptionBuilder.UseSqlServer(ConnectionString);
            return new AppDbContext(dbOptionBuilder.Options);
        }
    }
}
