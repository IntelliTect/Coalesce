namespace IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;

public interface ITemplateResolver
{
    IResolvedTemplate Resolve(TemplateDescriptor descriptor);
}
