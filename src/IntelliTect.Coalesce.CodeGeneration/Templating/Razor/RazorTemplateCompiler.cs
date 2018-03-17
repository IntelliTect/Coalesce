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
using System.Collections.Generic;
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
        
        private readonly ILogger<RazorTemplateCompiler> _logger;

        public RazorTemplateCompiler(ILogger<RazorTemplateCompiler> logger)
        {
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
                            _logger.LogTrace($"Won't precompile {resolved}, doesn't contain {searchString} in first {contents.Length} chars");
                        return looksLikeTemplate;
                    }
                })
                .Select( async resolved =>
                {
                    _logger.LogTrace($"Precompiling suspected template {resolved}");
                    try
                    {
                        await GetCachedTemplateType(resolved);
                        _logger.LogTrace($"Successfully precompiled {resolved}");
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
                return _templateTypeCache.GetOrAdd(key, _ => GetTemplateType(template));
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

            _logger.LogTrace($"Generated C# from template {template}");

            if (generatorResults.Diagnostics.Any(d => d.Severity == RazorDiagnosticSeverity.Error))
            {
                _logger.LogError($"Error compiling razor to C# for template: {template}");
                throw new TemplateProcessingException(generatorResults.Diagnostics.Select(e => e.ToString()), generatorResults.GeneratedCode);
            }

            var type = Compile(template, generatorResults.GeneratedCode);
            
            _logger.LogTrace($"Compiled C# for {template} into in-memory assembly.");

            return type;
        }


        
        private ICollection<MetadataReference> GetTemplateMetadataReferences(IResolvedTemplate template)
        {
            // Force load required assemblies into the current appdomain.
            // These are dependent assemblies of Coalesce that aren't otherwise loaded until this point.
            // Specific dependencies of specific generation suites and/or individual generators should be declared
            // as explicit dependencies of their generator in order to get included as a reference.
            new System.Security.Claims.ClaimsPrincipal();

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Select(asm => asm.GetName());

            var templateAssemblies = template.TemplateDescriptor.ManifestResourceAssembly
                .GetReferencedAssemblies()
                .Select(name => Assembly.Load(name).GetName());

            return loadedAssemblies
                .Concat(templateAssemblies)
                // Get only one reference for each assembly. There may be duplicates because we pulled from
                // both our current AppDomain and the templateAssemblies's deps.
                .GroupBy(name => name.FullName)
                .Select(group => group.First())
                .Select(name => new Uri(name.CodeBase).LocalPath)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();
        }

        private Type Compile(IResolvedTemplate template, string content)
        {
            var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(content) };

            var references = GetTemplateMetadataReferences(template);

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
