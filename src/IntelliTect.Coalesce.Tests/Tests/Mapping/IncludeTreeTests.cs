using System.Linq;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.EntityFrameworkCore;
using Xunit;

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

#if NET5_0_OR_GREATER
            // Just calling this to make sure the query can execute.
            // There might not be any items that match our precidate, but so long as we don't get exceptions, we don't care what the result is.
            Person obj = queryable.FirstOrDefault();
#endif

            IncludeTree tree = queryable.GetIncludeTree();
            AssertBasicChecks(tree);
        }
    }
}