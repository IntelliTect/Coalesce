using IntelliTect.Coalesce.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Testing;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0002_InvalidInjectAttributeUsageTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Fact]
    public async Task InjectAttributeOnCoalesceMethod_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestService
            {
                [Coalesce]
                public void TestMethod([Inject] IServiceProvider serviceProvider)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnSemanticKernelMethod_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestService
            {
                [SemanticKernel]
                public void TestMethod([Inject] IServiceProvider serviceProvider)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnServiceInterfaceMethod_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public interface ITestService
            {
                void TestMethod([Inject] IServiceProvider serviceProvider);
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnMethodWithoutCoalesceAttribute_ReportsWarning()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0002_InvalidInjectAttributeUsageCodeFixProvider>("""
            public class TestService
            {
                public void TestMethod([{|COALESCE0002:Inject|}] IServiceProvider serviceProvider)
                {
                }
            }
            """, """
            public class TestService
            {
                public void TestMethod(IServiceProvider serviceProvider)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnRegularInterfaceMethod_ReportsWarning()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0002_InvalidInjectAttributeUsageCodeFixProvider>("""
            public interface ITestInterface
            {
                void TestMethod([{|COALESCE0002:Inject|}] IServiceProvider serviceProvider);
            }
            """, """
            public interface ITestInterface
            {
                void TestMethod(IServiceProvider serviceProvider);
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnServiceClassMethod_ReportsWarning()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0002_InvalidInjectAttributeUsageCodeFixProvider>("""
            [Coalesce, Service]
            public class TestService
            {
                public void TestMethod([{|COALESCE0002:Inject|}] IServiceProvider serviceProvider)
                {
                }
            }
            """, """
            [Coalesce, Service]
            public class TestService
            {
                public void TestMethod(IServiceProvider serviceProvider)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnServiceClassMethodWithCoalesce_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public class TestService
            {
                [Coalesce]
                public void TestMethod([Inject] IServiceProvider serviceProvider)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnServiceClassImplementation_NoWarning()
    {
        // Must allow the same attributes as the interface
        // because other kinds of code analysis will get mad if your attributes differ from the interface being implemented.
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public interface ITestService
            {
                void TestMethod([Inject] IServiceProvider serviceProvider);
            }

            public class TestService : ITestService
            {
                public void TestMethod([Inject] IServiceProvider serviceProvider)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task MultipleParametersWithMixedAttributes_ReportsCorrectWarnings()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                [Coalesce]
                public void ValidMethod([Inject] IServiceProvider serviceProvider, string normalParam)
                {
                }

                public void InvalidMethod([{|COALESCE0002:Inject|}] IServiceProvider serviceProvider, string normalParam)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task NonInjectAttribute_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class InjectAttribute : Attribute { }

            public class TestClass
            {
                public void TestMethod([Inject] string parameter)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnConstructorParameter_ReportsWarning()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0002_InvalidInjectAttributeUsageCodeFixProvider>("""
            public class Person
            {
                public int Id { get; set; }
                public class PersonBehaviors(
                    [{|COALESCE0002:Inject|}] CrudContext<DbContext> context
                ) : StandardBehaviors<Person, DbContext>(context)
                {
                }
            }
            """, """
            public class Person
            {
                public int Id { get; set; }
                public class PersonBehaviors(
                    CrudContext<DbContext> context
                ) : StandardBehaviors<Person, DbContext>(context)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeCodeFix_RemovesAttributeFromList()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0002_InvalidInjectAttributeUsageCodeFixProvider>("""
            public class TestService
            {
                public void TestMethod([SomeOtherAttribute, {|COALESCE0002:Inject|}] IServiceProvider serviceProvider)
                {
                }
            }

            public class SomeOtherAttribute : Attribute { }
            """, """
            public class TestService
            {
                public void TestMethod([SomeOtherAttribute] IServiceProvider serviceProvider)
                {
                }
            }

            public class SomeOtherAttribute : Attribute { }
            """);
    }
}
