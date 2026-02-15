using System.Collections.Generic;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public abstract class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private static void SetupSolutionState(SolutionState solutionState)
    {
#if NET8_0
        solutionState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#elif NET9_0
        solutionState.ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
#elif NET10_0
        solutionState.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
#else
#error "Add reference assemblies for new target framework.
#endif

        var globalUsings = """
            global using System;
            global using System.Collections.Generic;
            global using System.IO;
            global using System.Linq;
            global using System.Threading;
            global using System.Threading.Tasks;
            global using IntelliTect.Coalesce;
            global using IntelliTect.Coalesce.DataAnnotations;
            global using IntelliTect.Coalesce.Models;
            global using Microsoft.EntityFrameworkCore;
            global using System.ComponentModel.DataAnnotations;
            global using System.ComponentModel.DataAnnotations.Schema;
            global using System.Security.Claims;
            """;

        solutionState.Sources.Add(("/GlobalUsings.cs", globalUsings));

        // Add reference to the real Coalesce assembly
        solutionState.AdditionalReferences.Add(typeof(ReflectionRepository).Assembly);
        solutionState.AdditionalReferences.Add(typeof(DbContext).Assembly);
    }

    protected List<string> DisabledDiagnostics { get; set; } = [];

    protected async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
        };
        test.DisabledDiagnostics.AddRange(DisabledDiagnostics);

        SetupSolutionState(test.TestState);

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    protected async Task VerifyCodeFixAsync<TCodeFixProvider>(string source, string fixedSource, params DiagnosticResult[] expected)
        where TCodeFixProvider : CodeFixProvider, new()
    {
        await VerifyCodeFixAsync<TCodeFixProvider>(source, fixedSource, 0, expected);
    }

    protected async Task VerifyCodeFixAsync<TCodeFixProvider>(string source, string fixedSource, int codeActionIndex, params DiagnosticResult[] expected)
        where TCodeFixProvider : CodeFixProvider, new()
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixProvider, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            CodeActionIndex = codeActionIndex
        };
        test.DisabledDiagnostics.AddRange(DisabledDiagnostics);

        SetupSolutionState(test.TestState);
        SetupSolutionState(test.FixedState);

        test.TestState.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    protected async Task VerifyAnalyzerAndCodeFixAsync<TCodeFixProvider>(string source, string fixedSource, params DiagnosticResult[] expected)
        where TCodeFixProvider : CodeFixProvider, new()
    {
        await VerifyAnalyzerAsync(source, expected);
        await VerifyCodeFixAsync<TCodeFixProvider>(source, fixedSource, expected);
    }
}
