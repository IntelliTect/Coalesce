using IntelliTect.Coalesce.Swashbuckle;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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
            swaggerGenOptions.SchemaFilter<CoalesceApiSchemaFilter>();

            var oldResolver = swaggerGenOptions.SwaggerGeneratorOptions.ConflictingActionsResolver;
            swaggerGenOptions.SwaggerGeneratorOptions.ConflictingActionsResolver = (actions) =>
            {
                return actions.FirstOrDefault(a => a.SupportedRequestFormats
                        .Any(f => f.MediaType == MediaTypeNames.Application.Json)
                    )
                    ?? oldResolver?.Invoke(actions);
            };
        }
    }
}
