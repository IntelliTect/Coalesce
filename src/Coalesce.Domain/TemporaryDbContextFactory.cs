using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    public class TemporaryDbContextFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext Create(DbContextFactoryOptions options)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=CoalesceDb;Trusted_Connection=True;");
            return new AppDbContext(builder.Options);
        }
    }
}
