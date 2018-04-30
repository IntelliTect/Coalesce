using IntelliTect.Coalesce.CodeGeneration.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Resolution
{
    public class TemplateResolver : ITemplateResolver
    {
        public TemplateResolver(CoalesceConfiguration config)
        {
            Config = config;
        }

        public CoalesceConfiguration Config { get; }

        public IResolvedTemplate Resolve(TemplateDescriptor descriptor)
        {
            return new ResolvedManifestResourceTemplate(descriptor);

            // TODO: check the coalesce configuration for template overrides,
            // and return a ResolvedFileSystemTemplate if found.
        }
    }
}
