using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Coalesce.Domain
{
    public static class SampleData
    {
        private static object _lock = new object();

        public static void Initialize(AppDbContext context, int randomSeed = 1)
        {
            // Lock so that parallel unit tests generate the same data.
            lock (_lock)
            {
                // Set the random seed.
                GenFuBaseValueGenerator.RandomSeed = randomSeed;

                try
                {
                    context.Database.Migrate();
                }
                catch (InvalidOperationException e) when (e.Message == "No service for type 'Microsoft.EntityFrameworkCore.Migrations.IMigrator' has been registered.")
                {
                    // this exception is expected when using an InMemory database
                }

                if (!context.Companies.Any())
                {
                    GenFu.GenFu.Configure<Company>().Fill(c => c.CompanyId, () => 0);
                    var companies = GenFu.GenFu.ListOf<Company>(10);
                    context.Companies.AddRange(companies);
                    context.SaveChanges();
                }

                if (!context.People.Any())
                {
                    int i = 0;
                    GenFu.GenFu.Configure<Person>().Fill(p => p.PersonId, () => 0)
                        .Fill(p => p.CompanyId, () =>
                        {
                            int companyId = (i % 10) + 1;
                            i++;
                            return companyId;
                        });
                    context.People.AddRange(GenFu.GenFu.ListOf<Person>(100));
                    context.SaveChanges();
                }

                if (!context.Products.Any())
                {
                    context.Products.Add(new Product { Name = "Office" });
                    context.Products.Add(new Product { Name = "Word" });
                    context.Products.Add(new Product { Name = "Excel" });
                    context.Products.Add(new Product { Name = "Visual Studio" });
                    context.Products.Add(new Product { Name = "Visual Studio 2013" });
                    context.Products.Add(new Product { Name = "Visual Studio 2015" });

                    context.SaveChanges();
                }

                if (!context.Cases.Any())
                {
                    context.Cases.Add(new Case
                    {
                        Description = "Test Case",
                        Title = "Case Title",
                        DevTeamAssignedId = 1
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
