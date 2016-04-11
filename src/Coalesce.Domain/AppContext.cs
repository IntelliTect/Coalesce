using Coalesce.Domain.External;
using Coalesce.Domain.Repositories;
using Intellitect.ComponentModel.Data;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Coalesce.Domain
{
    public class AppContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CaseProduct> CaseProducts { get; set; }


        public IQueryable<DevTeam> DevTeams
        {
            get
            {
                return DevTeamRepository.Items;
            }
        }

        public AppContext()
        {
        }

        public AppContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //IncludeExternalExtension.Register<AppContext>();
            modelBuilder.Entity<Case>().Ignore(c => c.DevTeamAssigned);
            modelBuilder.Ignore<DevTeam>();

            IncludeExternalExtension.Register<DevTeam>(DevTeamRepository.Items);

            DevTeamRepository.Load();
        }


    }
}
