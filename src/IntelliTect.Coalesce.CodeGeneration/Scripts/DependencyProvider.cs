using System;
using System.Collections.Generic;
#if NET451
using System.ComponentModel;
#endif
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Frameworks;
using Microsoft.DotNet.ProjectModel;
using System.IO;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.DotNet.ProjectModel.Workspaces;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    public static class DependencyProvider
    {
        private static NuGetFramework framework;

        static DependencyProvider()
        {
#if NET46
            framework = FrameworkConstants.CommonFrameworks.Net46;
#else
            framework = FrameworkConstants.CommonFrameworks.NetStandard16;
#endif
        }

        public static ProjectContext ProjectContext(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException($"{nameof(projectPath)} is required.");

            // Check for uri paths
            if (projectPath.StartsWith("file:")) projectPath = new Uri(projectPath).LocalPath;

            // Search up the folders from the path provided and find a project.json
            var foundProjectJsonPath = "";
            var curDirectory = new DirectoryInfo(projectPath);
            var rootDirectory = curDirectory.Root.FullName;
            while (curDirectory.FullName != rootDirectory)
            {
                if (curDirectory.EnumerateFiles("project.json", SearchOption.TopDirectoryOnly).Count() == 1)
                {
                    foundProjectJsonPath = curDirectory.FullName;
                    break;
                }
                curDirectory = curDirectory.Parent;
            }
            if (string.IsNullOrEmpty(foundProjectJsonPath)) throw new ArgumentException("Project path not found.");

            return new ProjectContextBuilder()
                    .WithProjectDirectory(foundProjectJsonPath)
                    .WithTargetFramework(framework)
                    .Build();
        }

        public static ModelTypesLocator ModelTypesLocator(ProjectContext project)
        {
            var workspace = new ProjectJsonWorkspace(project.ProjectDirectory);

            return new ModelTypesLocator(LibraryExporter(project), workspace);
        }

        public static CodeGeneratorActionsService CodeGeneratorActionsService(ProjectContext project)
        {
            ICodeGenAssemblyLoadContext loadContext = new DefaultAssemblyLoadContext();
            IFilesLocator files = new FilesLocator();

            return new CodeGeneratorActionsService(new RazorTemplating(
                new RoslynCompilationService(ApplicationInfo(project), loadContext, LibraryExporter(project))), files);
        }

        public static LibraryManager LibraryManager(ProjectContext project)
        {
            return new LibraryManager(project);
        }



        private static LibraryExporter LibraryExporter(ProjectContext project)
        {
            return new LibraryExporter(project, ApplicationInfo(project));
        }

        private static ApplicationInfo ApplicationInfo(ProjectContext project)
        {
#if RELEASE
            return new ApplicationInfo(project.GetType().ToString(), project.ProjectDirectory, "Release");
#else
            return new ApplicationInfo(project.GetType().ToString(), project.ProjectDirectory, "Debug");
#endif
        }
    }
}
