using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce;

public interface IResultTransformer<T>
    where T : class
{
    Task TransformResultsAsync(IReadOnlyList<T> results, IDataSourceParameters parameters);
}
