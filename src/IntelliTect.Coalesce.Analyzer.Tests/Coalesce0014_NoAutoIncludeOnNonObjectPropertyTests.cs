using IntelliTect.Coalesce.Analyzer.Analyzers;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0014_NoAutoIncludeOnNonObjectPropertyTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Theory]
    [InlineData("string")]
    [InlineData("int")]
    [InlineData("int?")]
    [InlineData("DateTime")]
    [InlineData("DateOnly")]
    [InlineData("Guid")]
    [InlineData("byte[]")]
    [InlineData("bool")]
    [InlineData("decimal")]
    public async Task NoAutoInclude_OnNonObjectProperty_ReportsWarning(string propertyType)
    {
        await VerifyAnalyzerAsync($$"""
            public class TestClass
            {
                [{|COA0014:Read(NoAutoInclude = true)|}]
                public {{propertyType}} TestProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task NoAutoInclude_OnEnumProperty_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            public enum TestEnum { A, B, C }

            public class TestClass
            {
                [{|COA0014:Read(NoAutoInclude = true)|}]
                public TestEnum TestProperty { get; set; }
            }
            """);
    }

    [Theory]
    [InlineData("RelatedClass")]
    [InlineData("List<RelatedClass>")]
    [InlineData("ICollection<RelatedClass>")]
    [InlineData("IEnumerable<RelatedClass>")]
    [InlineData("RelatedClass[]")]
    [InlineData("IRelatedClass")]
    public async Task NoAutoInclude_OnObjectOrCollectionProperty_NoWarning(string propertyType)
    {
        await VerifyAnalyzerAsync($$"""
            public interface IRelatedClass { int Id { get; } }
            public class RelatedClass { public int Id { get; set; } }

            public class TestClass
            {
                [Read(NoAutoInclude = true)]
                public {{propertyType}} TestProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task NoAutoInclude_False_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                [Read(NoAutoInclude = false)]
                public string TestProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task NoAutoInclude_NotSpecified_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                [Read("Admin")]
                public string TestProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task NoAutoInclude_OnClass_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            [Read(NoAutoInclude = true)]
            public class TestClass
            {
                public string TestProperty { get; set; }
            }
            """);
    }
}
