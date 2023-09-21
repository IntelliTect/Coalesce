using System.Collections.Generic;
using System.Security.Claims;

namespace IntelliTect.Coalesce
{
    public interface IMappingContext
    {
        string? Includes { get; }
        Dictionary<object, object> MappedObjects { get; }
        ClaimsPrincipal? User { get; }

        bool IsInRoleCached(string role);

        void AddMapping(object sourceObject, object mappedObject);

        bool TryGetMapping<TDto>(
            object sourceObject,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out TDto? mappedObject
        )
            where TDto : class;
    }
}