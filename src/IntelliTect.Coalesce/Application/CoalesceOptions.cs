using IntelliTect.Coalesce.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce
{

    public class CoalesceOptions
    {
        /// <summary>
        /// Determines whether API controllers will return the <see cref="Exception.Message"/> 
        /// of unhandled exceptions or not. 
        /// Defaults to true if IHostingEnvironment.EnvironmentName is "Development"; otherwise false.
        /// </summary>
        public bool DetailedExceptionMessages { get; set; }

        /// <summary>
        /// A function that will transform an unhandled exception in API controllers
        /// into a custom ApiResult object that will be sent to the client.
        /// Return null to use the default response handling.
        /// </summary>
        public Func<ActionExecutedContext, ApiResult?>? ExceptionResponseFactory { get; set; }
    }
}
