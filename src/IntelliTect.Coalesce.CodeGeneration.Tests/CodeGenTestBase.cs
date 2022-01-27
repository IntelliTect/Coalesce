using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.Validation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            suite = suite
                .WithOutputPath(Path.Combine(project.FullName, "out", suiteName));

            var validationResult = ValidateContext.Validate(suite.Model);
            Assert.Empty(validationResult.Where(r => r.IsError));

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

            Assert.All(errors, error =>
            {
                var loc = error.Location;

                Assert.False(true, "\"" + error.ToString() +
                    $"\" near:```\n" +
                    loc.SourceTree.ToString().Substring(loc.SourceSpan.Start, loc.SourceSpan.Length) +
                    "\n```"
                );
            });
        }
    }
}
