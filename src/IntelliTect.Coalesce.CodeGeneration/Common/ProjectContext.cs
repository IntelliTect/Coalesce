using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ProjectModel.Resolution;

namespace IntelliTect.Coalesce.CodeGeneration.Common
{
    public class ProjectContext
    {
        public List<ResolvedReference> CompilationAssemblies { get; set; }
        public string ProjectFullPath { get; set; }
        public string ProjectFilePath { get; set; }
    }
}
