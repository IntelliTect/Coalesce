using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Security.Claims;

namespace IntelliTect.Coalesce;

public interface IMappingContext
{
    string? Includes { get; }
    ClaimsPrincipal User { get; }

    bool IsInRoleCached(string role);

    void AddMapping(object sourceObject, IncludeTree? includeTree, object mappedObject);

    bool TryGetMapping<TDto>(
        object sourceObject,
        IncludeTree? includeTree,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
        out TDto? mappedObject
    )
        where TDto : class;

    IPropertyRestriction GetPropertyRestriction(Type type);
    TRestriction GetPropertyRestriction<TRestriction>()
        where TRestriction : IPropertyRestriction
        => (TRestriction)GetPropertyRestriction(typeof(TRestriction));

    Type GetResponseDtoType<TDto, T>(T entity)
        where TDto : class, IResponseDto<T>, new()
        where T : class;
}
