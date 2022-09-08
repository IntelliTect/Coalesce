using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace IntelliTect.Coalesce.Swashbuckle
{
    public class CoalesceDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Get rid of the empty, pointless definitions for IBehaviors and IDataSource.
            var defsToRemove = swaggerDoc.Components.Schemas.Keys.Where(d =>
                d.EndsWith(nameof(IDataSource<object>)) ||
                d.EndsWith(nameof(IBehaviors<object>))
            )
            .ToList();

            foreach (var definition in defsToRemove)
            {
                swaggerDoc.Components.Schemas.Remove(definition);
            }
        }
    }
}
