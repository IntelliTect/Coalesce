using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RoutedAttribute : Attribute
    {
        public bool ApiRouted { get; set; } = true;

        // public bool ViewsRouted { get; set; } = true;
    }
}