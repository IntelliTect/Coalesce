using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace IntelliTect.Coalesce
{
    public interface IMappingContext
    {
        string? Includes { get; }
        Dictionary<object, object> MappedObjects { get; }
        ClaimsPrincipal User { get; }

        bool IsInRoleCached(string role);

        void AddMapping(object sourceObject, object mappedObject);

        bool TryGetMapping<TDto>(
            object sourceObject,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out TDto? mappedObject
        )
            where TDto : class;

        IPropertyRestriction GetPropertyRestriction<TModel>(Type type);
        TRestriction GetPropertyRestriction<TRestriction, TModel>()
            where TRestriction : IPropertyRestriction
        {
            return (TRestriction)GetPropertyRestriction<TModel>(typeof(TRestriction));
        }

        IPropertyRestriction GetPropertyRestriction(Type type);
        TRestriction GetPropertyRestriction<TRestriction>()
            => (TRestriction)GetPropertyRestriction(typeof(TRestriction));
    }
}