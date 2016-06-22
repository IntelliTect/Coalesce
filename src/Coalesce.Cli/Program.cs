
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;

namespace Coalesce.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            var ws = MSBuildWorkspace.Create();
            var task = ws.OpenProjectAsync(@"..\coalesce.domain");
            task.Wait();
            var proj = task.Result;



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
