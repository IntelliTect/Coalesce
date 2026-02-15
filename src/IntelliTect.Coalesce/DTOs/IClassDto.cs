using Microsoft.EntityFrameworkCore;
using System;

namespace IntelliTect.Coalesce;

public interface IParameterDto<T>
    where T : class
{
    void MapTo(T obj, IMappingContext context);

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

public interface IResponseDto<T>
{
    void MapFrom(T obj, IMappingContext context, IncludeTree? tree = null);
}

/// <summary>
/// A implementation of an <see cref="IClassDto{T}"/> exposed by Coalesce 
/// will produce an API that behaves like an entity CRUD API,
/// but where the properties exposed by the API and the mapping to and 
/// from those properties is written by hand instead of being generated 
/// automatically from an existing DB-mapped entity.
/// </summary>
/// <typeparam name="T">
/// The DB-mapped entity type that this DTO is based on,
/// serving as the source and target for incoming queries, saves, and deletes.
/// </typeparam>
public interface IClassDto<T> : IParameterDto<T>, IResponseDto<T>
    where T : class
{
}

/// <summary>
/// Marker interface for an <see cref="IClassDto{T}"/> that is based on a <see cref="DbContext"/>
/// where that <see cref="DbContext"/> is not exposed directly with Coalesce or if the <see cref="DbContext"/>
/// owning the entity type <typeparamref name="T"/> cannot otherwise be determined.
/// </summary>
/// <typeparam name="T">The entity type that the DTO is based upon.</typeparam>
/// <typeparam name="TContext">The <see cref="DbContext"/> that provides the entity described by <typeparamref name="T"/></typeparam>
public interface IClassDto<T, TContext> : IClassDto<T>
    where T : class
    where TContext : DbContext
{ }
