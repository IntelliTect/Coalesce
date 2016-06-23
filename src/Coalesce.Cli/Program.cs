using Intellitect.Extensions.CodeGenerators.Mvc.Scripts;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Workspaces;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;
using NuGet.Frameworks;
using System;
using System.IO;

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

            app.OnExecute(async () =>
            {
                var model = new CommandLineGeneratorModel
                {
                    DataContextClass = dataContext.Value()
                };

#if NET451
                NuGetFramework framework = FrameworkConstants.CommonFrameworks.Net451;
#else
                NuGetFramework framework = FrameworkConstants.CommonFrameworks.NetStandard16;
#endif

                

                var projectContext = new ProjectContextBuilder()
                                        .WithProjectDirectory(@"..\Coalesce.Web")
                                        .WithTargetFramework(framework)
                                        .Build();

                

#if RELEASE
                var applicationInfo = new ApplicationInfo("Coalesce.Web", @"..\Coalesce.Web", "Release");
#else
                var applicationInfo = new ApplicationInfo("Coalesce.Web", @"..\Coalesce.Web", "Debug");
#endif
                var exporter = new LibraryExporter(projectContext, applicationInfo);
                var workspace = new ProjectJsonWorkspace(projectContext);

                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton<IModelTypesLocator, ModelTypesLocator>();
                serviceCollection.AddSingleton<ILibraryExporter>(exporter);
                serviceCollection.AddSingleton(projectContext);
                serviceCollection.AddSingleton<Microsoft.CodeAnalysis.Workspace>(workspace);
                serviceCollection.AddSingleton<ILibraryManager, LibraryManager>();
                serviceCollection.AddSingleton<ICompilationService, RoslynCompilationService>();
                serviceCollection.AddSingleton<IApplicationInfo>(applicationInfo);
                serviceCollection.AddSingleton<ICodeGenAssemblyLoadContext, DefaultAssemblyLoadContext>();
                serviceCollection.AddSingleton<IFilesLocator, FilesLocator>();
                serviceCollection.AddSingleton<ITemplating, RazorTemplating>();
                serviceCollection.AddSingleton<ILogger, ConsoleLogger>();
                serviceCollection.AddSingleton<ICodeGeneratorActionsService, CodeGeneratorActionsService>();

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

                CommandLineGenerator generator = new CommandLineGenerator(serviceProvider);

                await generator.GenerateCode(model);

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
