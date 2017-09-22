using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;

namespace IntelliTect.Coalesce.CodeGeneration.Common
{
    public class ProjectContext
    {

        public List<ResolvedReference> CompilationAssemblies { get; set; }
        public string ProjectFullPath { get; set; }
        public string ProjectFilePath { get; set; }

        public ICollection<MetadataReference> GetMetadataReferences()
        {
            var references = new List<MetadataReference>();

            // Todo: When the target app references scaffolders as nuget packages rather than project references,
            // we need to ensure all dependencies for compiling the generated razor template.
            // This requires further thinking for custom scaffolders because they may be using
            // some other references which are not available in any of these closures.
            // As the above comment, the right thing to do here is to use the dependency closure of
            // the assembly which has the template.
            var exports = CompilationAssemblies;

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

        private ModelTypesLocator _typeLocator;
        public ModelTypesLocator TypeLocator => _typeLocator = (_typeLocator ?? ModelTypesLocator.FromProjectContext(this));

    }
}
