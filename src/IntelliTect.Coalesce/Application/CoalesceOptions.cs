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
    }
}
