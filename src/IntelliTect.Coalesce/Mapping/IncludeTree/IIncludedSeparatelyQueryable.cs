using System.Linq;

namespace IntelliTect.Coalesce.Mapping.IncludeTrees;

public interface IIncludedSeparatelyQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
{
}
