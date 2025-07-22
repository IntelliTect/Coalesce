using IntelliTect.Coalesce.Analyzer.Analyzers;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0002_InvalidInjectAttributeUsageTests : CSharpAnalyzerVerifier<Coalesce0002_InvalidInjectAttributeUsage>
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
        await VerifyAnalyzerAsync("""
            public class TestService
            {
                public void TestMethod([{|COALESCE0002:Inject|}] IServiceProvider serviceProvider)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnRegularInterfaceMethod_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            public interface ITestInterface
            {
                void TestMethod([{|COALESCE0002:Inject|}] IServiceProvider serviceProvider);
            }
            """);
    }

    [Fact]
    public async Task InjectAttributeOnServiceClassMethod_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public class TestService
            {
                public void TestMethod([{|COALESCE0002:Inject|}] IServiceProvider serviceProvider)
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
}
