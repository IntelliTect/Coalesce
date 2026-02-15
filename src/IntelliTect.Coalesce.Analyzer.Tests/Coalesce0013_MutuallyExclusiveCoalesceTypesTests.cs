namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0013_MutuallyExclusiveCoalesceTypesTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Test]
    public async Task ServiceAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public class MyService
            {
            }
            """);
    }

    [Test]
    public async Task StandaloneEntityAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, StandaloneEntity]
            public class MyEntity
            {
            }
            """);
    }

    [Test]
    public async Task SimpleModelAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, SimpleModel]
            public class MyModel
            {
            }
            """);
    }

    [Test]
    public async Task DbContextInheritance_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class AppDbContext : DbContext
            {
            }
            """);
    }

    [Test]
    public async Task IDataSourceImplementation_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class PersonDataSource(CrudContext<DbContext> context) : StandardDataSource<Person, DbContext>(context)
            {
            }

            public class Person { }
            """);
    }

    [Test]
    public async Task IBehaviorsImplementation_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class PersonBehaviors(CrudContext<DbContext> context) : StandardBehaviors<Person, DbContext>(context)
            {
            }

            public class Person { }
            """);
    }

    [Test]
    public async Task IClassDtoImplementation_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce]
            public class PersonDto : IClassDto<Person>
            {
                public void MapTo(Person entity, IMappingContext context) { }
                public void MapFrom(Person entity, IMappingContext context, IncludeTree tree = null) { }
            }

            public class Person { }
            """);
    }

    [Test]
    public async Task ServiceAndStandaloneEntity_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service, StandaloneEntity]
            public class {|COA0013:MyClass|}
            {
            }
            """);
    }

    [Test]
    public async Task ServiceAndSimpleModel_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service, SimpleModel]
            public class {|COA0013:MyClass|}
            {
            }
            """);
    }

    [Test]
    public async Task StandaloneEntityAndSimpleModel_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, StandaloneEntity, SimpleModel]
            public class {|COA0013:MyClass|}
            {
            }
            """);
    }

    [Test]
    public async Task ServiceAndDbContext_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public class {|COA0013:MyDbContext|} : DbContext
            {
            }
            """);
    }

    [Test]
    public async Task StandaloneEntityAndIDataSource_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, StandaloneEntity]
            public class {|COA0013:PersonDataSource|}(CrudContext<DbContext> context) : StandardDataSource<Person, DbContext>(context)
            {
            }

            public class Person { }
            """);
    }

    [Test]
    public async Task SimpleModelAndIBehaviors_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, SimpleModel]
            public class {|COA0013:PersonBehaviors|}(CrudContext<DbContext> context) : StandardBehaviors<Person, DbContext>(context)
            {
            }

            public class Person { }
            """);
    }

    [Test]
    public async Task ServiceAndIClassDto_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public class {|COA0013:PersonDto|} : IClassDto<Person>
            {
                public void MapTo(Person entity, IMappingContext context) { }
                public void MapFrom(Person entity, IMappingContext context, IncludeTree tree = null) { }
            }

            public class Person { }
            """);
    }
    [Test]
    public async Task MultipleAttributes_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service, StandaloneEntity, SimpleModel]
            public class {|COA0013:MyClass|}
            {
            }
            """);
    }
}
