using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.CodeGeneration.Configuration;

public class CoalesceConfiguration
{
    public ProjectConfiguration DataProject { get; set; }

    public ProjectConfiguration WebProject { get; set; }

    public string RootGenerator { get; set; }

    /// <summary>
    /// If not empty, specifies the names of specific root types
    /// (i.e. those annotated with [Coalesce])
    /// that should be used for type discovery.
    /// If empty or null, all such types will be used.
    /// </summary>
    public List<string> RootTypesWhitelist { get; set; }

    public OutputConfiguration Output { get; set; } = new OutputConfiguration();

    public Dictionary<string, JObject> GeneratorConfig { get; set; } = new Dictionary<string, JObject>();

    public bool DryRun { get; set; }
}

public class OutputConfiguration
{
    // Specify an area name to output MVC components to.
    // Property is undocumented, implementation is probably incomplete.
    public string AreaName { get; set; } = null;

    public string TypescriptModulePrefix { get; set; } = null;

    /// <summary>
    /// Path relative to the .csproj of the web project where all output should be written.
    /// </summary>
    /// <remarks>TODO: needs documentation</remarks>
    public string TargetDirectory { get; set; } = null;
}

public class ProjectConfiguration
{
    /// <summary>
    /// Path to .csproj file for the project.
    /// </summary>
    public string ProjectFile { get; set; }

    /// <summary>
    /// Override the root namespace of the target project.
    /// </summary>
    public string RootNamespace { get; set; }

    /// <summary>
    /// Build configuration (Debug/Release) to use when building or analyzing projects.
    /// </summary>
    public string Configuration { get; set; } = "Debug";

    /// <summary>
    /// Target Framework to use when building or analyzing projects.
    /// </summary>
    public string Framework { get; set; }


    // Reflection project context builder (which is obsolete) only.
    public bool Build { get; set; }

    // Reflection project context builder (which is obsolete) only.
    public string Assembly { get; set; }

    internal bool BuildProjectReferences { get; set; } = true;
}
