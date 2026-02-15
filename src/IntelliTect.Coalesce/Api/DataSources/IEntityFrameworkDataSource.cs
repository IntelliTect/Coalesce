using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce;

/// <summary>
/// Marker interface for the data source that will be used when serving entity types for which no custom data source is resolved.
/// </summary>
/// <typeparam name="T">The entity type served</typeparam>
/// <typeparam name="TContext">The context that serves the entity.</typeparam>
public interface IEntityFrameworkDataSource<T, TContext> : IDataSource<T>
    where T : class
    where TContext : DbContext
{
}
