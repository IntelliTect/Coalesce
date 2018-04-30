using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Controls attributes of the API controller action generated for the target method.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ControllerActionAttribute : System.Attribute
    {
        public HttpMethod Method { get; set; }
    }

    public enum HttpMethod
    {
        Post = 0,
        Get = 1,
        Put = 2,
        Delete = 3,
        Patch = 4
    }
}
