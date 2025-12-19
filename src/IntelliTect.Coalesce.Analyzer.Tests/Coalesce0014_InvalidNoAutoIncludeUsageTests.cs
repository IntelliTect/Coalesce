namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0014_InvalidNoAutoIncludeUsageTests : CSharpAnalyzerVerifier<Coalesce0014_InvalidNoAutoIncludeUsage>
{
    [Fact]
    public async Task PlainPropertyWithNoAutoInclude_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [{|COA0014:Read(NoAutoInclude = true)|}]
                public string Name { get; set; }
            }
            """);
    }

    [Fact]
    public async Task IntPropertyWithNoAutoInclude_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [{|COA0014:Read(NoAutoInclude = true)|}]
                public int Age { get; set; }
            }
            """);
    }

    [Fact]
    public async Task ReferenceNavigationWithNoAutoInclude_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                public int RelatedId { get; set; }

                [Read(NoAutoInclude = true)]
                [ForeignKey(nameof(RelatedId))]
                public RelatedClass Related { get; set; }
            }

            public class RelatedClass
            {
                public int Id { get; set; }
            }
            """);
    }

    [Fact]
    public async Task CollectionNavigationWithNoAutoInclude_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                public int Id { get; set; }

                [Read(NoAutoInclude = true)]
                [InverseProperty(nameof(RelatedClass.Parent))]
                public ICollection<RelatedClass> Children { get; set; }
            }

            public class RelatedClass
            {
                public int Id { get; set; }
                public int ParentId { get; set; }
                [ForeignKey(nameof(ParentId))]
                public TestClass Parent { get; set; }
            }
            """);
    }

    [Fact]
    public async Task PropertyWithoutNoAutoInclude_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [Read("Admin")]
                public string Name { get; set; }
            }
            """);
    }

    [Fact]
    public async Task PropertyWithNoAutoIncludeFalse_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [Read(NoAutoInclude = false)]
                public string Name { get; set; }
            }
            """);
    }

    [Fact]
    public async Task ClassLevelNoAutoInclude_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            [Read(NoAutoInclude = true)]
            public class TestClass
            {
                public int Id { get; set; }
            }
            """);
    }

    [Fact]
    public async Task NonCoalesceReadAttribute_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using System;

            public class ReadAttribute : Attribute
            {
                public bool NoAutoInclude { get; set; }
            }

            public class TestClass
            {
                [Read(NoAutoInclude = true)]
                public string Name { get; set; }
            }
            """);
    }

    [Fact]
    public async Task DateTimePropertyWithNoAutoInclude_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [{|COA0014:Read(NoAutoInclude = true)|}]
                public DateTime CreatedAt { get; set; }
            }
            """);
    }

    [Fact]
    public async Task ForeignKeyPropertyWithNoAutoInclude_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [{|COA0014:Read(NoAutoInclude = true)|}]
                public int RelatedId { get; set; }

                [ForeignKey(nameof(RelatedId))]
                public RelatedClass Related { get; set; }
            }

            public class RelatedClass
            {
                public int Id { get; set; }
            }
            """);
    }
}
