using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Linq;

namespace Coalesce.Domain.Restrictions;

public class MultipleTypeRestriction(AppDbContext db) : IPropertyRestriction<Case>, IPropertyRestriction<CaseProduct>
{
    bool IPropertyRestriction<CaseProduct>.UserCanRead(IMappingContext context, string propertyName, CaseProduct model)
    {
        return db.CaseProducts.Any() && propertyName != null;
    }

    bool IPropertyRestriction<Case>.UserCanRead(IMappingContext context, string propertyName, Case model)
    {
        return db.Cases.Any();
    }

    bool IPropertyRestriction<CaseProduct>.UserCanWrite(IMappingContext context, string propertyName, CaseProduct? model, object? incomingValue)
    {
        return model?.Case.Status is Case.Statuses.Open;
    }

    bool IPropertyRestriction<Case>.UserCanWrite(IMappingContext context, string propertyName, Case? model, object? incomingValue)
    {
        return model?.Status is Case.Statuses.Open;
    }
}
