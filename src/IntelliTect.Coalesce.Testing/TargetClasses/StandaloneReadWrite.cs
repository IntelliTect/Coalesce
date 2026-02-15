using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

[Coalesce, StandaloneEntity]
[SemanticKernel("StandaloneReadWrite", DeleteEnabled = true, SaveEnabled = true)]
public class StandaloneReadWrite
{
    public int Id { get; set; }

    [Search(SearchMethod = SearchAttribute.SearchMethods.Contains), ListText]
    public string Name { get; set; } = "";

    [DefaultOrderBy]
    public DateTimeOffset Date { get; set; }

    private static int nextId = 0;
    private static ConcurrentDictionary<int, StandaloneReadWrite> backingStore = new ConcurrentDictionary<int, StandaloneReadWrite>();
    public class DefaultSource : StandardDataSource<StandaloneReadWrite>
    {
        public DefaultSource(CrudContext context) : base(context) { }

        public override Task<IQueryable<StandaloneReadWrite>> GetQueryAsync(IDataSourceParameters parameters)
            => Task.FromResult(backingStore.Values.AsQueryable());
    }

    public class Behaviors : StandardBehaviors<StandaloneReadWrite>
    {
        public Behaviors(CrudContext context) : base(context) { }

        public override Task ExecuteDeleteAsync(StandaloneReadWrite item)
        {
            backingStore.TryRemove(item.Id, out _);
            return Task.CompletedTask;
        }

        public override Task ExecuteSaveAsync(SaveKind kind, StandaloneReadWrite oldItem, StandaloneReadWrite item)
        {
            if (kind == SaveKind.Create)
            {
                item.Id = Interlocked.Increment(ref nextId);
                backingStore.TryAdd(item.Id, item);
            }
            else
            {
                backingStore.TryRemove(item.Id, out _);
                backingStore.TryAdd(item.Id, item);
            }
            return Task.CompletedTask;
        }
    }
}
