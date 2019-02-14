using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Allows specifying the type of date to contain. 
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class FileAttribute : System.Attribute
    {
        /// <summary>
        /// If there is no filename, this is the MIME type.
        /// </summary>
        public string MimeType { get; set; }
        /// <summary>
        /// Name of the property that will hold the filename.
        /// </summary>
        public string FilenameProperty { get; set; }
        /// <summary>
        /// Property to store the file hash into
        /// </summary>
        public string HashProperty { get; set; }
        /// <summary>
        /// Property to store the file size into
        /// </summary>
        public string SizeProperty { get; set; }

        public FileAttribute(string mimeType = "application/octet-stream", 
                             string filenameProperty = null, 
                             string hashProperty = null,
                             string sizeProperty = null)
        {
            this.MimeType = mimeType;
            this.FilenameProperty = filenameProperty;
            this.HashProperty = hashProperty;
            this.SizeProperty = sizeProperty;
        }
    }
}
