using IntelliTect.Coalesce.Swashbuckle;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;

// Intentionally namespaced to IntelliTect.Coalesce.
namespace IntelliTect.Coalesce
{
    public static class CoalesceSwashbuckleExtensions
    {
        /// <summary>
        /// Adds Swashbuckle.AspNetCore.SwaggerGen filters to enhance definitions of Coalesce-generated APIs.
        /// </summary>
        public static void AddCoalesce(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.OperationFilter<CoalesceApiOperationFilter>();
            swaggerGenOptions.DocumentFilter<CoalesceDocumentFilter>();
        }
    }
}
