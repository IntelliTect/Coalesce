
# Standalone Entities

In Coalesce, Standalone Entities are types that behave like [entity types](./entities.md) (they can support the full suite generated CRUD endpoints), but are not based on Entity Framework. These types are discovered by Coalesce by annotating them with `[Coalesce, StandaloneEntity]`.

For these types, you must define at least one custom [Data Source](/modeling/model-components/data-sources.md), and optionally a [Behaviors](/modeling/model-components/behaviors.md) class as well. If no behaviors are defined, the type is implicitly read-only, equivalent to turning off create/edit/delete via the [Security Attributes](/modeling/model-components/attributes/security-attribute.md).

To define data sources and behaviors for Standalone Entities, it is recommended you inherit from `StandardDataSource<T>` and `StandardBehaviors<T>`, respectively. For example:

``` c#
[Coalesce, StandaloneEntity]
public class StandaloneExample
{
    public int Id { get; set; }

    [Search(SearchMethod = SearchAttribute.SearchMethods.Contains), ListText]
    public string Name { get; set; } = "";

    [DefaultOrderBy]
    public DateTimeOffset Date { get; set; }

    private static int nextId = 0;
    private static ConcurrentDictionary<int, StandaloneExample> backingStore = new ConcurrentDictionary<int, StandaloneExample>();

    public class DefaultSource : StandardDataSource<StandaloneExample>
    {
        public DefaultSource(CrudContext context) : base(context) { }

        public override Task<IQueryable<StandaloneExample>> GetQueryAsync(IDataSourceParameters parameters)
            => Task.FromResult(backingStore.Values.AsQueryable());
    }

    public class Behaviors : StandardBehaviors<StandaloneExample>
    {
        public Behaviors(CrudContext context) : base(context) { }

        public override Task ExecuteDeleteAsync(StandaloneExample item)
        {
            backingStore.TryRemove(item.Id, out _);
            return Task.CompletedTask;
        }

        public override Task ExecuteSaveAsync(SaveKind kind, StandaloneExample? oldItem, StandaloneExample item)
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
```

The above example is admittedly contrived, as it is unlikely that you would be using an in-memory collection as a data persistence mechanism. A more likely real-world scenario would be to inject an interface to some other data store. Data Source and Behavior classes are instantiated using your application's service provider, so any registered service can be injected.