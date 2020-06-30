using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ControllerAttribute : Attribute
    {
        public bool ApiRouted { get; set; } = true;

        /// <summary>
        /// If set, will determine the name of the generated API controller.
        /// </summary>
        public string? ApiControllerName { get; set; } = null;

        /// <summary>
        /// If set, will append this value to the default name of the generated API controller.
        /// This value will not be used if ApiControllerName is set.
        /// </summary>
        public string? ApiControllerSuffix { get; set; } = null;

        /// <summary>
        /// If true, generated actions will be marked protected instead of public.
        /// 
        /// In order to expose them, one must inherit from the generated controller
        /// and override each desired generated method via hiding (use 'new', not 'override').
        /// </summary>
        public bool ApiActionsProtected { get; set; } = false;

        // public bool ViewsRouted { get; set; } = true;
    }
}