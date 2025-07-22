using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public abstract class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected static DiagnosticResult Error(string diagnosticId)
        => Microsoft.CodeAnalysis.Testing.DiagnosticResult.CompilerError(diagnosticId);

    protected static DiagnosticResult Warning(string diagnosticId)
        => Microsoft.CodeAnalysis.Testing.DiagnosticResult.CompilerWarning(diagnosticId);

    protected static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
        };

#if NET8_0
        test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#elif NET9_0
            test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
#else
#error "Add reference assemblies for new target framework.
#endif
        test.CompilerDiagnostics = CompilerDiagnostics.None;

        // Add reference to the real Coalesce assembly
        test.TestState.AdditionalReferences.Add(typeof(IntelliTect.Coalesce.DataAnnotations.ReadAttribute).Assembly);

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }
}
