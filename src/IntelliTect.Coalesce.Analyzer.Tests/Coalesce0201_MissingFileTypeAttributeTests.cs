using IntelliTect.Coalesce.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0201_MissingFileTypeAttributeTests : CSharpAnalyzerVerifier<AttributeUsageAnalyzer>
{
    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
    public async Task IFileParameterOnServiceInterfaceMethod_ReportsHint()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;

            [Coalesce, Service]
            public interface ITestService
            {
                void TestMethod(IFile {|COA0201:file|});
            }
            """);
    }

    [Test]
    public async Task IFileParameterOnServiceClassImplementation_NoHint()
    {
        // Should not suggest FileType on implementation methods when service is exposed via interface
        // but SHOULD suggest on the interface method
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;

            [Coalesce, Service]
            public interface ITestService
            {
                void TestMethod(IFile {|COA0201:file|});
            }

            public class TestService : ITestService
            {
                public void TestMethod(IFile file)
                {
                }
            }
            """);
    }

    [Test]
    public async Task IFileParameterOnServiceClass_ReportsHint()
    {
        // Should suggest FileType on service class methods when class is directly exposed (no interface) and method has [Coalesce]
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;

            [Coalesce, Service]
            public class TestService
            {
                [Coalesce]
                public void TestMethod(IFile {|COA0201:file|})
                {
                }
            }
            """);
    }
}
