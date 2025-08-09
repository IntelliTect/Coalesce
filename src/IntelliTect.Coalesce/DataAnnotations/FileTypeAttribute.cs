using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// When placed on a method parameter, provides suggestions for acceptable file types for file uploads.
    /// This attribute provides suggestions to help with validation and UI hints, not strict restrictions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FileTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the FileTypeAttribute with the specified file extensions.
        /// </summary>
        /// <param name="extensions">The file extensions that are suggested for this parameter, e.g., ".jpg", ".png", ".pdf"</param>
        public FileTypeAttribute(params string[] extensions)
        {
            Extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
        }

        /// <summary>
        /// Gets the suggested file extensions for this parameter.
        /// </summary>
        public string[] Extensions { get; }
    }
}