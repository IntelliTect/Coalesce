using Microsoft.Extensions.PlatformAbstractions;
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
            string containingProject,
            string applicationBasePath,
            string[] baseFolders,
            ILibraryManager libraryManager)
        {
            var rootFolders = new List<string>();
            var templateFolders = new List<string>();

            rootFolders.Add(applicationBasePath);

            var dependency = libraryManager.GetLibrary(containingProject);

            if (dependency != null)
            {
                string containingProjectPath = "";

                if (string.Equals("Project", dependency.Type, StringComparison.Ordinal))
                {
                    containingProjectPath = Path.GetDirectoryName(dependency.Path);
                }
                else if (string.Equals("Package", dependency.Type, StringComparison.Ordinal))
                {
                    containingProjectPath = dependency.Path;
                }
                else
                {
                    Debug.Assert(false, "Unexpected type of library information for template folders");
                }

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
