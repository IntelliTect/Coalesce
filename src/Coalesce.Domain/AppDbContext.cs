using Coalesce.Domain.External;
using Coalesce.Domain.Repositories;
using IntelliTect.Coalesce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Coalesce.Domain
{
    public class AppDbContext : DbContext
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

        /// <summary>
        /// Hook to create CaseDtos controller and type script.
        /// </summary>
        public IEnumerable<CaseDto> CaseDtos { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //IncludeExternalExtension.Register<AppDbContext>();
            modelBuilder.Entity<Case>().Ignore(c => c.DevTeamAssigned);
            modelBuilder.Ignore<DevTeam>();

            IncludeExternalExtension.Register<DevTeam>(DevTeamRepository.Items);

            DevTeamRepository.Load();
        }


    }
}
