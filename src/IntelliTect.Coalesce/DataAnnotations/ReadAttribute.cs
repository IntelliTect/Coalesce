using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// The Class or Property is read only for the users and groups and not accessible to others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadAttribute: SecurityAttribute
    {
    }
}
