using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Configuration
{
    public class CoalesceConfiguration
    {
        public ProjectConfiguration DataProject { get; set; }

        public ProjectConfiguration WebProject { get; set; }

        public OutputConfiguration Output { get; set; } = new OutputConfiguration();

        public Dictionary<string, JObject> GeneratorConfig { get; set; } = new Dictionary<string, JObject>();
    }

    public class OutputConfiguration
    {
        // Specify an area name to output MVC components to.
        // Property is undocumented, implementation is probably incomplete.
        public string AreaName { get; set; } = null;

        public string TypescriptModulePrefix { get; set; } = null;

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
    }
}
