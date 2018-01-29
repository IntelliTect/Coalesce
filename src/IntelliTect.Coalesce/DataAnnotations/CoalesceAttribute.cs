using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce
{
    /// <summary>
    /// The targeted class or method should be exposed by Coalesce.
    /// Different types will be exposed in different ways. See documentation for details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Interface)]
    public sealed class CoalesceAttribute : Attribute
    {

    }
}
