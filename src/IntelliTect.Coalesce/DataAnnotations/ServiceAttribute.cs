using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce
{
    /// <summary>
    /// <para>
    /// The targeted class or interface will be exposed as a service by Coalesce.
    /// </para>
    /// <para>
    /// All methods of a targeted interface, or all public methods marked with <see cref="CoalesceAttribute"/> of a targeted class, 
    /// will have an API endpoint generated.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class ServiceAttribute : Attribute
    {
    }
}
