using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Base
{
    public abstract class ProjectContext
    {

        public string ProjectFilePath { get; set; }

        public string ProjectPath => Path.GetDirectoryName(ProjectFilePath);

        public string ProjectFileName => Path.GetFileName(ProjectPath);

        public abstract TypeLocator TypeLocator { get; }

        public abstract ICollection<MetadataReference> GetTemplateMetadataReferences();

    }
}
