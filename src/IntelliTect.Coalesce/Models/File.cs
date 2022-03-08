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

#if NET5_0_OR_GREATER
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
#endif

        public string? Name { get; set; }

        public long Length { get; set; }

        public string? ContentType { get; set; }

        public Stream? Content { get; set; }
    }

}
