using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.Templating;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Web.CodeGeneration.Core;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Templating
{
    public class RazorTemplateProvider
    {
        private ConcurrentDictionary<string, CoalesceTemplate> _templateCache = new ConcurrentDictionary<string, CoalesceTemplate>();
        private ProjectContext _projectContext;

        public RazorTemplateProvider(ProjectContext projectContext)
        {
            _projectContext = projectContext;
        }

        public CoalesceTemplate GetCompiledTemplate(string templatePath)
        {
            return _templateCache.GetOrAdd(templatePath, path =>
            {
                var templateContent = File.ReadAllText(path);
                return GetCompiledTemplate(path, templateContent);
            });
        }

        private CoalesceTemplate GetCompiledTemplate(string path, string content)
        {
            RazorTemplateEngine engine = new CoalesceRazorTemplateEngine(
                RazorEngine.Create(options => {
                    //RazorExtensions.Register(options);
                    options
                        .AddDirective(InheritsDirective.Directive)
                        .SetBaseType(nameof(CoalesceTemplate));
                }
                ),
                RazorProject.Create(Path.GetDirectoryName(path))
            ); 

            using (var reader = new StringReader(content))
            {
                //var className = ParserHelpers.SanitizeClassName(Path.GetFileName(path));
                var generatorResults = engine.GenerateCode(path);

                if (generatorResults.Diagnostics.Any(d => d.Severity == RazorDiagnosticSeverity.Error))
                {
                    throw new TemplateProcessingException(generatorResults.Diagnostics.Select(e => e.ToString()), generatorResults.GeneratedCode);
                }

                var type = Compile(generatorResults.GeneratedCode);
                var compiledObject = Activator.CreateInstance(type);
                var razorTemplate = compiledObject as CoalesceTemplate;

                if (!(compiledObject is CoalesceTemplate))
                {
                    throw new InvalidCastException($"Couldn't cast the result of template {path} to class {typeof(CoalesceTemplate).FullName}.");
                }

                return razorTemplate;
            }
        }

        private Type Compile(string content)
        {
            var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(content) };

            var references = _projectContext.GetTemplateMetadataReferences();
            var assemblyName = Path.GetRandomFileName();

            var compilation = CSharpCompilation.Create(assemblyName,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                        syntaxTrees: syntaxTrees,
                        references: references);

            using (var assemblyStream = new MemoryStream())
            {
                using (var pdbStream = new MemoryStream())
                {
                    var result = compilation.Emit(
                        assemblyStream,
                        pdbStream);

                    if (!result.Success)
                    {
                        throw new TemplateProcessingException(result.Diagnostics.Select(d => d.ToString()), content);
                    }

                    assemblyStream.Seek(0, SeekOrigin.Begin);
                    pdbStream.Seek(0, SeekOrigin.Begin);

                    var assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
                    var type = assembly.GetExportedTypes().FirstOrDefault(a => !a.IsNested);

                    return type;
                }
            }
        }

        public async Task<Stream> RunTemplateAsync(CoalesceTemplate template, dynamic templateModel, string outputPath)
        {
            template.Model = templateModel;

            string result;
            try
            {
                result = await template.ExecuteTemplate();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"There was an error running the template {template.FileName}: {ex.Message}", ex);
            }

            if (outputPath.EndsWith(".cs"))
            {
                var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(result);
                var root = syntaxTree.GetRoot();
                root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());
                result = root.ToFullString();
            }
            
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }
    }
}
