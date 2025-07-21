using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Specify the allowed file types for a <see cref="Models.IFile"/> parameter.
/// The value is passed through to the "accept" attribute of an 
/// <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/input/file#accept">HTML File Input</see>.
/// This does not implement any server-side validation of file content.
/// </summary>
/// <param name="fileTypes">Comma-delimited list of file extensions and/or MIME types.</param>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FileTypeAttribute(string fileTypes) : Attribute
{
    public string FileTypes { get; set; } = fileTypes;
}
