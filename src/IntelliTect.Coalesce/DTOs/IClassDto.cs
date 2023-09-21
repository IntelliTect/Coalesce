using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Mapping;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System;

namespace IntelliTect.Coalesce
{
    public interface IClassDto<T>
        where T : class
    {
        void MapTo(T obj, IMappingContext context);

        void MapFrom(T obj, IMappingContext context, IncludeTree? tree = null);

        T MapToNew(IMappingContext context)
        {
            Type tType = typeof(T);

            T obj = Activator.CreateInstance(tType) as T
                ?? throw new InvalidOperationException(
                    $"IClassDto {this.GetType().Name} is based on type {tType.Name}, which does not have a parameterless constructor. " +
                    $"Please manually implement the IClassDto.MapToNew method."
                );

            MapTo(obj, context);
            return obj;
        }
    }

    /// <summary>
    /// Marker interface for an <see cref="IClassDto{T}"/> that is based on a <see cref="DbContext"/>
    /// where that <see cref="DbContext"/> is not exposed directly with Coalesce or if the <see cref="DbContext"/>
    /// owning the type <typeparamref name="T"/> cannot otherwise be determined.
    /// </summary>
    /// <typeparam name="T">The entity type that the DTO is based upon.</typeparam>
    /// <typeparam name="TContext">The <see cref="DbContext"/> that provides the entity described by <typeparamref name="T"/></typeparam>
    public interface IClassDto<T, TContext> : IClassDto<T>
        where T : class
        where TContext : DbContext
    { }
}
