using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;

public interface IResolvedTemplate
{
    /// <summary>
    /// The template descriptor that was resolved.
    /// </summary>
    TemplateDescriptor TemplateDescriptor { get; }

    /// <summary>
    /// Whether or not the template was resolved from the filesystem.
    /// If true, FullName will contain the path on the filesystem where the template was found.
    /// </summary>
    bool ResolvedFromDisk { get; }

    /// <summary>
    /// The full name of the template, including path and file name.
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// Gets a stream of the contents of the template.
    /// </summary>
    /// <returns>
    /// A stream of the contents of the template.
    /// </returns>
    Stream GetContents();
}
