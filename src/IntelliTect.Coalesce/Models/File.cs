using IntelliTect.Coalesce.Api.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    /// <summary>
    /// Representation of a file for use in Coalesce method parameters and returns.
    /// </summary>
    public class File : IFile
    {
        public File() { }

        public File(Stream content)
        {
            Content = content;
            if (content.CanSeek)
            {
                Length = content.Length;
            }
        }

        public File(byte[] content) : this(new MemoryStream(content ?? Array.Empty<byte>())) { }

        /// <summary>
        /// Construct a file that will stream its content from a database.
        /// When returned from a Coalesce method using HTTP GET, this will support HTTP Range requests.
        /// </summary>
        /// <param name="contentQuery">A query that determines the field in the database to stream.</param>
        public File(IQueryable<byte[]> contentQuery)
        {
            Content = new QueryableByteStream(contentQuery);
            // Don't bother setting Length here, since we don't actually use it when creating responses.
            // aspnetcore itself has to be the one that reads .Length from the underlying stream.
        }

        public string? Name { get; set; }

        public long Length { get; set; }

        public string? ContentType { get; set; }

        public Stream? Content { get; set; }

        /// <summary>
        /// When used in a method result, forces the file to be downloaded by a browser
        /// when the method's URL is opened directly as a link,
        /// rather than attempting to display the file in the browser tab for supported content types.
        /// This can also be forced with the `download` attribute on an `a` HTML element.
        /// </summary>
        public bool ForceDownload { get; set; }
    }

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
            Content = (param as IFile).Content,
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

}
