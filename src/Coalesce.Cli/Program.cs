using Intellitect.Extensions.CodeGenerators.Mvc.Scripts;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Workspaces;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Coalesce.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "Coalesce"
            };

            app.HelpOption("-h|--help");
            var dataContext = app.Option("-dc|--dataContext", "Data Context containing the classes to scaffold", CommandOptionType.SingleValue);
            var force = app.Option("-f|--force", "Use this option to overwrite existing files", CommandOptionType.SingleValue);
            var relativeFolderPath = app.Option("-outDir|--relativeFolderPath", "Specify the relative output folder path from project where the file needs to be generated, if not specified, file will be generated in the project folder", CommandOptionType.SingleValue);
            var onlyGenerateFiles = app.Option("-filesOnly|--onlyGenerateFiles", "Will only generate the file output and will not restore any of the packages", CommandOptionType.SingleValue);
            var validateOnly = app.Option("-vo|--validateOnly", "Validates the model but does not generate the models", CommandOptionType.SingleValue);
            var area = app.Option("-a|--area", "The area where the generated/scaffolded code should be placed", CommandOptionType.SingleValue);
            var module = app.Option("-m|--module", "The prefix to apply to the module name of the generated typescript files", CommandOptionType.SingleValue);
            var webProject = app.Option("-wp|--webproject", "Relative path to the web project; if empty will search up from current folder for first project.json", CommandOptionType.SingleValue);
            var dataProject = app.Option("-dp|--dataproject", "Relative path to the data project", CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                var model = new CommandLineGeneratorModel
                {
                    DataContextClass = dataContext.Value() ?? "",
                    Force = force.Value() != null && force.Value().ToLower() == "true",
                    RelativeFolderPath = relativeFolderPath.Value() ?? "",
                    OnlyGenerateFiles = onlyGenerateFiles.Value() != null && onlyGenerateFiles.Value().ToLower() == "true",
                    ValidateOnly = validateOnly.Value() != null && validateOnly.Value().ToLower() == "true",
                    AreaLocation = area.Value() ?? "",
                    TypescriptModulePrefix = module.Value() ?? ""
                };

                // Get the framework we'll load our project contexts under
#if NET461
                NuGetFramework framework = FrameworkConstants.CommonFrameworks.Net461;
#else
                NuGetFramework framework = FrameworkConstants.CommonFrameworks.NetStandard16;
#endif
                // Find the web project
                //var runtimeId = DependencyContext.Default.Target.Runtime;
                ProjectContext webContext = null;
                if (!string.IsNullOrEmpty(webProject.Value()))
                {
                    // @"..\Coalesce.Web"
                    webContext = new ProjectContextBuilder()
                                        //.WithRuntimeIdentifiers(new[] { runtimeId })
                                        .WithProjectDirectory(webProject.Value())
                                        .WithTargetFramework(framework)
                                        .Build();
                }
                else
                {
                    var curDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                    var rootDirectory = curDirectory.Root.FullName;

                    while (curDirectory.FullName != rootDirectory)
                    {
                        if (curDirectory.EnumerateFiles("project.json", SearchOption.TopDirectoryOnly).Count() == 1)
                        {
                            webContext = new ProjectContextBuilder()
                                                //.WithRuntimeIdentifiers(new[] { runtimeId })
                                                .WithProjectDirectory(curDirectory.FullName)
                                                .WithTargetFramework(framework)
                                                .Build();
                            break;
                        }

                        curDirectory = curDirectory.Parent;
                    }
                }

                if (webContext == null) throw new ArgumentException("Web project was not found.");

                // Find the data project
                ProjectContext dataProjectContext = null;
                if (!string.IsNullOrEmpty(dataProject.Value()))
                {
                    // @"..\Coalesce.Web"
                    dataProjectContext = new ProjectContextBuilder()
                                        //.WithRuntimeIdentifiers(new[] { runtimeId })
                                        .WithProjectDirectory(dataProject.Value())
                                        .WithTargetFramework(framework)
                                        .Build();
                }
                else
                {
                    var curDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                    var rootDirectory = curDirectory.Root.FullName;

                    while (curDirectory.FullName != rootDirectory)
                    {
                        if (curDirectory.EnumerateFiles("project.json", SearchOption.TopDirectoryOnly).Count() == 1)
                        {
                            dataProjectContext = new ProjectContextBuilder()
                                                //.WithRuntimeIdentifiers(new[] { runtimeId })
                                                .WithProjectDirectory(curDirectory.FullName)
                                                .WithTargetFramework(framework)
                                                .Build();
                            break;
                        }

                        curDirectory = curDirectory.Parent;
                    }
                }

                if (dataProjectContext == null) throw new ArgumentException("Data project was not found.");

                CommandLineGenerator generator = new CommandLineGenerator(webContext, dataProjectContext);

                await generator.GenerateCode(model);

                return 0;

//#if RELEASE
//                var applicationInfo = new ApplicationInfo("Coalesce.Web", @"..\Coalesce.Web", "Release");
//#else
//                var applicationInfo = new ApplicationInfo("Coalesce.Web", @"..\Coalesce.Web", "Debug");
//#endif
//                var exporter = new LibraryExporter(webContext, applicationInfo);
//                var workspace = new ProjectJsonWorkspace(webContext);

//                IServiceCollection serviceCollection = new ServiceCollection();
//                serviceCollection.AddSingleton<IModelTypesLocator, ModelTypesLocator>();
//                serviceCollection.AddSingleton<ILibraryExporter>(exporter);
//                serviceCollection.AddSingleton(webContext);
//                serviceCollection.AddSingleton<Microsoft.CodeAnalysis.Workspace>(workspace);
//                serviceCollection.AddSingleton<ILibraryManager, LibraryManager>();
//                serviceCollection.AddSingleton<ICompilationService, RoslynCompilationService>();
//                serviceCollection.AddSingleton<IApplicationInfo>(applicationInfo);
//                serviceCollection.AddSingleton<ICodeGenAssemblyLoadContext, DefaultAssemblyLoadContext>();
//                serviceCollection.AddSingleton<IFilesLocator, FilesLocator>();
//                serviceCollection.AddSingleton<ITemplating, RazorTemplating>();
//                serviceCollection.AddSingleton<ILogger, ConsoleLogger>();
//                serviceCollection.AddSingleton<ICodeGeneratorActionsService, CodeGeneratorActionsService>();

//                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

                //CommandLineGenerator generator = new CommandLineGenerator(serviceProvider);

                //await generator.GenerateCode(model);

                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //Console.WriteLine(args);
            //Console.WriteLine(Directory.GetCurrentDirectory());
            //var ws = MSBuildWorkspace.Create();
            //var task = ws.OpenProjectAsync(@"..\..\coalesce.domain");
            //task.Wait();
            //var proj = task.Result;



            //var sln = ws.OpenSolutionAsync(@"Z:\Dev\Temp\SimpleWinformsTestApp\SimpleWinformsTestApp.sln").Result;


            //Microsoft.CodeAnalysis.Solution sln = new Solution();
            //Workspace.LoadProjectFromCommandLineArguments("MyProject", "C#", commandLineForProject, BaseDirectory.ItemSpec);


            //var solution = Solution.Create(SolutionId.CreateNewId()).AddCSharpProject("Foo", "Foo").Solution;
            //var workspaceServices = (IHaveWorkspaceServices)solution;
            //var projectDependencyService = workspaceServices.WorkspaceServices.GetService<IProjectDependencyService>();
            //Roslyn.Services.Workspace.LoadSolution test = new object();
            //var assemblies = new List<Stream>();
            //foreach (var projectId in projectDependencyService.GetDependencyGraph(solution).GetTopologicallySortedProjects())
            //{
            //    using (var stream = new MemoryStream())
            //    {
            //        solution.GetProject(projectId).GetCompilation().Emit(stream);
            //        assemblies.Add(stream);
            //    }
            //}



        }
    }
}
