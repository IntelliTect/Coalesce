using Coalesce.Domain;
using Coalesce.Domain.Tests;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Coalesce.Domain.Tests
{
    public class IncludeTreeTests : IClassFixture<DatabaseFixture>
    {
        private AppDbContext db;

        public IncludeTreeTests(DatabaseFixture dbFixture)
        {
            db = dbFixture.Db;
        }


        [Fact]
        public void IncludeTree()
        {
            var queryable = db.People
                .Where(e => e.FirstName != null)
                .Include(p => p.Company)
                .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.AssignedTo)
                .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.Company)
                .Where(e => e.LastName != null);

            // Just calling this to make sure the query can execute.
            // There might not be any items that match our precidate, but so long as we don't get exceptions, we don't care what the result is.
            var obj = queryable.FirstOrDefault();
            var tree = queryable.GetIncludeTree();

            // Basic check to see if IncludeTree works for EF includes.
            Assert.NotNull(tree
                [nameof(Person.CasesAssigned)]
                [nameof(Case.CaseProducts)]
                [nameof(CaseProduct.Case)]
                [nameof(Case.AssignedTo)]);

            // check to see if IncludedSeparately works
            Assert.NotNull(tree
                [nameof(Person.CasesReported)]
                [nameof(Case.ReportedBy)]
                [nameof(Person.Company)]);

        }
    }
}
