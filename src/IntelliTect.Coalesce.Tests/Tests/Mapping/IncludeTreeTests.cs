using System.Collections.Generic;
using System.Linq;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IntelliTect.Coalesce.Tests.Mapping
{
    public class IncludeTreeTests : IClassFixture<TestDbContextFixture>
    {
        public IncludeTreeTests(TestDbContextFixture dbFixture)
        {
            db = dbFixture.Db;
        }

        private readonly AppDbContext db;


        private void AssertBasicChecks(IncludeTree tree)
        {
            // Basic check to see if IncludeTree works for EF includes.
            Assert.NotNull(tree
                [nameof(Person.CasesAssigned)]
                [nameof(Case.CaseProducts)]
                [nameof(CaseProduct.Case)]
                [nameof(Case.AssignedTo)]);

            // Make sure that multiple EF include calls merge properly.
            // If this doesn't pass, something is wrong with the way merging is done.
            Assert.NotNull(tree
                [nameof(Person.CasesAssigned)]
                [nameof(Case.CaseProducts)]
                [nameof(CaseProduct.Case)]
                [nameof(Case.ReportedBy)]);

            // check to see if IncludedSeparately works
            Assert.NotNull(tree
                [nameof(Person.CasesReported)]
                [nameof(Case.ReportedBy)]
                [nameof(Person.Company)]);

            // Make sure that multiple IncludedSeprately calls merge properly.
            // If this doesn't pass, something is wrong with the way merging is done.
            Assert.NotNull(tree
                [nameof(Person.CasesReported)]
                [nameof(Case.ReportedBy)]
                [nameof(Person.CasesAssigned)]);
        }

        [Fact]
        public void IncludeTree_BasicLambdaChecks()
        {
            IQueryable<Person> queryable = db.People
                .Where(e => e.FirstName != null)
                .Include(p => p.Company)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.AssignedTo)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.ReportedBy)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.Company)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.CasesAssigned)
                .Where(e => e.LastName != null);

            // Just calling this to make sure the query can execute.
            // There might not be any items that match our precidate, but so long as we don't get exceptions, we don't care what the result is.
            Person obj = queryable.FirstOrDefault();
            IncludeTree tree = queryable.GetIncludeTree();

            AssertBasicChecks(tree);
        }

        [Fact]
        public void IncludeTree_StaticFor()
        {
            var tree = IncludeTree.For<Person>(q => q
                .Include(p => p.Company)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.AssignedTo)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.ReportedBy)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.Company)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.CasesAssigned)
            );

            AssertBasicChecks(tree);
        }

        [Fact]
        public void IncludeTree_StaticQueryFor()
        {
            var tree = IncludeTree.QueryFor<Person>()
                .Include(p => p.Company)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.AssignedTo)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.ReportedBy)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.Company)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.CasesAssigned)
                .GetIncludeTree();

            AssertBasicChecks(tree);
        }

        [Fact]
        public void IncludeTree_BasicStringChecks()
        {
            IQueryable<Person> queryable = db.People
                .Where(e => e.FirstName != null)
                // Includes with strings instead of lambdas:
                .Include(nameof(Person.Company))
                .Include($"{nameof(Person.CasesAssigned)}.{nameof(Case.CaseProducts)}.{nameof(CaseProduct.Case)}.{nameof(Case.AssignedTo)}")
                .Include($"{nameof(Person.CasesAssigned)}.{nameof(Case.CaseProducts)}.{nameof(CaseProduct.Case)}.{nameof(Case.ReportedBy)}")

                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.Company)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.CasesAssigned)
                .Where(e => e.LastName != null);

            // Just calling this to make sure the query can execute.
            // There might not be any items that match our precidate, but so long as we don't get exceptions, we don't care what the result is.
            Person obj = queryable.FirstOrDefault();
            IncludeTree tree = queryable.GetIncludeTree();

            AssertBasicChecks(tree);
        }

        [Fact]
        public void IncludeTree_FilteredIncludeChecks()
        {
            IQueryable<Person> queryable = db.People
                .Where(e => e.FirstName != null)
                .Include(p => p.Company)
                .Include(p => p.CasesAssigned
                    // Exhaustive list of all filtered include operations:
                    // https://docs.microsoft.com/en-us/ef/core/querying/related-data/eager#filtered-include
                    .Where(c => c.Description != null)
                    .OrderBy(c => c.OpenedAt)
                    .ThenBy(c => c.ReportedById)
                    .OrderByDescending(c => c.AssignedToId)
                    .ThenByDescending(c => c.CaseKey)
                    .Skip(1)
                    .Take(2)
                ).ThenInclude(c => c.CaseProducts.OrderBy(c => c.ProductId)).ThenInclude(c => c.Case.AssignedTo)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.ReportedBy)
                .IncludedSeparately(e => e.CasesReported
                    .Where(c => c.Description != null)
                    .OrderBy(c => c.OpenedAt)
                    .ThenBy(c => c.ReportedById)
                    .OrderByDescending(c => c.AssignedToId)
                    .ThenByDescending(c => c.CaseKey)
                    .Skip(1)
                    .Take(2)).ThenIncluded(c => c.ReportedBy.Company)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.CasesAssigned.Where(c => c.Description != null))
                .Where(e => e.LastName != null);

            // Just calling this to make sure the query can execute.
            // There might not be any items that match our predicate, but so long as we don't get exceptions, we don't care what the result is.
            Person obj = queryable.FirstOrDefault();

            IncludeTree tree = queryable.GetIncludeTree();
            AssertBasicChecks(tree);
        }

        [Fact]
        public void IncludeTree_ProjectedQuery_AnonymousTypes()
        {
            var queryable = db.People
                .Select(p => new
                {
                    SimpleNavigation = p.Company,
                    ComplexCollection = p.CasesAssigned.Select(c => new
                    {
                        CaseTitle = c.Title,
                        Products = c.CaseProducts
                            // Intermediate projection:
                            .Select(p => new
                            {
                                p.Product,
                                p.Case,
                            })
                            // This is the "real" projection.
                            .Select(p => p.Product)
                            // NewExpression in a position that isn't a projection.
                            .OrderBy(p => new {p.Name}.Name)
                            .ToList()
                    })
                });

            IncludeTree tree = queryable.GetIncludeTree();
            Assert.True(tree is { Count: 2 });
            Assert.True(tree["SimpleNavigation"] is { Count: 0 });
            Assert.True(tree["ComplexCollection"] is { Count: 1 });
            Assert.True(tree["ComplexCollection"]["Products"] is { Count: 0 });
        }

        [Fact]
        public void IncludeTree_ProjectedQuery_ConcreteTypes()
        {
            var queryable = db.Cases
                .Select(c => new StandaloneProjected
                {
                    Id = c.CaseKey,
                    OpenedBy = c.ReportedBy,
                    OpenerAllOpenedCases = c.ReportedBy.CasesReported,
                    Products = c.CaseProducts.Select(x => x.Product),
                });

            IncludeTree tree = queryable.GetIncludeTree();
            Assert.True(tree is { Count: 3 });
            Assert.True(tree["OpenedBy"] is { Count: 0 });
            Assert.True(tree["OpenerAllOpenedCases"] is { Count: 0 });
            Assert.True(tree["Products"] is { Count: 0 });
        }

        [Fact]
        public void IncludeTree_ProjectedQuery_IncludedSeparately()
        {
            var queryable = db.Cases
                .Select(c => new StandaloneProjected
                {
                    Id = c.CaseKey,
                    OpenedBy = c.ReportedBy,
                })
                .IncludedSeparately(p => p.OpenerAllOpenedCases).ThenIncluded(c => c.CaseProducts)
                ;

            IncludeTree tree = queryable.GetIncludeTree();
            Assert.True(tree is { Count: 2 });
            Assert.True(tree["OpenedBy"] is { Count: 0 });
            Assert.True(tree["OpenerAllOpenedCases"] is { Count: 1 });
            Assert.True(tree["OpenerAllOpenedCases"]["CaseProducts"] is { Count: 0 });
        }

        [Fact]
        public void IncludeTree_ProjectedQuery_ProjectedIncludedProp_DoesNotUsePriorIncludes()
        {
            // See comments on https://github.com/IntelliTect/Coalesce/issues/478#issuecomment-2555963059
            // for why we don't want to respect .Includes that happen before a projection
            var queryable = db.Cases
                .Include(c => c.ReportedBy.Company)
                .Select(c => new StandaloneProjected
                {
                    Id = c.CaseKey,
                    OpenedBy = c.ReportedBy,
                })
                ;

            IncludeTree tree = queryable.GetIncludeTree();
            Assert.True(tree is { Count: 1 });
            Assert.True(tree["OpenedBy"] is { Count: 0 });
        }


        private class StandaloneProjected
        {
            public int Id { get; set; }
            public IEnumerable<Product> Products { get; set; }
            public ICollection<Case> OpenerAllOpenedCases { get; set; }
            public Person OpenedBy { get; set; }
        }
    }
}