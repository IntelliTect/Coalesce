using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using Microsoft.DotNet.Cli.Utils;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.Versioning;
using PC = IntelliTect.Coalesce.CodeGeneration.Configuration.ProjectConfiguration;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection
{
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
}
