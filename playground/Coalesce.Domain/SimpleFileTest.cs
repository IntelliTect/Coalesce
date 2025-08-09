using Microsoft.AspNetCore.Http;
using IntelliTect.Coalesce.DataAnnotations;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    /// <summary>
    /// Simple test class to verify analyzer functionality
    /// </summary>
    public class SimpleFileTest
    {
        /// <summary>
        /// This should trigger COA001 analyzer warning
        /// </summary>
        public void MethodWithoutFileTypeAttribute(IFormFile file)
        {
            // This should trigger the analyzer
        }

        /// <summary>
        /// This should NOT trigger analyzer warning (has FileType attribute)
        /// </summary>
        public void MethodWithFileTypeAttribute([FileType(".jpg", ".png")] IFormFile file)
        {
            // This should not trigger the analyzer
        }

        /// <summary>
        /// This should NOT trigger analyzer warning (not a file parameter)
        /// </summary>
        public void MethodWithStringParameter(string text)
        {
            // This should not trigger the analyzer
        }
    }
}