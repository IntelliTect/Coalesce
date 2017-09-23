using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Configuration
{
    public class CoalesceConfiguration
    {
        //public Dictionary<string, CoalesceProject> Projects { get; set; }

        public ProjectConfiguration WebProject { get; set; }
        public ProjectConfiguration DataProject { get; set; }
    }

    public class ProjectConfiguration
    {
        public BuildConfiguration Build { get; set; }

        // public string BuildCommand { get; set; }

        public string ProjectFile { get; set; }

        public string Assembly { get; set; }
    }

    public class BuildConfiguration
    {
        public string Args { get; set; }

        public string Output { get; set; }
    }
}
