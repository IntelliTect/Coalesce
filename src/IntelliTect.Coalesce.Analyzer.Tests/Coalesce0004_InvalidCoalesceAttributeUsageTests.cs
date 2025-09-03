namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0004_InvalidCoalesceAttributeUsageTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Fact]
    public async Task CoalesceAttributeOnDbContext_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class AppDbContext : DbContext
            {
            }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnDataSource_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class PersonDataSource(CrudContext<DbContext> context) : StandardDataSource<Person, DbContext>(context)
            {
            }

            public class Person { }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnBehaviors_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class PersonBehaviors(CrudContext<DbContext> context) : StandardBehaviors<Person, DbContext>(context)
            {
            }

            public class Person { }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnCustomDto_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class PersonDto : IClassDto<Person>
            {
                public int Id { get; set; }
                public void MapTo(Person entity, IMappingContext context) { }
                public void MapFrom(Person obj, IMappingContext context, IncludeTree? tree = null) { }
            }

            public class Person { }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnService_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public class PersonService
            {
            }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnStandaloneEntity_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, StandaloneEntity]
            public class Person
            {
                public int Id { get; set; }
            }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnSimpleModel_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, SimpleModel]
            public class Person
            {
                public int Id { get; set; }
            }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnRegularClass_ReportsError()
    {
        await VerifyAnalyzerAndCodeFixAsync<RemoveAttributeCodeFixProvider>("""
            [{|COA0004:Coalesce|}]
            public class RegularClass
            {
            }
            """, """
            public class RegularClass
            {
            }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnInterface_ReportsError()
    {
        await VerifyAnalyzerAndCodeFixAsync<RemoveAttributeCodeFixProvider>("""
            [{|COA0004:Coalesce|}]
            public interface IRegularInterface
            {
            }
            """, """
            public interface IRegularInterface
            {
            }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeWithMultipleAttributes_RemovesOnlyCoalesce()
    {
        await VerifyAnalyzerAndCodeFixAsync<RemoveAttributeCodeFixProvider>("""
            [{|COA0004:Coalesce|}, Obsolete]
            public class RegularClass
            {
            }
            """, """
            [Obsolete]
            public class RegularClass
            {
            }
            """);
    }

    [Fact]
    public async Task CoalesceAttributeOnSeparateAttributeList_RemovesEntireList()
    {
        await VerifyAnalyzerAndCodeFixAsync<RemoveAttributeCodeFixProvider>("""
            [Obsolete]
            [{|COA0004:Coalesce|}]
            public class RegularClass
            {
            }
            """, """
            [Obsolete]
            public class RegularClass
            {
            }
            """);
    }

    [Fact]
    public async Task NonCoalesceAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Obsolete]
            public class RegularClass
            {
            }
            """);
    }
}
