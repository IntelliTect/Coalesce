using System.IO;
using System.Reflection;

namespace IntelliTect.Coalesce.CodeGeneration.Templating;

public class TemplateDescriptor
{
    public Assembly ManifestResourceAssembly { get; set; }

    public string TemplatePath { get; set; }

    public string TemplateFileName { get; set; }

    public string FullPath => Path.Combine(TemplatePath, TemplateFileName);

    public string ManifestResourceFullName =>
        ManifestResourceAssembly.GetName().Name + "." + FullPath.Replace('/', '.').Replace('\\', '.');

    public TemplateDescriptor()
    {
        ManifestResourceAssembly = Assembly.GetCallingAssembly();
    }

    public TemplateDescriptor(string templatePath, string templateFileName)
        : this(Assembly.GetCallingAssembly(), templatePath, templateFileName)
    {
    }

    public TemplateDescriptor(Assembly containingAssembly, string templatePath, string templateFileName)
    {
        ManifestResourceAssembly = containingAssembly;
        TemplatePath = templatePath;
        TemplateFileName = templateFileName;
    }

    public override string ToString() => ManifestResourceAssembly != null ? ManifestResourceFullName : FullPath;
}
