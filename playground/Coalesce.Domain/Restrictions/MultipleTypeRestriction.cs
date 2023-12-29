using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using System.Linq;

namespace Coalesce.Domain.Restrictions;

public class MultipleTypeRestriction(AppDbContext db) : IPropertyRestriction<Case>, IPropertyRestriction<CaseProduct>
{
    public bool UserCanRead(IMappingContext context, string propertyName, CaseProduct model)
    {
        return db.CaseProducts.Any() && propertyName != null;
    }

    public bool UserCanRead(IMappingContext context, string propertyName, Case model)
    {
        return db.Cases.Any();
    }

    public bool UserCanWrite(IMappingContext context, string propertyName, CaseProduct? model, object? incomingValue)
    {
        return model?.Case.Status is Case.Statuses.Open;
    }

    public bool UserCanWrite(IMappingContext context, string propertyName, Case? model, object? incomingValue)
    {
        return model?.Status is Case.Statuses.Open;
    }
}
