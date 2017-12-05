using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Razor
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

        /// <summary>
        /// Locate all templates embedded in an assembly, and try and precompile them.
        /// </summary>
        public Task PrecompileAssemblyTemplates(Assembly assembly)
        {
            var tasks = assembly.GetManifestResourceNames()
                .Where(name => name.EndsWith(".cshtml"))
                .AsParallel()
                .Select(name =>
                {
                    var parts = Regex.Match(name, $@"{assembly.GetName().Name}\.(.*)\.([^\.]+\.cshtml)").Groups;
                    var path = parts[1].Value;
                    var fileName = parts[2].Value;
                    return new TemplateDescriptor(assembly, path, fileName);
                })
                .Select(descriptor => new ResolvedManifestResourceTemplate(descriptor))
                .Where(resolved =>
                {
                    using (var reader = new StreamReader(resolved.GetContents()))
                    {
                        // Read the first few characters to try and see if this looks like a template.
                        // If this check doesn't work, no harm done - this whole process just an attempt at optimization.
                        var contents = new char[500];
                        const string searchString = nameof(CoalesceTemplate);
                        reader.ReadBlock(contents, 0, contents.Length);
                        var looksLikeTemplate = new String(contents).Contains(searchString);

                        if (!looksLikeTemplate)
                            _logger.LogDebug($"Won't precompile {resolved}, doesn't contain {searchString} in first {contents.Length} chars");
                        return looksLikeTemplate;
                    }
                })
                .Select( async resolved =>
                {
                    _logger.LogTrace($"Precompiling suspected template {resolved}");
                    try
                    {
                        await GetTemplateInstance(resolved);
                        _logger.LogDebug($"Successfully precompiled {resolved}");
                    }
                    catch
                    {
                        _logger.LogDebug($"Precompilation of {resolved} failed.");
                    }
                });

            return Task.WhenAll(tasks.ToArray());
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
            // this synchronization pattern is from https://stackoverflow.com/a/34834337/2465631

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
                    InheritsDirective.Register(options);
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
                // They won't automatically be used in generation if you load directly from a stream:
                //      They are applied automatically by Razor when loading a file from a filesystem,
                //      but aren't when we construct a document ourselves.
                // One potential resolution for this would be to create our own implementation of RazorEngine -
                // the underlying implementation that is ultimately used in this code is a FileSystemRazorProject.
                // We could theoretically create our own ManifestResourceRazorProject, or potentially event a RazorProject
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
                // Check log level enabled before trying to log this, since this will allocate a massive string.
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

            var references = 
                _projectContext.GetTemplateMetadataReferences()

                // Add in references to any other currently loaded Coalesce assemblies.
                // Primarily, this will be the assembly that actually hosts the generator,
                // but this could also be any other assembly that IntelliTect.Coalesce.CodeGeneration needs but IntelliTect.Coalesce doesn't.

                // This pevents Coalesce-based projects from needing to reference the code gen
                // packages/projects/assemblies when using stock generators,
                // which are always named IntelliTect.Coalesce.* and are the only generators Coalesce supports as of 10.26.2017 anyway.
                .Concat(AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(asm => !asm.IsDynamic && !string.IsNullOrEmpty(asm.Location) && asm.FullName.Contains("IntelliTect.Coalesce"))
                    .Select(asm => MetadataReference.CreateFromFile(asm.Location)));

            var assemblyName = Path.GetRandomFileName();

            var compilation = CSharpCompilation.Create(assemblyName,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                        syntaxTrees: syntaxTrees,
                        references: references);

            //var analyzers = System.Collections.Immutable.ImmutableArray.CreateBuilder<Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer>();
            //Assembly.GetAssembly(typeof(Microsoft.CodeAnalysis..))
            //        .GetTypes()
            //        .Where(x => typeof(DiagnosticAnalyzer).IsAssignableFrom(x))
            //        .Select(Activator.CreateInstance)
            //        .Cast<DiagnosticAnalyzer>()
            //        .ToList()
            //        .ForEach(x => analyzers.Add(x));


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
