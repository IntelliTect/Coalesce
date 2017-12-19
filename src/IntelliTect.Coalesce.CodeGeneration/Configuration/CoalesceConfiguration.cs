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

        public InputConfiguration Input { get; set; } = new InputConfiguration();
        public OutputConfiguration Output { get; set; } = new OutputConfiguration();
    }

    public class InputConfiguration
    {

    }

    public class OutputConfiguration
    {
        public string AreaName { get; set; } = null;

        public string TypescriptModulePrefix { get; set; } = null;
    }

    public class ProjectConfiguration
    {
        public bool Build { get; set; }

        public string ProjectFile { get; set; }

        public string Assembly { get; set; }

        public string RootNamespace { get; set; }

        /// <summary>
        /// Build configuration (Debug/Release) to use when building or analyzing projects.
        /// </summary>
        public string Configuration { get; set; } = "Debug";

        /// <summary>
        /// Target Framework to use when building or analyzing projects.
        /// </summary>
        public string Framework { get; set; }
    }
}
