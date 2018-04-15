using IntelliTect.Coalesce.CodeGeneration.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Base
{
    public interface IProjectContextFactory
    {
        ProjectContext CreateContext(ProjectConfiguration projectConfig, bool restore = false);
    }
}
