using IntelliTect.Coalesce.Models;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IO;
using System.Linq;

namespace IntelliTect.Coalesce.Swashbuckle;

public class CoalesceDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Get rid of bad schema definitions that we don't use.
        var defsToRemove = swaggerDoc.Components.Schemas.Keys.Where(d =>
            // These get incorrectly put here if your API surface has file responses.
            d.Equals(nameof(IFile)) ||
            d.Equals("IFileItemResult") ||
            d.Equals(nameof(Stream))
        )
        .ToList();

        foreach (var definition in defsToRemove)
        {
            swaggerDoc.Components.Schemas.Remove(definition);
        }
    }
}
