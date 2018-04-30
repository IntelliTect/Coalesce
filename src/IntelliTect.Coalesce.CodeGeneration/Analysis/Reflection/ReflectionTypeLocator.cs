using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using System.IO;
using System.Reflection;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection
{
    public class ReflectionTypeLocator : TypeLocator
    {
        private ReflectionProjectContext _projectContext;

        public ReflectionTypeLocator(ReflectionProjectContext projectContext)
        {
            _projectContext = projectContext;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var AppBasePath = Path.GetDirectoryName(_projectContext.AssemblyFileInfo.FullName);
            if (!Path.IsPathRooted(AppBasePath))
            {
                AppBasePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), AppBasePath));
            }

            var assemblyName = new AssemblyName(args.Name);

            foreach (var extension in new[] { ".dll", ".exe" })
            {
                var path = Path.Combine(AppBasePath, assemblyName.Name + extension);
                if (File.Exists(path))
                {
                    try
                    {
                        return Assembly.LoadFrom(path);
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        private Assembly _assembly;

        private Assembly GetAssembly()
        {
            var asmFile = _projectContext.AssemblyFileInfo;
            if (!asmFile.Exists) return null;
            return _assembly = asmFile == null ? null : Assembly.LoadFile(asmFile.FullName);
        }

        public override TypeViewModel FindType(string typeName, bool throwWhenNotFound = true)
        {
            var assembly = GetAssembly();
            if (assembly == null)
                throw new FileNotFoundException($"Cannot locate type {typeName} - Assembly for project {_projectContext.ProjectFileName} is unavailable.");

            var candidateModelTypes = assembly
                .ExportedTypes
                .Where(t => t.Name == typeName || t.FullName == typeName)
                .ToList();

            int count = candidateModelTypes.Count;
            if (count == 0)
            {
                if (throwWhenNotFound)
                {
                    throw new ArgumentException(string.Format("A type with the name {0} does not exist", typeName));
                }
                return null;
            }

            if (count > 1)
            {
                throw new ArgumentException(string.Format(
                    "Multiple types matching the name {0} exist:{1}, please use a fully qualified name",
                    typeName,
                    string.Join(",", candidateModelTypes.Select(t => t.Name).ToArray())));
            }

            return new ReflectionTypeViewModel(candidateModelTypes.Single());
        }

        public override IEnumerable<TypeViewModel> FindDerivedTypes(string typeName, bool throwWhenNotFound = true)
        {
            throw new NotImplementedException();
        }
    }
}
