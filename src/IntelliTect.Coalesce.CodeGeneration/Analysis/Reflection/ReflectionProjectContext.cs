using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using Microsoft.DotNet.Cli.Utils;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.Versioning;
using PC = IntelliTect.Coalesce.CodeGeneration.Configuration.ProjectConfiguration;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection
{
    public class ReflectionProjectContext : ProjectContext
    {
        public FileInfo AssemblyFileInfo { get; internal set; }

        public ReflectionProjectContext(ProjectConfiguration projectConfig) : base(projectConfig)
        {
            throw new NotImplementedException("ReflectionProjectContext is in a somewhat unfinished state." +
                "If reflection-based generation is needed, it will probably need some work before it is robust enough for use..");
        }

        public override TypeLocator TypeLocator => new ReflectionTypeLocator(this);

        public override ICollection<MetadataReference> GetTemplateMetadataReferences()
        {
            // Force load required assemblies into the current appdomain.
            // This was taken from commit 09c9be3, RazorTemplating.cs
            // Load Microsoft.CSharp.RuntimeBinder
            new Microsoft.CSharp.RuntimeBinder.RuntimeBinderException();
            // Load Micosoft.AspNetCore.Html
            new Microsoft.AspNetCore.Html.HtmlString("");

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Select(asm => asm.GetName());

            var outputAssemblies = AssemblyFileInfo.Directory
                .EnumerateFiles()
                .Where(f => new[] { "exe", "dll" }.Contains(f.Extension))
                .Select(f => AssemblyName.GetAssemblyName(f.FullName));

            return loadedAssemblies
                .Concat(outputAssemblies)
                // Get only one reference for each assembly. There may be duplicates because we pulled from
                // both our current AppDomain and the target assembly's output dir.
                .GroupBy(name => name.FullName)
                .Select(group => group.First())
                .Select(name => new Uri(name.CodeBase).LocalPath)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();
        }
    }
}
