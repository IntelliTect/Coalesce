using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coalesce.Domain
{
    public static class SampleData
    {
        private static readonly object _lock = new object();

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
                    context.Products.Add(new Product
                    {
                        Name = "Office",
                        Details = new ProductDetails
                        {
                            CompanyHqAddress = GenFu.GenFu.New<StreetAddress>(),
                            ManufacturingAddress = GenFu.GenFu.New<StreetAddress>()
                        },
                        UniqueId = Guid.NewGuid()
                    });
                    context.Products.Add(new Product
                    {
                        Name = "Word",
                        Details = new ProductDetails
                        {
                            CompanyHqAddress = GenFu.GenFu.New<StreetAddress>(),
                            ManufacturingAddress = GenFu.GenFu.New<StreetAddress>()
                        },
                        UniqueId = Guid.NewGuid()
                    });
                    context.Products.Add(new Product
                    {
                        Name = "Excel",
                        Details = new ProductDetails
                        {
                            CompanyHqAddress = GenFu.GenFu.New<StreetAddress>(),
                            ManufacturingAddress = GenFu.GenFu.New<StreetAddress>()
                        },
                        UniqueId = Guid.NewGuid()
                    });
                    context.Products.Add(new Product
                    {
                        Name = "Visual Studio",
                        Details = new ProductDetails
                        {
                            CompanyHqAddress = GenFu.GenFu.New<StreetAddress>(),
                            ManufacturingAddress = GenFu.GenFu.New<StreetAddress>()
                        },
                        UniqueId = Guid.NewGuid()
                    });
                    context.Products.Add(new Product
                    {
                        Name = "Visual Studio 2013",
                        Details = new ProductDetails
                        {
                            CompanyHqAddress = GenFu.GenFu.New<StreetAddress>(),
                            ManufacturingAddress = GenFu.GenFu.New<StreetAddress>()
                        },
                        UniqueId = Guid.NewGuid()
                    });
                    context.Products.Add(new Product
                    {
                        Name = "Visual Studio 2015",
                        Details = new ProductDetails
                        {
                            CompanyHqAddress = GenFu.GenFu.New<StreetAddress>(),
                            ManufacturingAddress = GenFu.GenFu.New<StreetAddress>()
                        },
                        UniqueId = Guid.NewGuid()
                    });

                    context.SaveChanges();
                }

                if (!context.Cases.Any())
                {
                    var random = new Random(randomSeed);
                    int i = 1;
                    string[] severities = new[] { "Low", "Medium", "High", "Critical" };
                    string[] titlePhrases = new[] { "Issue with {0}", "Problem with {0}", "{0} is broken", "{0} doesn't work", "HELP FIX {0} ASAP" };

                    // I'm having fun, OK?
                    string[] descPhrases = new[]
                    {
                        "It just worked yesterday why did you guys break it?",
                        "I didn't do anything to it, I swear! It broke right in front of my eyes!",
                        "I got an error message. What's your fax number so I can send you the Polaroid I took of it?",
                        "please fix asap",
                        "doesn't work anymore",
                        "",
                        "Everything works great!",
                        "I was working last night and my cat jumped on my desk but then the doorbell rang and it was the " +
                        "mailman but the package was damaged so I didnt sign for it but we chatted for a while about the neighbor's new dog " +
                        "and then I was hungry so I went and made this really good casserole that my mother used to make for " +
                        "dinner and then when I went back to my desk my cat was sleeping on the keyboard and I lost all my documents. Fix please?"
                    };

                    GenFu.GenFu.Configure<Case>()
                        .Fill(p => p.CaseKey, () => 0)
                        .Fill(p => p.Title, c => "Case Title " + i)
                        .Fill(p => p.Description, c => "Test Case " + i)
                        .Fill(p => p.Severity, c => severities[random.Next(0, severities.Length)])
                        .Fill(p => p.ReportedById, c => i)
                        .Fill(p => p.AssignedToId, c => i % 10 + 1)
                        .Fill(p => p.DevTeamAssignedId, c => (i++ % 4) + 1);

                    var cases = GenFu.GenFu.ListOf<Case>(20);

                    // Some of the seeds don't work in GenFu for some reason. We do them here.
                    cases.ForEach(c =>
                   {
                       c.OpenedAt = DateTimeOffset.Now.AddSeconds(-random.Next(10, 50000000));
                       c.Status = (Case.Statuses)random.Next(0, (int)Case.Statuses.Cancelled + 1);
                       c.CaseProducts = new List<CaseProduct>
                       {
                            new CaseProduct
                            {
                                Product = context.Products.Skip(random.Next(0, context.Products.Count())).First()
                            }
                       };

                       var phrase = titlePhrases[random.Next(0, titlePhrases.Length)];
                       var productName = c.CaseProducts.First().Product.Name;
                       if (phrase.ToCharArray().Last() <= 'Z')
                       {
                           productName = productName.ToUpper();
                       }
                       c.Description = descPhrases[random.Next(0, descPhrases.Length)];
                       c.Title = string.Format(phrase, productName);
                   });

                    context.Cases.AddRange(cases);
                    context.SaveChanges();
                }
            }
        }
    }
}
