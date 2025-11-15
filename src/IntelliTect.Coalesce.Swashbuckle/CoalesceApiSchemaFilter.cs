using IntelliTect.Coalesce.Models;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IntelliTect.Coalesce.Swashbuckle;

public class CoalesceApiSchemaFilter : ISchemaFilter
{
#if NET10_0_OR_GREATER
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
#else
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
#endif
    {
        if (context.Type.IsAssignableTo(typeof(ISparseDto)))
        {
            string description = "This type supports partial/surgical modifications. Properties that are entirely omitted/undefined will be left unchanged on the target object.";

            if (!string.IsNullOrWhiteSpace(schema.Description)) schema.Description += " " + description;
            else schema.Description = description;
        }

    }
}
