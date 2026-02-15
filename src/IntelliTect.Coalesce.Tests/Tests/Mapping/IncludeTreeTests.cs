using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Testing.Fixtures;
using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.Tests.Mapping;

[ClassDataSource<TestDbContextFixture>(Shared = SharedType.None)]
public class IncludeTreeTests
{
    public IncludeTreeTests(TestDbContextFixture dbFixture)
    {
        db = dbFixture.Db;
    }

    private readonly AppDbContext db;


    private async Task AssertBasicChecks(IncludeTree tree)
    {
        // Basic check to see if IncludeTree works for EF includes.
        await Assert.That(tree
            [nameof(Person.CasesAssigned)]
            [nameof(Case.CaseProducts)]
            [nameof(CaseProduct.Case)]
            [nameof(Case.AssignedTo)]).IsNotNull();

        // Make sure that multiple EF include calls merge properly.
        // If this doesn't pass, something is wrong with the way merging is done.
        await Assert.That(tree
            [nameof(Person.CasesAssigned)]
            [nameof(Case.CaseProducts)]
            [nameof(CaseProduct.Case)]
            [nameof(Case.ReportedBy)]).IsNotNull();

        // check to see if IncludedSeparately works
        await Assert.That(tree
            [nameof(Person.CasesReported)]
            [nameof(Case.ReportedBy)]
            [nameof(Person.Company)]).IsNotNull();

        // Make sure that multiple IncludedSeprately calls merge properly.
        // If this doesn't pass, something is wrong with the way merging is done.
        await Assert.That(tree
            [nameof(Person.CasesReported)]
            [nameof(Case.ReportedBy)]
            [nameof(Person.CasesAssigned)]).IsNotNull();
    }

    [Test]
    public async Task IncludeTree_BasicLambdaChecks()
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

        await AssertBasicChecks(tree);
    }

    [Test]
    public async Task IncludeTree_StaticFor()
    {
        var tree = IncludeTree.For<Person>(q => q
            .Include(p => p.Company)
            .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.AssignedTo)
            .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.ReportedBy)
            .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.Company)
            .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.CasesAssigned)
        );

        await AssertBasicChecks(tree);
    }

    [Test]
    public async Task IncludeTree_StaticQueryFor()
    {
        var tree = IncludeTree.QueryFor<Person>()
            .Include(p => p.Company)
            .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.AssignedTo)
            .Include(p => p.CasesAssigned).ThenInclude(c => c.CaseProducts).ThenInclude(c => c.Case.ReportedBy)
            .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.Company)
            .IncludedSeparately(e => e.CasesReported).ThenIncluded(c => c.ReportedBy.CasesAssigned)
            .GetIncludeTree();

        await AssertBasicChecks(tree);
    }

    [Test]
    public async Task IncludeTree_CastedIncludes()
    {
        var tree = IncludeTree.QueryFor<AbstractModel>()
            .Include(p => ((AbstractImpl1)p).Parent).ThenInclude(p => (p as AbstractImpl1).Parent).ThenInclude(p => p.AbstractModelPeople)
            .GetIncludeTree();

        await Assert.That(tree
            [nameof(AbstractImpl1.Parent)]
            [nameof(AbstractImpl1.Parent)]
            [nameof(AbstractImpl1.AbstractModelPeople)]).IsNotNull();
    }

    [Test]
    public async Task IncludeTree_BasicStringChecks()
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

        await AssertBasicChecks(tree);
    }

    [Test]
    public async Task IncludeTree_FilteredIncludeChecks()
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
        await AssertBasicChecks(tree);
    }

    [Test]
    public async Task IncludeTree_ProjectedQuery_AnonymousTypes()
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
                        .OrderBy(p => new { p.Name }.Name)
                        .ToList()
                })
            });

        IncludeTree tree = queryable.GetIncludeTree();
        await Assert.That(tree is { Count: 2 }).IsTrue();
        await Assert.That(tree["SimpleNavigation"] is { Count: 0 }).IsTrue();
        await Assert.That(tree["ComplexCollection"] is { Count: 1 }).IsTrue();
        await Assert.That(tree["ComplexCollection"]["Products"] is { Count: 0 }).IsTrue();
    }

    [Test]
    public async Task IncludeTree_ProjectedQuery_ConcreteTypes()
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
        await Assert.That(tree is { Count: 3 }).IsTrue();
        await Assert.That(tree["OpenedBy"] is { Count: 0 }).IsTrue();
        await Assert.That(tree["OpenerAllOpenedCases"] is { Count: 0 }).IsTrue();
        await Assert.That(tree["Products"] is { Count: 0 }).IsTrue();
    }

    [Test]
    public async Task IncludeTree_ProjectedQuery_IncludedSeparately()
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
        await Assert.That(tree is { Count: 2 }).IsTrue();
        await Assert.That(tree["OpenedBy"] is { Count: 0 }).IsTrue();
        await Assert.That(tree["OpenerAllOpenedCases"] is { Count: 1 }).IsTrue();
        await Assert.That(tree["OpenerAllOpenedCases"]["CaseProducts"] is { Count: 0 }).IsTrue();
    }

    [Test]
    public async Task IncludeTree_ProjectedQuery_ProjectedIncludedProp_DoesNotUsePriorIncludes()
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
        await Assert.That(tree is { Count: 1 }).IsTrue();
        await Assert.That(tree["OpenedBy"] is { Count: 0 }).IsTrue();
    }


    private class StandaloneProjected
    {
        public int Id { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public ICollection<Case> OpenerAllOpenedCases { get; set; }
        public Person OpenedBy { get; set; }
    }
}
