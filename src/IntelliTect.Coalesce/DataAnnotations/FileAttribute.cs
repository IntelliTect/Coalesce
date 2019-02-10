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
        public string MimeType { get; set; }
        public string FilenameProperty { get; set; }
        public string DefaultFilename { get; set; }

        public FileAttribute(string mimeType = "text/plain", string filenameProperty = null, string defaultFilename = null)
        {
            this.MimeType = mimeType;
            this.FilenameProperty = filenameProperty;
            this.DefaultFilename = defaultFilename;
        }
    }
}
