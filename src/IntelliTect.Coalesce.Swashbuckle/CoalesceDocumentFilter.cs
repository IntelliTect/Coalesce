using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace IntelliTect.Coalesce.Swashbuckle
{
    public class CoalesceDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            // Get rid of the empty, pointless definitions for IBehaviors and IDataSource.
            var defsToRemove = swaggerDoc.Definitions.Keys.Where(d =>
                d.StartsWith($"{nameof(IDataSource<object>)}[") ||
                d.StartsWith($"{nameof(IBehaviors<object>)}[") 
            )
            .ToList();

            foreach (var definition in defsToRemove)
            {
                swaggerDoc.Definitions.Remove(definition);
            }
        }
    }
}
