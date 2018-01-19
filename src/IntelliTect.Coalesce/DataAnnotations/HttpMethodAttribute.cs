using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Allows specifying if this method should use an HTTP method type other than POST.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class HttpMethodAttribute : System.Attribute
    {
        public enum HttpMethodType
        {
            Post = 0,
            Get = 1,
            Put = 2,
            Delete = 3
        }

        public HttpMethodAttribute(HttpMethodType methodType = HttpMethodType.Post)
        {
            this.MethodType = methodType;
        }

        public HttpMethodType MethodType { get; set; }

        public Type test { get; }
    }
}
