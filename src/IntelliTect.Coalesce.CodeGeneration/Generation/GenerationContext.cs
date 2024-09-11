using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class GenerationContext
    {
        public GenerationContext(CoalesceConfiguration config)
        {
            CoalesceConfiguration = config;
        }

        public CoalesceConfiguration CoalesceConfiguration { get; }

        public ProjectContext WebProject { get; internal set; }

        public ProjectContext DataProject { get; internal set; }

        public string OutputNamespaceRoot => CoalesceConfiguration.WebProject?.RootNamespace ?? WebProject.RootNamespace;

        public string AreaName => CoalesceConfiguration.Output.AreaName;

        public string TypescriptModulePrefix => CoalesceConfiguration.Output.TypescriptModulePrefix;

        private int actionCount = 0;
        public void ActionPerformed()
        {
            Interlocked.Increment(ref actionCount);
        }
        public int ActionsPerformedCount => actionCount;
    }
}
