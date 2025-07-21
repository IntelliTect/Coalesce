using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.CSharp;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn;

public class RoslynProjectContextFactory : IProjectContextFactory
{
    public RoslynProjectContextFactory(ILogger<RoslynProjectContextFactory> logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }

    public ProjectContext CreateContext(ProjectConfiguration projectConfig, bool restore = false)
    {
        var context = new RoslynProjectContext(projectConfig);

        var builder = new MsBuildProjectContextBuilder(Logger, context);
        if (restore)
        {
            builder = builder.RestoreProjectPackages();
        }

        var msbContext = context.MsBuildProjectContext = builder.BuildProjectContext();

        if (!LanguageVersionFacts.TryParse(msbContext.LangVersion, out var langVersion))
        {
            Logger.LogWarning($"Unknown or unsupported C# Language version '{msbContext.LangVersion}' specified by {msbContext.ProjectName}. Code generation may malfunction.");
        }
        else
        {
            context.LangVersion = langVersion;
        }

        return context;
    }
}
