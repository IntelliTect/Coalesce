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
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Edit(SecurityPermissionLevels.AllowAll)]
        public string {|COA002:TestProperty|} { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task EditAttribute_OnProperty_WithoutPermissionLevel_DoesNotReportDiagnostic()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

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

            await VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task EditAttribute_OnClass_WithPermissionLevel_DoesNotReportDiagnostic()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    [Edit(SecurityPermissionLevels.AllowAuthenticated)]
    public class TestClass
    {
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task EditAttribute_OnProperty_CodeFix_RemovesPermissionLevel()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Edit(SecurityPermissionLevels.AllowAll)]
        public string {|COA002:TestProperty|} { get; set; }
    }
}";

            var fixedCode = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Edit]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, fixedCode);
        }

        [Fact]
        public async Task EditAttribute_OnProperty_CodeFix_RemovesPermissionLevel_KeepsRoles()
        {
            var test = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Edit(SecurityPermissionLevels.AllowAll, ""Admin"")]
        public string {|COA002:TestProperty|} { get; set; }
    }
}";

            var fixedCode = @"
using IntelliTect.Coalesce.DataAnnotations;

namespace TestNamespace
{
    public class TestClass
    {
        [Edit(""Admin"")]
        public string TestProperty { get; set; }
    }
}";

            await VerifyCodeFixAsync(test, fixedCode);
        }

        private static async Task VerifyCodeFixAsync(string source, string fixedSource)
        {
            var test = new CSharpCodeFixTest<EditAttributePermissionLevelAnalyzer, EditAttributePermissionLevelCodeFixProvider, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };

            // Add reference to IntelliTect.Coalesce
            test.TestState.AdditionalReferences.Add(typeof(IntelliTect.Coalesce.DataAnnotations.EditAttribute).Assembly);

            await test.RunAsync();
        }
    }
}