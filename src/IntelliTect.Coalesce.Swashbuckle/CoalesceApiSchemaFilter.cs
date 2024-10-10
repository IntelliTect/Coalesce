using IntelliTect.Coalesce.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IntelliTect.Coalesce.Swashbuckle
{
    public class CoalesceApiSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsAssignableTo(typeof(ISparseDto)))
            {
                string description = "This type supports partial/surgical modifications. Properties that are entirely omitted/undefined will be left unchanged on the target object.";

                if (!string.IsNullOrWhiteSpace(schema.Description)) schema.Description += " " + description;
                else schema.Description = description;
            }

        }
    }
}
