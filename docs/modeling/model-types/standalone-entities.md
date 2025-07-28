
# Standalone Entities

<!-- MARKER:summary -->
Standalone Entities are CRUD model types that support all the standard features of [CRUD Models](/modeling/model-types/crud.md), but are not required to be based on Entity Framework. Instead, you the developer must define a data source that produces instances of the model. This **can** be an Entity Framework query, but could also be any other mechanism that you can imagine and write in C#.
<!-- MARKER:summary-end -->

To define a Standalone Entity:
1. Make a class and annotate it with `[Coalesce, StandaloneEntity]`.
2. Define a default [Data Source](/modeling/model-components/data-sources.md) for the type, usually inheriting from `StandardDataSource<T>`
3. Optionally, define a [Behaviors](/modeling/model-components/behaviors.md) class as well to give the model create/edit/delete capabilities. If no behaviors are defined, the model is read-only.


## Read-only with EF backing store

In the below example, the standalone entity `PageListing` is used as a lightweight, read-only representation of a `Page` EF entity,
with some properties omitted for performance (`Content`) and other properties simplified (`Author`).

``` c#
[Coalesce, StandaloneEntity]
public class PageListing
{
    public int Id { get; set; }

    [Search(SearchMethod = SearchAttribute.SearchMethods.Contains), ListText]
    public string Title { get; set; } = "";

    [DefaultOrderBy(OrderByDirections.Descending)]
    public DateTimeOffset Date { get; set; }

    public string Author { get; set; }

    public class DefaultSource(CrudContext<AppDbContext> context) 
        : StandardDataSource<PageListing>(context)
    {
        public override Task<IQueryable<PageListing>> GetQueryAsync(IDataSourceParameters parameters)
        => context.DbContext.Pages
            .Where(p => p.IsPublished)
            .Select(p => new PageListing 
            {
                Id = p.Id,
                Title = p.Title,
                DateModified = p.Date,
                Author = p.CreatedBy.FullName
            });
    }
}

// EF entity model
public class Page 
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTimeOffset DateModified { get; set; }
    public string Content { get; set; }
    public bool IsPublished { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; }
}
```


## Read/write with EF backing store

Building on the previous example, we can make the `Title` of a `PageListing` editable as follows:

``` c#
[Coalesce, StandaloneEntity]
[Create(DenyAll)]
[Delete(DenyAll)]
public class PageListing
{
    // properties and data source same as previous example.

    public class Behaviors(CrudContext<AppDbContext> context) : StandardBehaviors<PageListing>(context)
    {
        public override Task ExecuteSaveAsync(SaveKind kind, PageListing? oldItem, PageListing item)
        {
            // Note: `page` is guaranteed to exist here because the `PageListing item` instance
            // is a projection from the Page table and was loaded from the type's data source 
            // immediately before ExecuteSaveAsync was called.
            var page = await context.DbContext.Pages.FindAsync(item.Id)!;

            // Perform mapping of properties that should be savable, from `item` to the backing entity.
            page.Title = item.Title;

            await context.DbContext.SaveChangesAsync();
        }

        public override Task ExecuteDeleteAsync(PageListing item) => throw new NotSupportedException();
    }
}
```

To add support for creates or deletes, implement the additional necessary actions in the overridden methods on the behaviors, and remove the DenyAll attributes.


``` c#
[Coalesce, StandaloneEntity]
public class PageListing
{
    // properties and data source same as previous example.

    public class Behaviors(CrudContext<AppDbContext> context) : StandardBehaviors<PageListing>(context)
    {
        public override async Task ExecuteSaveAsync(SaveKind kind, PageListing? oldItem, PageListing item)
        {
            Page page;
            if (kind == SaveKind.Create)
            {
                context.DbContext.Add(page = new Page { CreatedById = User.GetUserId() });
            }
            else
            {
                page = await context.DbContext.Pages.FindAsync(item.Id)!;
            }

            page.Title = item.Title;
            await context.DbContext.SaveChangesAsync();

            // Propagate the new primary key back to the standalone entity instance 
            // (in case this was a Create action instead of an Update).
            item.Id = page.Id;
        }

        public override async Task ExecuteDeleteAsync(PageListing item)
        {
            var page = await context.DbContext.Pages.FindAsync(item.Id)!;
            context.DbContext.Remove(page);
            await context.DbContext.SaveChangesAsync();
        }
    }
}
```

## Read-only without EF

Standalone entities can be created with *any* kind of backing store you can imagine - an in-memory store, a Redis instance, or an external REST API, for example.

The below example is admittedly contrived, as it is unlikely that you would be using an in-memory collection as a data persistence mechanism. A more likely real-world scenario would be to dependency inject an interface to some other data store.

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

    public class DefaultSource(CrudContext context) : StandardDataSource<StandaloneExample>(context)
    {
        public override Task<IQueryable<StandaloneExample>> GetQueryAsync(IDataSourceParameters parameters)
            => Task.FromResult(backingStore.Values.AsQueryable());
    }
}
```

## Read/write without EF

Building on the previous example, we can add support for saves and deletes by adding a Behaviors implementation:

``` c#

[Coalesce, StandaloneEntity]
public class StandaloneExample
{
    // properties and data source same as previous example.

    public class Behaviors(CrudContext context) : StandardBehaviors<StandaloneExample>(context)
    {
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
            else if (backingStore.TryGetValue(item.Id, out var storeItem))
            {
                storeItem.Name = item.Name;
                storeItem.Date = item.Date;
            }
            return Task.CompletedTask;
        }
    }
}
```

