using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ProjectModel.Resolution;
using System.IO;
using System.Reflection;

namespace IntelliTect.Coalesce.CodeGeneration.Common
{
    public class ProjectContext
    {
        public ProjectContext() { }
        public ProjectContext(string folder, string assemblyName)
        {
            ProjectFullPath = folder;
            ProjectFilePath = "";

            var assemblyFile = new DirectoryInfo(ProjectFullPath).GetFiles(assemblyName + ".dll", SearchOption.AllDirectories).FirstOrDefault();
            if (assemblyFile == null)
            {
                assemblyFile = new DirectoryInfo(ProjectFullPath).GetFiles(assemblyName + ".exe", SearchOption.AllDirectories).FirstOrDefault();
            }
            if (assemblyFile == null)
            {
                //throw new ArgumentException($"Could not find a assembly named {assemblyName + ".*"} in {ProjectFullPath}");
                Console.WriteLine ($"Could not find a assembly named {assemblyName + ".*"} in {ProjectFullPath}");
            }
            else
            {
                AssemblyFilePath = assemblyFile.FullName;

                // Load the assembly
                AssemblyName an = AssemblyName.GetAssemblyName(AssemblyFilePath);
                Assembly = Assembly.Load(an);

                CompilationAssemblies = new List<ResolvedReference>();
                // Pull in all the dependencies
                //var allDlls = assemblyFile.Directory.GetFiles("*.dll", SearchOption.AllDirectories);
                foreach (var item in Assembly.GetReferencedAssemblies())
                {
                    //CompilationAssemblies.Add(new ResolvedReference(item.Name.Substring(0, item.Name.Length - item.Extension.Length), item.FullName));
                }
            }
        }


        public List<ResolvedReference> CompilationAssemblies { get; set; }
        public string ProjectFullPath { get; set; }
        public string ProjectFilePath { get; set; }
        public string AssemblyFilePath { get; set; }

        public Assembly Assembly { get; set; }
    }
}
