using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Should users be allowed to delete an entity via the API/button.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Class)]
    public class DeleteAttribute: SecurityAttribute
    {
    }
}
