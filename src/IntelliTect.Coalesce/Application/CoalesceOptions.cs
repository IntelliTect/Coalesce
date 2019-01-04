using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce
{

    public class CoalesceOptions
    {
        /// <summary>
        /// Determines whether API controllers will return the <see cref="Exception.Message"/> 
        /// of unhandled exceptions or not. 
        /// Defaults to true if <see cref="IHostingEnvironment.EnvironmentName"/> is "Development"; otherwise false.
        /// </summary>
        public bool DetailedExceptionMessages { get; set; }
    }
}
