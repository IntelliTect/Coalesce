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
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Read(SecurityPermissionLevels.AllowAll)]
        public string {|COA001:TestProperty|} { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task ReadAttribute_OnProperty_WithoutPermissionLevel_DoesNotReportDiagnostic()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

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

            await VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task ReadAttribute_OnClass_WithPermissionLevel_DoesNotReportDiagnostic()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    [Read(SecurityPermissionLevels.AllowAuthenticated)]
    public class TestClass
    {
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task ReadAttribute_OnProperty_CodeFix_RemovesPermissionLevel()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Read(SecurityPermissionLevels.AllowAll)]
        public string {|COA001:TestProperty|} { get; set; }
    }
}";

            var fixedCode = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Read]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, fixedCode);
        }

        [Fact]
        public async Task ReadAttribute_OnProperty_CodeFix_RemovesPermissionLevel_KeepsRoles()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Read(SecurityPermissionLevels.AllowAll, ""Admin"")]
        public string {|COA001:TestProperty|} { get; set; }
    }
}";

            var fixedCode = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Read(""Admin"")]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, fixedCode);
        }

        private static async Task VerifyCodeFixAsync(string source, string fixedSource)
        {
            var test = new CSharpCodeFixTest<ReadAttributePermissionLevelAnalyzer, ReadAttributePermissionLevelCodeFixProvider, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };

            // Add reference to IntelliTect.Coalesce
            test.TestState.AdditionalReferences.Add(typeof(IntelliTect.Coalesce.DataAnnotations.ReadAttribute).Assembly);

            await test.RunAsync();
        }
    }
}