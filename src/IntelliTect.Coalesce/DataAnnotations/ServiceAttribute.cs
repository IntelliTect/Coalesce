using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce
{
    /// <summary>
    /// The targeted class or interface will be exposed as a service by Coalesce.
    /// All of its methods (public if targeting a class) will have a corresponding API endpoint generated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class ServiceAttribute : Attribute
    {
    }
}
