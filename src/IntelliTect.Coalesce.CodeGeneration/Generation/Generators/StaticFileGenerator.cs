using System.IO;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;

namespace IntelliTect.Coalesce.CodeGeneration.Generation;

public class StaticFileGenerator : FileGenerator
{
    protected ITemplateResolver Resolver { get; }

    public StaticFileGenerator(GeneratorServices services, ITemplateResolver resolver) : base(services)
    {
        Resolver = resolver;
    }

    public TemplateDescriptor Template { get; set; }

    public bool NoOverwrite { get; set; } = false;


    public override Task<Stream> GetOutputAsync()
    {
        return Task.FromResult(Resolver.Resolve(Template).GetContents());
    }

    public override bool ShouldGenerate() => NoOverwrite ? !File.Exists(EffectiveOutputPath) : true;

    public override string ToString()
    {
        if (EffectiveOutputPath != null)
        {
            return $"{Template.ToString()} => {EffectiveOutputPath}";
        }
        return Template.ToString();
    }

    #region Fluent

    public StaticFileGenerator WithTemplate(TemplateDescriptor template)
    {
        this.Template = template;
        return this;
    }

    public StaticFileGenerator PreventOverwrite(bool noOverwrite = true)
    {
        NoOverwrite = noOverwrite;
        return this;
    }

    #endregion
}
