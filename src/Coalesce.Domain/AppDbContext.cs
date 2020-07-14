using IntelliTect.Coalesce;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Coalesce.Domain
{
    [Coalesce]
    public class AppDbContext : DbContext
    {
#nullable disable
        public DbSet<Person> People { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CaseProduct> CaseProducts { get; set; }
        public DbSet<ZipCode> ZipCodes { get; set; }
#nullable restore

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().OwnsOne(p => p.Details, cb =>
            {
                cb.OwnsOne(c => c.ManufacturingAddress);
                cb.OwnsOne(c => c.CompanyHqAddress);
            });

        }
    }
}
