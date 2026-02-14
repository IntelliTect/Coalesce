using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.Api.DataSources;

public class ProjectedStandaloneEntityTests : TestDbContextFixture
{
    private StandaloneProjected.DefaultSource Source()
        => new StandaloneProjected.DefaultSource(CrudContext);

    [Test]
    public async Task GetIncludeTree_AccountsForProjectedProperties()
    {
        CrudContext.DbContext.Cases.Add(new()
        {
            ReportedBy = new() { FirstName = "Grant" },
            AssignedTo = new() { FirstName = "Meg" },
            CaseProducts = [
                new(){ Product = new(){ Name = "Visual Studio" } }
            ]
        });
        CrudContext.DbContext.SaveChanges();
        CrudContext.DbContext.ChangeTracker.Clear();

        var query = await Source().GetQueryAsync(new DataSourceParameters());
        var results = query.ToList();
        var ct = CrudContext.DbContext.ChangeTracker.Entries().ToList();

        // Precondition: Validate that for projected queries,
        // EF still performs tracking and navigation fixup on
        // entities that are loaded into projected properties.
        // We explicitly project OpenerAllOpenedCases, but we do NOT
        // perform any projection of `.ReportedBy.CasesReported`.
        // However, EF still does navigation fixup and populates CasesReported anyway.
        await Assert.That(results.Single().OpenerAllOpenedCases.Single()).IsEqualTo(results.Single().OpenedBy.CasesReported.Single());

        IncludeTree tree = query.GetIncludeTree();
        await Assert.That(tree is { Count: 3 }).IsTrue();
        await Assert.That(tree["OpenedBy"] is { Count: 0 }).IsTrue();
        await Assert.That(tree["OpenerAllOpenedCases"] is { Count: 0 }).IsTrue();
        await Assert.That(tree["Products"] is { Count: 0 }).IsTrue();
    }
}

[StandaloneEntity]
public class StandaloneProjected
{
    public int Id { get; set; }
    public IEnumerable<Product> Products { get; set; }
    public ICollection<Case> OpenerAllOpenedCases { get; set; }
    public Person OpenedBy { get; set; }

    public class DefaultSource(CrudContext<AppDbContext> context) : StandardDataSource<StandaloneProjected>(context)
    {
        public override Task<IQueryable<StandaloneProjected>> GetQueryAsync(IDataSourceParameters parameters)
        {
            return Task.FromResult(context.DbContext.Cases
                .Select(c => new StandaloneProjected
                {
                    Id = c.CaseKey,
                    OpenedBy = c.ReportedBy,
                    OpenerAllOpenedCases = c.ReportedBy.CasesReported,
                    Products = c.CaseProducts.Select(x => x.Product),
                }));
        }
    }
}