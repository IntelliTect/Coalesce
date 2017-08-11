using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.Templating;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Parser;
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

namespace IntelliTect.Coalesce.CodeGeneration.Generation
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
            RazorTemplatingHost host = new RazorTemplatingHost(typeof(CoalesceTemplate));
            RazorTemplateEngine engine = new RazorTemplateEngine(host);

            using (var reader = new StringReader(content))
            {
                var className = ParserHelpers.SanitizeClassName(Path.GetFileName(path));
                var generatorResults = engine.GenerateCode(reader, className, $"{nameof(Coalesce)}.Templating", path);

                if (!generatorResults.Success)
                {
                    throw new TemplateProcessingException(generatorResults.ParserErrors.Select(e => $"{e.Message} at {e.Location}"), generatorResults.GeneratedCode);
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

            var references = GetApplicationReferences();
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

        public async Task RunTemplateAsync(CoalesceTemplate template, dynamic templateModel, string outputPath)
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
                root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace.Create());
                result = root.ToFullString();
            }

            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
            {
                await WriteFileAsync(sourceStream, outputPath);
            }
        }

        private async Task WriteFileAsync(Stream contentsStream, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            if (File.Exists(outputPath))
            {
                if (!FileUtilities.HasDifferences(contentsStream, outputPath))
                {
                    return;
                }

                // Remove read only flag, if it exists.
                // Commented out because I don't know why we do this. If something is read only, its probably that way on purpose.
                // File.SetAttributes(outputPath, File.GetAttributes(outputPath) & ~FileAttributes.ReadOnly);
            }

            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                contentsStream.Seek(0, SeekOrigin.Begin);
                await contentsStream.CopyToAsync(fileStream);
            };
        }


        //Todo: This is using application references to compile the template,
        //perhaps that's not right, we should use the dependencies of the caller
        //who calls the templating API.
        private List<MetadataReference> GetApplicationReferences()
        {
            var references = new List<MetadataReference>();

            // Todo: When the target app references scaffolders as nuget packages rather than project references,
            // we need to ensure all dependencies for compiling the generated razor template.
            // This requires further thinking for custom scaffolders because they may be using
            // some other references which are not available in any of these closures.
            // As the above comment, the right thing to do here is to use the dependency closure of
            // the assembly which has the template.
            var exports = _projectContext.CompilationAssemblies;

            if (exports != null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var export in exports)
                {
                    var loadedAssembly = assemblies.FirstOrDefault(a => !a.IsDynamic && Path.GetFileName(a.Location) == export.Name);

                    // If the referenced assembly is a core Coalesce assembly, we want to use the assembly that is loaded into the current AppDomain,
                    // as opposed to the path referenced in the 'export' object. export.ResolvedPath is going to be in the output directory of the target web project,
                    // which isn't going to have the most up to date Coalesce code unless the web project is rebuild before running coalesce generation.
                    // The web project very likely can't be rebuilt when someone is regenerating coalesce (because generated code may not match up with the current data model),
                    // and besides, that's really terrible to require recompilation of the web project before being able to regenerate it.

                    if (loadedAssembly != null && loadedAssembly.FullName.StartsWith($"{nameof(IntelliTect)}.{nameof(Coalesce)}"))
                    {
                        references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
                    }
                    else
                    {
                        references.AddRange(export.GetMetadataReference(throwOnError: true));
                    }

                    // TODO: Coalesce has a bit of a chicken-and-egg problem. If essential DLLs aren't in the output folder of the web project (someone ran a Clean Solution, for eg),
                    // code generation will fail. It can also be true that the web project is in a state that can't be built without running generation first.
                    // So, something else that this code needs to do is recoginze that the path pointed to by export.ResolvedPath doesn't exist,
                    // and then attempt to find that DLL elsewhere.
                    // The vast majority of references will resolve to the .nuget package cache, which is fine. But, for those that don't,
                    // we need to make attempts to find them elsewhere.
                    // The most common case for this would be finding the Data Project dll directly in the output of the data project (or, perhaps even building the data project ourselves).
                }
            }

            return references;
        }
    }
}
