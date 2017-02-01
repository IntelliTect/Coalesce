using System;
using System.Collections.Generic;
#if NET451
using System.ComponentModel;
#endif
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Frameworks;
using System.IO;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.Extensions.ProjectModel;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;
using Microsoft.CodeAnalysis.MSBuild;

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

        public static IProjectContext ProjectContext(string projectPath)
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
                if (curDirectory.EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly).Count() == 1)
                {
                    foundProjectJsonPath = curDirectory.FullName;
                    break;
                }
                curDirectory = curDirectory.Parent;
            }
            if (string.IsNullOrEmpty(foundProjectJsonPath)) throw new ArgumentException("Project path not found.");

            var configuration = "Debug";
#if RELEASE
            configuration = "Release";
#endif

            return new MsBuildProjectContextBuilder(foundProjectJsonPath, "D:\\Work\\Microsoft.VisualStudio.Web.CodeGeneration.Tools.targets", configuration)
                .Build();
        }

        public static ModelTypesLocator ModelTypesLocator(IProjectContext project)
        {
            var workspace = MSBuildWorkspace.Create();
            var result = workspace.OpenProjectAsync(project.ProjectFullPath).Result;

            //var workspace = new ProjectJsonWorkspace(project.ProjectDirectory);

            return new ModelTypesLocator(workspace);
        }

        public static CodeGeneratorActionsService CodeGeneratorActionsService(IProjectContext project)
        {
            ICodeGenAssemblyLoadContext loadContext = new DefaultAssemblyLoadContext();
            IFilesLocator files = new FilesLocator();

            return new CodeGeneratorActionsService(new RazorTemplating(
                new RoslynCompilationService(ApplicationInfo(project), loadContext, project)), files);
        }

        private static ApplicationInfo ApplicationInfo(IProjectContext project)
        {
            return new ApplicationInfo(
                project.ProjectName,
                Path.GetDirectoryName(project.ProjectFullPath));
        }
    }
}
