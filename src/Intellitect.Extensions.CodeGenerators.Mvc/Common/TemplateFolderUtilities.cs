using Intellitect.Extensions.CodeGenerators.Mvc.Scripts;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.Extensions.CodeGenerators.Mvc.Common
{
    public static class TemplateFoldersUtilities
    {
        public static List<string> GetTemplateFolders(
            string codeGenAssembly,
            ProjectContext cliProject,
            ProjectContext mvcProject,
            string[] baseFolders
        )
        {
            // rootFolders = projects to search for templates
            var rootFolders = new List<string>();

            // any root folder that contains templates
            var templateFolders = new List<string>();

            rootFolders.Add(mvcProject.ProjectDirectory);

            var libraryManager = DependencyProvider.LibraryManager(cliProject);
            var dependency = libraryManager.GetLibrary(codeGenAssembly);

            if (dependency != null)
            {
                string containingProjectPath = dependency.Path.EndsWith("project.json") ?
                    Path.GetDirectoryName(dependency.Path) : dependency.Path;

                if (Directory.Exists(containingProjectPath))
                {
                    rootFolders.Add(containingProjectPath);
                }
            }

            foreach (var rootFolder in rootFolders)
            {
                foreach (var baseFolderName in baseFolders)
                {
                    string templatesFolderName = "Templates";
                    var candidateTemplateFolders = Path.Combine(rootFolder, templatesFolderName, baseFolderName);
                    if (Directory.Exists(candidateTemplateFolders))
                    {
                        templateFolders.Add(candidateTemplateFolders);
                    }
                }
            }

            return templateFolders;
        }

        internal static IEnumerable<string> GetTemplateFolders(string containingProject, string applicationBasePath, string[] baseFolders, object libraryManager)
        {
            throw new NotImplementedException();
        }
    }
}
