using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.Validation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.CodeGeneration.Tests
{
    public class CodeGenTestBase
    {
        protected GenerationExecutor BuildExecutor()
        {
            return new GenerationExecutor(
                new Configuration.CoalesceConfiguration
                {
                    WebProject = new Configuration.ProjectConfiguration
                    {
                        RootNamespace = "MyProject"
                    }
                },
                Microsoft.Extensions.Logging.LogLevel.Information
            );
        }

        protected async Task AssertSuiteOutputCompiles(IRootGenerator suite)
        {
            var project = new DirectoryInfo(Directory.GetCurrentDirectory())
                .FindFileInAncestorDirectory("IntelliTect.Coalesce.CodeGeneration.Tests.csproj")
                .Directory;

            var suiteName = suite.GetType().Name;

            var tfmAttr = Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>();

            suite = suite
                .WithOutputPath(Path.Combine(project.FullName, "out", tfmAttr.FrameworkName, suiteName));

            var validationResult = ValidateContext.Validate(suite.Model);
            Assert.Empty(validationResult.Where(r => r.IsError));

            // While not needed for any test assertions,
            // we actually generate the output to disk so that we can look at it
            // in case we need to diagnose a failure or something.
            await suite.GenerateAsync();

            var generators = suite
                .GetGeneratorsFlattened()
                .OfType<IFileGenerator>()
                .Where(g => g.EffectiveOutputPath.EndsWith(".cs"))
                .ToList();

            var tasks = generators.Select(gen => (Generator: gen, Output: gen.GetOutputAsync()));
            await Task.WhenAll(tasks.Select(t => t.Output ));

            var dtoFiles = tasks
                .Select((task) => CSharpSyntaxTree.ParseText(
                    SourceText.From(new StreamReader(task.Output.Result).ReadToEnd()), 
                    path: task.Generator.EffectiveOutputPath
                ))
                .ToArray();

            var comp = ReflectionRepositoryFactory.GetCompilation(dtoFiles);
            AssertSuccess(comp);
        }

        protected void AssertSuccess(CSharpCompilation comp)
        {
            var errors = comp
                .GetDiagnostics()
                .Where(d => d.Severity >= Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

            if (!errors.Any()) return;

            Assert.False(true, string.Join("\n", errors.Select(error =>
                error.ToString() +
                $" near `" +
                error.Location.SourceTree.ToString().Substring(error.Location.SourceSpan.Start, error.Location.SourceSpan.Length) +
                "`"
            )));
        }
    }
}
