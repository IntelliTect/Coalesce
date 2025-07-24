using IntelliTect.Coalesce.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Testing;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0201_MissingFileTypeAttributeTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Fact]
    public async Task IFileParameterWithFileTypeAttribute_NoHint()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestService
            {
                [Coalesce]
                public void TestMethod([FileType("image/*")] IFile file)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task IFileParameterWithoutFileTypeAttributeOnCoalesceMethod_ReportsHint()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [Coalesce]
                public void TestMethod(IFile {|COA0201:file|})
                {
                }
            }
            """);
    }

    [Fact]
    public async Task IFileParameterWithoutFileTypeAttributeOnSemanticKernelMethod_ReportsHint()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [SemanticKernel]
                public void TestMethod(IFile {|COA0201:file|})
                {
                }
            }
            """);
    }

    [Fact]
    public async Task NonIFileParameter_NoHint()
    {
        await VerifyAnalyzerAsync("""
            public class TestService
            {
                [Coalesce]
                public void TestMethod(string parameter)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task MultipleIFileParameters_ReportsHintForEach()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestService
            {
                [Coalesce]
                public void TestMethod(FileParameter {|COA0201:file1|}, [FileType("image/*")] IFile file2, IFile {|COA0201:file3|})
                {
                }
            }
            """);
    }

    [Fact]
    public async Task CodeFix_AddsFileTypeAttributeToIFileParameter()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0201_MissingFileTypeAttributeCodeFixProvider>("""
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [Coalesce]
                public void TestMethod(IFile {|COA0201:file|})
                {
                }
            }
            """, """
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [Coalesce]
                public void TestMethod([FileType("*/*")] IFile file)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task CodeFix_AddsFileTypeAttributeToMultipleParameters()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0201_MissingFileTypeAttributeCodeFixProvider>("""
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [Coalesce]
                public void TestMethod(IFile {|COA0201:file1|}, IFile {|COA0201:file2|})
                {
                }
            }
            """, """
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [Coalesce]
                public void TestMethod([FileType("*/*")] IFile file1, [FileType("*/*")] IFile file2)
                {
                }
            }
            """);
    }

    [Fact]
    public async Task CodeFix_PreservesIndentationForParameterOnOwnLine()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce0201_MissingFileTypeAttributeCodeFixProvider>("""
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [Coalesce]
                public void TestMethod(
                    IFile {|COA0201:file|})
                {
                }
            }
            """, """
            using IntelliTect.Coalesce.Models;

            public class TestService
            {
                [Coalesce]
                public void TestMethod(
                    [FileType("*/*")] IFile file)
                {
                }
            }
            """);
    }
}
