namespace IntelliTect.Coalesce.Analyzer.Tests;

public class GetQueryOrderingAnalyzerTests : CSharpAnalyzerVerifier<GetQueryOrderingAnalyzer>
{
    [Test]
    public async Task GetQuery_WithOrderBy_ReportsWarning()
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
                    var query = base.GetQuery(parameters);
                    return {|COA0012:query.OrderBy(e => e.Name)|};
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_WithOrderByDescending_ReportsWarning()
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
                    var query = base.GetQuery(parameters);
                    return {|COA0012:query.OrderByDescending(e => e.Name)|};
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_WithThenBy_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
                public int Priority { get; set; }
            }

            [Coalesce]
            public class TestEntityDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    var ordered = {|COA0012:query.OrderBy(e => e.Priority)|};
                    return {|COA0012:ordered.ThenBy(e => e.Name)|};
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_WithThenByDescending_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
                public int Priority { get; set; }
            }

            [Coalesce]
            public class TestEntityDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    var ordered = {|COA0012:query.OrderBy(e => e.Priority)|};
                    return {|COA0012:ordered.ThenByDescending(e => e.Name)|};
                }
            }
            """);
    }

    [Test]
    public async Task GetQueryAsync_WithOrderBy_ReportsWarning()
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
                public override async Task<IQueryable<TestEntity>> GetQueryAsync(IDataSourceParameters parameters)
                {
                    var query = await base.GetQueryAsync(parameters);
                    return {|COA0012:query.OrderBy(e => e.Name)|};
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_WithOrderByInSubquery_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class RelatedEntity
            {
                public int Id { get; set; }
                public int TestEntityId { get; set; }
                public string Value { get; set; } = "";
            }

            [Coalesce]
            public class TestEntityDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    return query.Where(e => Db.Set<RelatedEntity>()
                        .Where(r => r.TestEntityId == e.Id)
                        .OrderBy(r => r.Value)
                        .Any());
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_WithOrderByInSelectSubquery_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class RelatedEntity
            {
                public int Id { get; set; }
                public int TestEntityId { get; set; }
                public string Value { get; set; } = "";
            }

            [Coalesce]
            public class TestEntityDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    return query.Select(e => new TestEntity
                    {
                        Id = e.Id,
                        Name = Db.Set<RelatedEntity>()
                            .Where(r => r.TestEntityId == e.Id)
                            .OrderBy(r => r.Value)
                            .Select(r => r.Value)
                            .FirstOrDefault() ?? ""
                    });
                }
            }
            """);
    }

    [Test]
    public async Task NonGetQueryMethod_WithOrderBy_NoWarning()
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
                public override IQueryable<TestEntity> ApplyListDefaultSorting(IQueryable<TestEntity> query)
                {
                    return query.OrderBy(e => e.Name);
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_WithIncludeButNoOrdering_NoWarning()
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
                    var query = base.GetQuery(parameters);
                    return query.Where(e => e.Name != null);
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_WithMultipleOrderingOperations_ReportsMultipleWarnings()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
                public int Priority { get; set; }
            }

            [Coalesce]
            public class TestEntityDataSource(CrudContext<DbContext> context) : StandardDataSource<TestEntity, DbContext>(context)
            {
                public override IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = base.GetQuery(parameters);
                    return {|COA0012:{|COA0012:query.OrderBy(e => e.Priority)|}.ThenBy(e => e.Name)|};
                }
            }
            """);
    }

    [Test]
    public async Task GetQuery_InClassNotInheritingFromStandardDataSource_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class SomeOtherClass
            {
                public IQueryable<TestEntity> GetQuery(IDataSourceParameters parameters)
                {
                    var query = GetData();
                    return query.OrderBy(e => e.Name);
                }

                private IQueryable<TestEntity> GetData()
                {
                    return new List<TestEntity>().AsQueryable();
                }
            }
            """);
    }
}
