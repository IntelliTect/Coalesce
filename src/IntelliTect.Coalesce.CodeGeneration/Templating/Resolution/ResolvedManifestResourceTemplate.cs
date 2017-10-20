using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Resolution
{
    public class ResolvedManifestResourceTemplate : IResolvedTemplate
    {
        public ResolvedManifestResourceTemplate(TemplateDescriptor descriptor)
        {
            TemplateDescriptor = descriptor;
        }

        public TemplateDescriptor TemplateDescriptor { get; private set; }

        public bool ResolvedFromDisk => false;

        public string FullName => TemplateDescriptor.FullPath;

        public Stream GetContents()
        {
            var stream = TemplateDescriptor.ManifestResourceAssembly.GetManifestResourceStream(TemplateDescriptor.ManifestResourceFullName);
            if (stream == null)
            {
                throw new FileNotFoundException("Could not find template", TemplateDescriptor.ManifestResourceFullName);
            }
            return stream;
        }
    }
}
