#if NET10_0_OR_GREATER
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api.OpenApi;

/// <summary>
/// Workaround for https://github.com/dotnet/aspnetcore/issues/61038.
/// In .NET 10, when JsonNumberHandling.AllowReadingFromString is set (which is the ASP.NET Core default),
/// numeric types are incorrectly represented with a string type and pattern in the OpenAPI schema.
/// This transformer fixes numeric schemas to use the correct integer/number type.
/// A document transformer is used instead of a schema transformer because schema transformers
/// are not applied to schemas generated through CoalesceApiOperationFilter.AddOtherBodyTypes
/// or to allOf sub-schemas for polymorphic types.
/// </summary>
internal class CoalesceNumericSchemaTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is null) return Task.CompletedTask;

        foreach (var (_, schema) in document.Components.Schemas)
        {
            FixSchema(schema);
        }
        return Task.CompletedTask;
    }

    private static void FixSchema(IOpenApiSchema schema)
    {
        if (schema.Properties is not null)
        {
            foreach (var (_, propSchema) in schema.Properties)
            {
                FixNumericType(propSchema);
            }
        }

        if (schema.AllOf is not null)
        {
            foreach (var subSchema in schema.AllOf)
            {
                FixSchema(subSchema);
            }
        }

        if (schema.Items is not null)
        {
            FixNumericType(schema.Items);
        }
    }

    private static void FixNumericType(IOpenApiSchema schema)
    {
        if (schema is not OpenApiSchema mutableSchema) return;
        if (mutableSchema.Type is not JsonSchemaType type) return;
        if (!type.HasFlag(JsonSchemaType.String)) return;

        if (type.HasFlag(JsonSchemaType.Integer) || type.HasFlag(JsonSchemaType.Number))
        {
            mutableSchema.Type = type & ~JsonSchemaType.String;
            mutableSchema.Pattern = null;
        }
    }
}
#endif
