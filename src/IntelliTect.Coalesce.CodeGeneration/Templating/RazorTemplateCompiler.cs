using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using IntelliTect.Coalesce.CodeGeneration.Templating.Internal;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.Templating;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
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
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Templating
{
    public class RazorTemplateCompiler
    {
        private ConcurrentDictionary<string, Type> _templateTypeCache = new ConcurrentDictionary<string, Type>();
        private ConcurrentDictionary<string, SemaphoreSlim> _templateCacheLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        private readonly ProjectContext _projectContext;
        private readonly ILogger<RazorTemplateCompiler> _logger;

        public RazorTemplateCompiler(GenerationContext genContext, ILogger<RazorTemplateCompiler> logger)
        {
            _projectContext = genContext.WebProject;
            _logger = logger;
        }

        public async Task<CoalesceTemplate> GetTemplateInstance(IResolvedTemplate template)
        {
            var type = await GetCachedTemplateType(template);
            var compiledObject = Activator.CreateInstance(type);

            if (!(compiledObject is CoalesceTemplate razorTemplate))
            {
                throw new InvalidCastException($"Couldn't cast the result of template {template} to class {typeof(CoalesceTemplate).FullName}.");
            }
            return razorTemplate;
        }

        public async Task<Type> GetCachedTemplateType(IResolvedTemplate template)
        {
            // https://stackoverflow.com/a/34834337/2465631

            var key = template.FullName;
            var keyLock = _templateCacheLocks.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();
            try
            {
                return _templateTypeCache.GetOrAdd(key, path =>
                {
                    return GetTemplateType(template);
                });
            }
            finally
            {
                keyLock.Release();
            }
        }

        private Type GetTemplateType(IResolvedTemplate template)
        {
            RazorTemplateEngine engine = new CoalesceRazorTemplateEngine(
                RazorEngine.Create(options => {
                    //RazorExtensions.Register(options);
                    options
                        .AddDirective(InheritsDirective.Directive)
                        .SetBaseType(nameof(CoalesceTemplate));
                }),
                RazorProject.Create(template.ResolvedFromDisk
                    ? template.FullName
                    : Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()))
            );

            RazorCSharpDocument generatorResults;
            if (template.ResolvedFromDisk)
            {
                generatorResults = engine.GenerateCode(template.FullName);
            }
            else
            {
                // Need to provide the default imports here.
                // They aren't used in generation if you load directly from a stream.
                // They are applied automatically by Razor when loading a file from a filesystem,
                // but aren't when we construct a document ourselves.
                // One potential resolution for this would be to create our own implementation of RazorEngine -
                // the implementation that gets resolved above from RazorProject.Create is a FileSystemRazorProject.
                // We could theoretically create our own ManifestResourceRazorProject, or potentially a RazorProject
                // that will resolve any needed files from either location, prioritizing filesystem if the base template is read from filesystem.
                // This would probably be WAY overkill for our needs, though.
                var document = RazorCodeDocument.Create(
                    RazorSourceDocument.ReadFrom(
                        template.GetContents(),
                        template.TemplateDescriptor.TemplateFileName),
                    new[] { engine.Options.DefaultImports }
                );
                generatorResults = engine.GenerateCode(document);
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace($"Generated C# from template {template}:\r\n{generatorResults.GeneratedCode}");
            }
            else
            {
                _logger.LogDebug($"Generated C# from template {template}");
            }

            if (generatorResults.Diagnostics.Any(d => d.Severity == RazorDiagnosticSeverity.Error))
            {
                _logger.LogError($"Error compiling razor to C# for template: {template}");
                throw new TemplateProcessingException(generatorResults.Diagnostics.Select(e => e.ToString()), generatorResults.GeneratedCode);
            }

            var type = Compile(generatorResults.GeneratedCode);
            
            _logger.LogDebug($"Compiled C# for {template} into in-memory assembly.");

            return type;
        }

        private Type Compile(string content)
        {
            var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(content) };

            var references = _projectContext.GetTemplateMetadataReferences()
                .Concat(AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(asm => !asm.IsDynamic && !string.IsNullOrEmpty(asm.Location))
                    .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                );
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
    }
}
