using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Resolution
{
    public class ResolvedFileSystemTemplate : IResolvedTemplate
    {
        public ResolvedFileSystemTemplate(TemplateDescriptor descriptor, string resolvedPath)
        {
            TemplateDescriptor = descriptor;
        }

        public TemplateDescriptor TemplateDescriptor { get; }

        public bool ResolvedFromDisk => true;

        public string FullName { get; }

        public Stream GetContents()
        {
            return File.OpenRead(FullName);
        }

        public override string ToString() => FullName;
    }
}
