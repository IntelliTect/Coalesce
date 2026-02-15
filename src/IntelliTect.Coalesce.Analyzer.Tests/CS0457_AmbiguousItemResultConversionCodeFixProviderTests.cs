using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using IntelliTect.Coalesce.Analyzer.Fixers;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Analyzer.Tests;


public class CS0457_AmbiguousItemResultConversionCodeFixProviderTests : CSharpAnalyzerVerifier<EmptyDiagnosticAnalyzer>
{
    [Test]
    public async Task AmbiguousConversion_VariableAssignment_OffersCorrectFixes()
    {
        var source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    string message = "test";
                    ItemResult<string> result = {|CS0457:message|};
                }
            }
            """;

        var successfulResultFixed = """
            public class TestClass
            {
                public void TestMethod()
                {
                    string message = "test";
                    ItemResult<string> result = new(true, obj: message);
                }
            }
            """;

        var errorResultFixed = """
            public class TestClass
            {
                public void TestMethod()
                {
                    string message = "test";
                    ItemResult<string> result = new(false, message: message);
                }
            }
            """;

        // Test the successful result fix (first code action, index 0)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, successfulResultFixed, 0);

        // Test the error result fix (second code action, index 1)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, errorResultFixed, 1);
    }

    [Test]
    public async Task AmbiguousConversion_ReturnStatement_OffersCorrectFixes()
    {
        var source = """
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    string message = "test";
                    return message;
                }
            }
            """;

        var successfulResultFixed = """
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    string message = "test";
                    return new(true, obj: message);
                }
            }
            """;

        var errorResultFixed = """
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    string message = "test";
                    return new(false, message: message);
                }
            }
            """;

        var expectedDiagnostic = DiagnosticResult.CompilerError("CS0457").WithSpan(6, 16, 6, 23).WithArguments(
            "IntelliTect.Coalesce.Models.ItemResult<string>.implicit operator IntelliTect.Coalesce.Models.ItemResult<string>(string?)",
            "IntelliTect.Coalesce.Models.ItemResult<string>.implicit operator IntelliTect.Coalesce.Models.ItemResult<string>(string)",
            "string",
            "IntelliTect.Coalesce.Models.ItemResult<string>");

        // Test the successful result fix (first code action, index 0)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, successfulResultFixed, 0, expectedDiagnostic, expectedDiagnostic);

        // Test the error result fix (second code action, index 1)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, errorResultFixed, 1, expectedDiagnostic, expectedDiagnostic);
    }

    [Test]
    public async Task AmbiguousConversion_VariableDeclaration_OffersCorrectFixes()
    {
        var source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    ItemResult<string> result = {|CS0457:"error message"|};
                }
            }
            """;

        var successfulResultFixed = """
            public class TestClass
            {
                public void TestMethod()
                {
                    ItemResult<string> result = new(true, obj: "error message");
                }
            }
            """;

        var errorResultFixed = """
            public class TestClass
            {
                public void TestMethod()
                {
                    ItemResult<string> result = new(false, message: "error message");
                }
            }
            """;

        // Test the successful result fix (first code action, index 0)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, successfulResultFixed, 0);

        // Test the error result fix (second code action, index 1)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, errorResultFixed, 1);
    }

    [Test]
    public async Task AmbiguousConversion_StringLiteral_OffersCorrectFixes()
    {
        var source = """
            public class TestClass
            {
                public ItemResult<string> GetErrorResult() => "Something went wrong";
            }
            """;

        var successfulResultFixed = """
            public class TestClass
            {
                public ItemResult<string> GetErrorResult() => new(true, obj: "Something went wrong");
            }
            """;

        var errorResultFixed = """
            public class TestClass
            {
                public ItemResult<string> GetErrorResult() => new(false, message: "Something went wrong");
            }
            """;

        var expectedDiagnostic = DiagnosticResult.CompilerError("CS0457").WithSpan(3, 51, 3, 73).WithArguments(
            "IntelliTect.Coalesce.Models.ItemResult<string>.implicit operator IntelliTect.Coalesce.Models.ItemResult<string>(string?)",
            "IntelliTect.Coalesce.Models.ItemResult<string>.implicit operator IntelliTect.Coalesce.Models.ItemResult<string>(string)",
            "string",
            "IntelliTect.Coalesce.Models.ItemResult<string>");

        // Test the successful result fix (first code action, index 0)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, successfulResultFixed, 0, expectedDiagnostic, expectedDiagnostic);

        // Test the error result fix (second code action, index 1)
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, errorResultFixed, 1, expectedDiagnostic, expectedDiagnostic);
    }

    [Test]
    public async Task NonItemResultAmbiguousConversion_DoesNotOfferFix()
    {
        var source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    // Create a real CS0457 scenario similar to ItemResult but not ItemResult
                    ConvertibleType<string> result = {|CS0457:"test"|};
                }
            }
            
            public class ConvertibleType<T>
            {
                // Create ambiguous implicit conversions like ItemResult has
                public static implicit operator ConvertibleType<T>(T value) => new ConvertibleType<T>();
                public static implicit operator ConvertibleType<T>(string errorMessage) => new ConvertibleType<T>();
            }
            """;

        // This test verifies that we don't offer fixes for non-ItemResult CS0457 errors
        // The source and fixedSource are the same because no fix should be applied
        await VerifyCodeFixAsync<CS0457_AmbiguousItemResultConversionCodeFixProvider>(source, source);
    }
}
