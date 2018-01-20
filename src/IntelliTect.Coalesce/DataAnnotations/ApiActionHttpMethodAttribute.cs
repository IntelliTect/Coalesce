using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Allows specifying if this method should use an HTTP method type other than POST in the generated API controller.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ApiActionHttpMethodAttribute : System.Attribute
    {
        public enum HttpMethod
        {
            Post = 0,
            Get = 1,
            Put = 2,
            Delete = 3, 
            Patch = 4
        }

        public ApiActionHttpMethodAttribute(HttpMethod method = HttpMethod.Post)
        {
            this.Method = method;
        }

        public HttpMethod Method { get; set; }
    }
}
