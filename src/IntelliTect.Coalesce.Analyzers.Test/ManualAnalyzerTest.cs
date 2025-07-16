using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.Analyzers.Test
{
    public class ManualAnalyzerTest
    {
        [Fact]
        public async Task ReadAttributeAnalyzer_DetectsPermissionLevelOnProperty()
        {
            var code = @"
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    public enum SecurityPermissionLevels { AllowAll = 1, AllowAuthenticated = 2, DenyAll = 3 }
    public abstract class SecurityAttribute : Attribute
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthenticated;
        public string Roles { get; set; } = string.Empty;
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadAttribute : SecurityAttribute
    {
        public ReadAttribute() { }
        public ReadAttribute(SecurityPermissionLevels permissionLevel) { PermissionLevel = permissionLevel; }
        public ReadAttribute(params string[] roles) { Roles = string.Join(string.Empty, roles); }
    }
}

namespace TestNamespace
{
    using IntelliTect.Coalesce.DataAnnotations;

    public class TestClass
    {
        [Read(SecurityPermissionLevels.AllowAll)]
        public string TestProperty { get; set; }
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "TestAssembly",
                new[] { syntaxTree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var analyzer = new ReadAttributePermissionLevelAnalyzer();
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
            var analyzerDiagnostics = diagnostics.Where(d => d.Descriptor.Id == "COA001").ToArray();

            Assert.Single(analyzerDiagnostics);
            Assert.Contains("TestProperty", analyzerDiagnostics[0].GetMessage());
        }

        [Fact]
        public async Task EditAttributeAnalyzer_DetectsPermissionLevelOnProperty()
        {
            var code = @"
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    public enum SecurityPermissionLevels { AllowAll = 1, AllowAuthenticated = 2, DenyAll = 3 }
    public abstract class SecurityAttribute : Attribute
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthenticated;
        public string Roles { get; set; } = string.Empty;
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class EditAttribute : SecurityAttribute
    {
        public EditAttribute() { }
        public EditAttribute(SecurityPermissionLevels permissionLevel) { PermissionLevel = permissionLevel; }
        public EditAttribute(params string[] roles) { Roles = string.Join(string.Empty, roles); }
    }
}

namespace TestNamespace
{
    using IntelliTect.Coalesce.DataAnnotations;

    public class TestClass
    {
        [Edit(SecurityPermissionLevels.AllowAll)]
        public string TestProperty { get; set; }
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "TestAssembly",
                new[] { syntaxTree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var analyzer = new EditAttributePermissionLevelAnalyzer();
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
            var analyzerDiagnostics = diagnostics.Where(d => d.Descriptor.Id == "COA002").ToArray();

            Assert.Single(analyzerDiagnostics);
            Assert.Contains("TestProperty", analyzerDiagnostics[0].GetMessage());
        }

        [Fact]
        public async Task ReadAttributeAnalyzer_DoesNotDetectOnClass()
        {
            var code = @"
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    public enum SecurityPermissionLevels { AllowAll = 1, AllowAuthenticated = 2, DenyAll = 3 }
    public abstract class SecurityAttribute : Attribute
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthenticated;
        public string Roles { get; set; } = string.Empty;
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadAttribute : SecurityAttribute
    {
        public ReadAttribute() { }
        public ReadAttribute(SecurityPermissionLevels permissionLevel) { PermissionLevel = permissionLevel; }
        public ReadAttribute(params string[] roles) { Roles = string.Join(string.Empty, roles); }
    }
}

namespace TestNamespace
{
    using IntelliTect.Coalesce.DataAnnotations;

    [Read(SecurityPermissionLevels.AllowAll)]
    public class TestClass
    {
        public string TestProperty { get; set; }
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "TestAssembly",
                new[] { syntaxTree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var analyzer = new ReadAttributePermissionLevelAnalyzer();
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
            var analyzerDiagnostics = diagnostics.Where(d => d.Descriptor.Id == "COA001").ToArray();

            Assert.Empty(analyzerDiagnostics);
        }
    }
}