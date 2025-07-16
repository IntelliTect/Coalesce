using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.Analyzers.Test
{
    public class ReadAttributePermissionLevelAnalyzerTests
    {
        [Fact]
        public async Task ReadAttribute_OnProperty_WithPermissionLevel_ReportsDiagnostic()
        {
            var testCode = TestCode.TestAttributes + @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Read(SecurityPermissionLevels.AllowAll)]
        public string {|COA001:TestProperty|} { get; set; }
    }
}";

            await VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReadAttribute_OnProperty_WithoutPermissionLevel_DoesNotReportDiagnostic()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Read]
        public string TestProperty { get; set; }
        
        [Read(""Admin"", ""User"")]
        public string TestProperty2 { get; set; }
    }
}";

            await VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReadAttribute_OnClass_WithPermissionLevel_DoesNotReportDiagnostic()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    [Read(SecurityPermissionLevels.AllowAuthenticated)]
    public class TestClass
    {
        public string TestProperty { get; set; }
    }
}";

            await VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReadAttribute_OnProperty_CodeFix_RemovesPermissionLevel()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Read(SecurityPermissionLevels.AllowAll)]
        public string {|COA001:TestProperty|} { get; set; }
    }
}";

            var fixedCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Read]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task ReadAttribute_OnProperty_CodeFix_RemovesPermissionLevel_KeepsRoles()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Read(SecurityPermissionLevels.AllowAll, ""Admin"")]
        public string {|COA001:TestProperty|} { get; set; }
    }
}";

            var fixedCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Read(""Admin"")]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        private static async Task VerifyAnalyzerAsync(string source)
        {
            var test = new CSharpAnalyzerTest<ReadAttributePermissionLevelAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };

            await test.RunAsync();
        }

        private static async Task VerifyCodeFixAsync(string source, string fixedSource)
        {
            var test = new CSharpCodeFixTest<ReadAttributePermissionLevelAnalyzer, ReadAttributePermissionLevelCodeFixProvider, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };

            await test.RunAsync();
        }
    }
}