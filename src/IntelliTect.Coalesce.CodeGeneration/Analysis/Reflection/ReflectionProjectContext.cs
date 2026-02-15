using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using System;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection;

public class ReflectionProjectContext : ProjectContext
{
    public FileInfo AssemblyFileInfo { get; internal set; }

    public ReflectionProjectContext(ProjectConfiguration projectConfig) : base(projectConfig)
    {
        throw new NotImplementedException("ReflectionProjectContext is in a somewhat unfinished state." +
            "If reflection-based generation is needed, it will probably need some work before it is robust enough for use..");
    }

    public override TypeLocator TypeLocator => new ReflectionTypeLocator(this);
}
