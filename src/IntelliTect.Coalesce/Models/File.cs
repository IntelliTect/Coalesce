using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    /// <summary>
    /// Representation of a file for use in Coalesce method parameters.
    /// </summary>
    public class File : IFile
    {
        public File() { }

        public File(Stream content)
        {
            Content = content;
            Length = content.Length;
        }

        public File(byte[] content) : this(new MemoryStream(content ?? Array.Empty<byte>())) { }

        public string? Name { get; set; }

        public long Length { get; set; }

        public string? ContentType { get; set; }

        public Stream? Content { get; set; }
    }
}
