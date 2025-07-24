using IntelliTect.Coalesce.Analyzer.Analyzers;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0005_UnexposedSecondaryAttributeTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Fact]
    public async Task ServiceWithCoalesceAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Coalesce, Service]
            public class PersonService
            {
            }
            """);
    }

    [Fact]
    public async Task StandaloneEntityWithCoalesceAttribute_NoError()
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
    public async Task ExecuteAttributeWithCoalesceAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            public class PersonService
            {
                [Coalesce, Execute]
                public void DoSomething() { }
            }
            """);
    }

    [Fact]
    public async Task ExecuteAttributeWithSemanticKernelAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            public class PersonService
            {
                [SemanticKernel, Execute]
                public void DoSomething() { }
            }
            """);
    }

    [Fact]
    public async Task ExecuteAttributeOnServiceClass_ReportsError()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0005_UnexposedSecondaryAttributeCodeFixProvider>("""
            [Coalesce, Service]
            public class PersonService
            {
                [{|COA0006:Execute|}]
                public void DoSomething() { }
            }
            """, """
            [Coalesce, Service]
            public class PersonService
            {
                [Coalesce, Execute]
                public void DoSomething() { }
            }
            """);
    }

    [Fact]
    public async Task ServiceClassAlone_ReportsError()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0005_UnexposedSecondaryAttributeCodeFixProvider>("""
            [{|COA0005:Service|}]
            public class PersonService
            {
            }
            """, """
            [Coalesce, Service]
            public class PersonService
            {
            }
            """);
    }

    [Fact]
    public async Task StandaloneEntityWithoutCoalesceOrSemanticKernel_ReportsError()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0005_UnexposedSecondaryAttributeCodeFixProvider>("""
            [{|COA0005:StandaloneEntity|}]
            public class Person
            {
                public int Id { get; set; }
            }
            """, """
            [Coalesce, StandaloneEntity]
            public class Person
            {
                public int Id { get; set; }
            }
            """);
    }

    [Fact]
    public async Task ExecuteAttributeOnSimpleMethod_ReportsError()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0005_UnexposedSecondaryAttributeCodeFixProvider>("""
            public class TestService
            {
                [{|COA0006:Execute|}]
                public void DoSomething() { }
            }
            """, """
            public class TestService
            {
                [Coalesce, Execute]
                public void DoSomething() { }
            }
            """);
    }

    [Fact]
    public async Task ServiceWithExistingAttributes_AddsCoalesceFirst()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0005_UnexposedSecondaryAttributeCodeFixProvider>("""
            [Obsolete]
            [{|COA0005:StandaloneEntity|}]
            public class PersonService
            {
            }
            """, """
            [Obsolete]
            [Coalesce, StandaloneEntity]
            public class PersonService
            {
            }
            """);
    }

    [Fact]
    public async Task RegularClassWithoutSpecialAttributes_NoError()
    {
        await VerifyAnalyzerAsync("""
            public class RegularClass
            {
                public void DoSomething() { }
            }
            """);
    }

    [Fact]
    public async Task RegularMethodWithoutExecuteAttribute_NoError()
    {
        await VerifyAnalyzerAsync("""
            public class RegularClass
            {
                [Obsolete]
                public void DoSomething() { }
            }
            """);
    }
}
