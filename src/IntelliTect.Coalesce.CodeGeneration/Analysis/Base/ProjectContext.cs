using System.IO;
using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Base;

public abstract class ProjectContext
{
    public ProjectContext(ProjectConfiguration projectConfig)
    {
        ProjectConfiguration = projectConfig;
        ProjectFilePath = Path.GetFullPath(projectConfig.ProjectFile);
        Configuration = projectConfig.Configuration;
        Framework = projectConfig.Framework;
    }

    public ProjectConfiguration ProjectConfiguration { get; }

    /// <summary>
    /// Full path of the project file.
    /// </summary>
    public string ProjectFilePath { get; }

    /// <summary>
    /// Full path to the project directory.
    /// </summary>
    public string ProjectPath => Path.GetDirectoryName(ProjectFilePath);

    /// <summary>
    /// Name of the msbuild project file.
    /// </summary>
    public string ProjectFileName => Path.GetFileName(ProjectPath);

    public abstract TypeLocator TypeLocator { get; }

    /// <summary>
    /// Build configuration (Debug/Release) to use when building or analyzing projects.
    /// </summary>
    public string Configuration { get; }

    /// <summary>
    /// Framework to build against when building or analyzing projects.
    /// </summary>
    public string Framework { get; }

    public virtual string RootNamespace { get; }

}
