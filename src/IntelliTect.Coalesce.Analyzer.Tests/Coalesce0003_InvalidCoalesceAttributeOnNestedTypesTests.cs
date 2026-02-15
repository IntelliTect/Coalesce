namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0003_InvalidCoalesceAttributeOnNestedTypesTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Test]
    public async Task CoalesceAttributeOnTopLevelDataSource_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class MyDataSource(CrudContext<DbContext> context) : StandardDataSource<MyModel, DbContext>(context)
            {
            }
            
            public class MyModel { }
            """);
    }

    [Test]
    public async Task CoalesceAttributeOnTopLevelBehaviors_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class MyBehaviors(CrudContext<DbContext> context) : StandardBehaviors<MyModel, DbContext>(context)
            {
            }
            
            public class MyModel { }
            """);
    }

    [Test]
    public async Task CoalesceAttributeOnNestedDataSource_ReportsWarning()
    {
        await VerifyAnalyzerAndCodeFixAsync<RemoveAttributeCodeFixProvider>("""
            public class MyModel
            {
                [{|COA0003:Coalesce|}]
                public class DefaultSource(CrudContext<DbContext> context) : StandardDataSource<MyModel, DbContext>(context)
                {
                }
            }
            """, """
            public class MyModel
            {
                public class DefaultSource(CrudContext<DbContext> context) : StandardDataSource<MyModel, DbContext>(context)
                {
                }
            }
            """);
    }

    [Test]
    public async Task CoalesceAttributeOnNestedBehaviors_ReportsWarning()
    {
        await VerifyAnalyzerAndCodeFixAsync<RemoveAttributeCodeFixProvider>("""
            public class MyModel
            {
                public int Id { get; set; }

                [{|COA0003:Coalesce|}]
                public class Behaviors(CrudContext<DbContext> context) : StandardBehaviors<MyModel, DbContext>(context)
                {
                }
            }
            """, """
            public class MyModel
            {
                public int Id { get; set; }

                public class Behaviors(CrudContext<DbContext> context) : StandardBehaviors<MyModel, DbContext>(context)
                {
                }
            }
            """);
    }

    [Test]
    public async Task CoalesceAttributeWithMultipleAttributes_RemovesOnlyCoalesce()
    {
        await VerifyAnalyzerAndCodeFixAsync<RemoveAttributeCodeFixProvider>("""
            public class MyModel
            {
                public int Id { get; set; }

                [SomeOtherAttribute, {|COA0003:Coalesce|}]
                public class DefaultSource(CrudContext<DbContext> context) : StandardDataSource<MyModel, DbContext>(context)
                {
                }
            }
            
            public class SomeOtherAttributeAttribute : System.Attribute { }
            """, """
            public class MyModel
            {
                public int Id { get; set; }

                [SomeOtherAttribute]
                public class DefaultSource(CrudContext<DbContext> context) : StandardDataSource<MyModel, DbContext>(context)
                {
                }
            }
            
            public class SomeOtherAttributeAttribute : System.Attribute { }
            """);
    }
}
