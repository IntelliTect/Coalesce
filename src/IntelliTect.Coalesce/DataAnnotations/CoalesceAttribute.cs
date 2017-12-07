using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.DataAnnotations
{
    // TODO: allow specifying on an interface to generate service APIs off of an interface? This would be the best design.

    /// <summary>
    /// The targeted class or method should be exposed by Coalesce.
    /// Different types will be exposed in different ways. See documentation for details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class CoalesceAttribute : Attribute
    {

    }
}
