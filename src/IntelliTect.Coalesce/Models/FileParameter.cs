using IntelliTect.Coalesce.Api.Controllers;
using System.IO;

namespace IntelliTect.Coalesce.Models;

public class FileParameter : IFile
{
    public required byte[] Content { get; set; }

    public string? ContentType { get; set; }

    public string? Name { get; set; }

    public long Length => Content.Length;

    bool IFile.ForceDownload => false;
    Stream? IFile.Content => new MemoryStream(Content);

    public static implicit operator File(FileParameter param) => new File
    {
        Content = ((IFile)param).Content,
        ContentType = param.ContentType,
        Length = param.Length,
        Name = param.Name,
    };

    public static implicit operator FileParameter(File param) => new FileParameter
    {
        Content = param.Content?.ReadAllBytes() ?? [],
        ContentType = param.ContentType,
        Name = param.Name,
    };
}
