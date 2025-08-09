using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<IntelliTect.Coalesce.Analyzers.FileTypeAttributeAnalyzer>;

namespace IntelliTect.Coalesce.Analyzers.Tests
{
    public class FileTypeAttributeAnalyzerTests
    {
        [Fact]
        public async Task ShouldSuggestFileTypeForIFormFileParameter()
        {
            var testCode = @"
using Microsoft.AspNetCore.Http;

public class TestClass
{
    public void TestMethod(IFormFile {|#0:file|})
    {
    }
}";

            var expected = Verifier.Diagnostic("COA001")
                .WithLocation(0)
                .WithArguments("file");

            await Verifier.VerifyAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task ShouldNotSuggestWhenFileTypeAttributePresent()
        {
            var testCode = @"
using Microsoft.AspNetCore.Http;
using IntelliTect.Coalesce.DataAnnotations;

public class TestClass
{
    public void TestMethod([FileType("".jpg"", "".png"")] IFormFile file)
    {
    }
}";

            await Verifier.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ShouldNotSuggestForNonFileParameters()
        {
            var testCode = @"
public class TestClass
{
    public void TestMethod(string text, int number)
    {
    }
}";

            await Verifier.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ShouldNotSuggestForServiceMethodExposedByInterface()
        {
            var testCode = @"
using Microsoft.AspNetCore.Http;

public interface ITestService
{
    void ProcessFile(IFormFile file);
}

public class TestService : ITestService
{
    public void ProcessFile(IFormFile file)
    {
    }
}";

            await Verifier.VerifyAnalyzerAsync(testCode);
        }
    }
}