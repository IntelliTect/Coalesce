using IntelliTect.Coalesce.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce2001_SecurityBypassAnalyzerTests : CSharpAnalyzerVerifier<SecurityBypassAnalyzer>
{
    [Test]
    public async Task DataSourceWithClaimsPrincipalAndDefaultDataSource_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";

                [DefaultDataSource]
                public class DefaultSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
                {
                    public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                    {
                        return base.GetQuery(parameters);
                    }
                }
            }

            [Coalesce]
            public class TestEntityDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    if (User.IsInRole("Admin")) return query;
                    return query.Where(e => e.Name == User.Identity.Name);
                }
            }
            """);
    }

    [Test]
    public async Task DataSourceWithoutClaimsPrincipal_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [Coalesce]
            public class TestEntityDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    // No ClaimsPrincipal usage here
                    return base.GetQuery(parameters);
                }
            }
            """);
    }

    [Test]
    public async Task DefaultDataSourceWithClaimsPrincipal_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [Coalesce, DefaultDataSource]
            public class TestEntityDefaultDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    if (User.IsInRole("Admin")) return query;
                    return query.Where(e => e.Name == User.Identity.Name);
                }
            }
            """);
    }

    [Test]
    public async Task DataSourceWithClaimsPrincipalButNoDefaultDataSource_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            public class VulnerableEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [Coalesce]
            public class {|COA2001:VulnerableEntityDataSource|}(CrudContext<DbContext> context) : StandardDataSource<VulnerableEntity, DbContext>(context)
            {
                public override IQueryable<VulnerableEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    if (User.IsInRole("Admin")) return query;
                    return query.Where(e => e.Name == User.Identity.Name);
                }
            }
            """);
    }

    [Test]
    public async Task DataSourceWithClaimsPrincipalMethodCall_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            public class VulnerableEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [Coalesce]
            public class {|COA2001:VulnerableEntityDataSource|}(CrudContext<DbContext> context) : StandardDataSource<VulnerableEntity, DbContext>(context)
            {
                public override IQueryable<VulnerableEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    if (User.IsInRole("Admin")) return query;
                    return query.Where(e => e.Name == User.Identity.Name);
                }
            }
            """);
    }

    [Test]
    public async Task DataSourceWithClaimsPrincipalExtensionMethod_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            public static class ClaimsPrincipalExtensions
            {
                public static string? GetUserId(this ClaimsPrincipal user)
                    => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            public class VulnerableEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [Coalesce]
            public class {|COA2001:VulnerableEntityDataSource|}(CrudContext<DbContext> context) : StandardDataSource<VulnerableEntity, DbContext>(context)
            {
                public override IQueryable<VulnerableEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    var userId = User.GetUserId();
                    return query.Where(e => e.Id.ToString() == userId);
                }
            }
            """);
    }

    [Test]
    public async Task StandaloneEntityWithSingleDataSourceAndClaimsPrincipal_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, StandaloneEntity]
            public class StandaloneEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";

                public class DefaultSource(CrudContext context) : StandardDataSource<StandaloneEntity>(context)
                {
                    public override async Task<IQueryable<StandaloneEntity>> GetQueryAsync(IDataSourceParameters parameters)
                    {
                        // Standalone entities have no implicit fallback risk
                        var query = new[]{ new StandaloneEntity() }.AsQueryable();

                        if (User.IsInRole("Admin")) return query;
                        return query.Where(e => e.Name == User.Identity.Name);
                    }
                }
            }
            """);
    }
}
