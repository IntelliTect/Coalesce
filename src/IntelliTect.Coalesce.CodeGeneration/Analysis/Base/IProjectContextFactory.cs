using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Base;

public interface IProjectContextFactory
{
    ProjectContext CreateContext(ProjectConfiguration projectConfig, bool restore = false);
}
