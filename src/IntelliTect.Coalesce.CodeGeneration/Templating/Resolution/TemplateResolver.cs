using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;

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
    }
}
