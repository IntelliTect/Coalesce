using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;

public class ResolvedManifestResourceTemplate : IResolvedTemplate
{
    public ResolvedManifestResourceTemplate(TemplateDescriptor descriptor)
    {
        TemplateDescriptor = descriptor;
    }

    public TemplateDescriptor TemplateDescriptor { get; }

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

    public override string ToString() => TemplateDescriptor.ManifestResourceFullName;
}
