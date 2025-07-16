using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.Analyzers.Test
{
    public class EditAttributePermissionLevelAnalyzerTests
    {
        [Fact]
        public async Task EditAttribute_OnProperty_WithPermissionLevel_ReportsDiagnostic()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Edit(SecurityPermissionLevels.AllowAll)]
        public string {|COA002:TestProperty|} { get; set; }
    }
}";

            await VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EditAttribute_OnProperty_WithoutPermissionLevel_DoesNotReportDiagnostic()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Edit]
        public string TestProperty { get; set; }
        
        [Edit(""Admin"", ""User"")]
        public string TestProperty2 { get; set; }
    }
}";

            await VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EditAttribute_OnClass_WithPermissionLevel_DoesNotReportDiagnostic()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    [Edit(SecurityPermissionLevels.AllowAuthenticated)]
    public class TestClass
    {
        public string TestProperty { get; set; }
    }
}";

            await VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EditAttribute_OnProperty_CodeFix_RemovesPermissionLevel()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Edit(SecurityPermissionLevels.AllowAll)]
        public string {|COA002:TestProperty|} { get; set; }
    }
}";

            var fixedCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Edit]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task EditAttribute_OnProperty_CodeFix_RemovesPermissionLevel_KeepsRoles()
        {
            var testCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Edit(SecurityPermissionLevels.AllowAll, ""Admin"")]
        public string {|COA002:TestProperty|} { get; set; }
    }
}";

            var fixedCode = TestCode.TestAttributes + @"
namespace TestNamespace
{
    public class TestClass
    {
        [Edit(""Admin"")]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        private static async Task VerifyAnalyzerAsync(string source)
        {
            var test = new CSharpAnalyzerTest<EditAttributePermissionLevelAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };

            await test.RunAsync();
        }

        private static async Task VerifyCodeFixAsync(string source, string fixedSource)
        {
            var test = new CSharpCodeFixTest<EditAttributePermissionLevelAnalyzer, EditAttributePermissionLevelCodeFixProvider, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };

            await test.RunAsync();
        }
    }
}