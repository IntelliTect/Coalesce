using System.Diagnostics.CodeAnalysis;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.Analyzer.Tests;

public abstract class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
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

        test.TestState.Sources.Add(("/GlobalUsings.cs",
            // Standard global usings from the template
            """
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
            """));

        // Add reference to the real Coalesce assembly
        test.TestState.AdditionalReferences.Add(typeof(ReflectionRepository).Assembly);
        test.TestState.AdditionalReferences.Add(typeof(DbContext).Assembly);

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }
}
