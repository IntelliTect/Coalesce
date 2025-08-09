using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    /// <summary>
    /// Example service to demonstrate FileType attribute usage
    /// </summary>
    [Coalesce, Service]
    public class FileService
    {
        /// <summary>
        /// Method with FileType attribute - should not trigger analyzer warning
        /// </summary>
        public async Task<string> UploadImageWithAttribute([FileType(".jpg", ".png", ".gif")] IFormFile imageFile)
        {
            return "File uploaded successfully";
        }

        /// <summary>
        /// Method without FileType attribute - should trigger analyzer warning
        /// </summary>
        public async Task<string> UploadImageWithoutAttribute(IFormFile imageFile)
        {
            return "File uploaded successfully";
        }

        /// <summary>
        /// Non-file parameter - should not trigger analyzer warning
        /// </summary>
        public async Task<string> ProcessText(string text)
        {
            return "Text processed successfully";
        }
    }

    /// <summary>
    /// Interface exposed service - should not trigger analyzer warnings
    /// </summary>
    [Coalesce, Service]
    public interface IDocumentService
    {
        Task<string> ProcessDocument(IFormFile documentFile);
    }

    public class DocumentService : IDocumentService
    {
        /// <summary>
        /// This method is exposed by interface - should NOT trigger analyzer warning
        /// </summary>
        public async Task<string> ProcessDocument(IFormFile documentFile)
        {
            return "Document processed successfully";
        }
    }
}