using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild;
using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn
{
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

            context.MsBuildProjectContext = builder.BuildProjectContext();

            return context;
        }
    }
}
