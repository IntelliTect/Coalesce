# Custom DTOs

<!-- MARKER:summary -->
In addition to the generated [Generated C# DTOs](/stacks/agnostic/dtos.md) that Coalesce will create for you for [EF Entity Models](/modeling/model-types/entities.md) and [Standalone Entities](/modeling/model-types/standalone-entities.md), you may also create your own implementation of an `IClassDto` with customized properties and expose it as if it was a first-class CRUD Model type. These are known as Custom DTOs and support all the standard features of [CRUD Models](/modeling/model-types/crud.md).

::: warning Note
Custom DTOs are an advanced feature of Coalesce and are not needed by most applications. In almost all cases, [EF Entity Models](/modeling/model-types/entities.md) can be customized to handle any needs that you might want to use a Custom DTO for, or [Standalone Entities](/modeling/model-types/standalone-entities.md) can be used to serve an alternate projection of an EF Entity Model.
:::
<!-- MARKER:summary-end -->

## Purpose

Custom DTOs have a fair amount of overlap with the capabilities of [Standalone Entities](/modeling/model-types/standalone-entities.md).

- Both can expose a lightweight or alternate representation of an entity. For example, a "listing" version of an entity with large data members omitted (such that for performance, they've never retrieved from the database).
- Both choose exactly how each property value is mapped between the client and the database. Each property mapping is written by hand by the developer.
- Both expose their own set of API endpoints (`/get`, `/list`, `/save`, etc).

However, standalone entities have the following advantages over custom DTOs:

- Standalone entities are significantly easier to write than custom DTOs for [read-only use cases](./standalone-entities.md#read-only-with-ef-backing-store).
- Standalone entities aren't limited to being based on a specific, single EF Entity, while custom DTOs must always choose an entity type to be based upon.
- Standalone entities support all security attributes, while custom DTOs do not support property-level [Security Attributes](/modeling/model-components/attributes/security-attribute.md), nor [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md). In custom DTOs, this logic must be written by hand in the MapTo/MapFrom methods when it is needed.
- Standalone entities support surgical saves easily, while custom DTOs [require significant extra code](#surgical-saves).

Custom DTOs have the following advantages over standalone entities:

- You write all the mapping logic between the DTO and the entity, which can make it easier to implement large amounts of custom logic at this layer that standalone entities would need to implement with [Restrictions](/modeling/model-components/attributes/restrict.md). While these kinds of mapping restrictions can also be written in a standalone entity's [projected EF query](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying#project-only-properties-you-need), some logic can be difficult or impossible to represent in an EF query expression.
- Create/Update/Delete operations can be done without writing a [Behaviors](/modeling/model-components/behaviors.md) class.



## Creating a Custom DTO

To create a custom DTO, define a class annotated with [[Coalesce]](/modeling/model-components/attributes/coalesce.md) that implements `IClassDTO<T>`, where `T` is an EF Entity Model with a corresponding `DbSet<T>` on a `DbContext` that has also been exposed with [[Coalesce]](/modeling/model-components/attributes/coalesce.md). Add any [Properties](/modeling/model-components/properties.md) to it just as you would add [model properties](/modeling/model-components/properties.md) to a regular EF model. If you are not exposing a `DbContext` class with [[Coalesce]](/modeling/model-components/attributes/coalesce.md) but still wish to create a Custom DTO based upon one of its entities, you can inherit from `IClassDTO<T, TContext>` instead as a means of explicitly declaring the type of the DbContext.

Next, ensure that one property is annotated with `[Key]` so that Coalesce can know the primary key of your DTO in order to perform database lookups and keep track of your object uniquely in the client-side TypeScript.

Now, populate the required `MapTo` and `MapFrom` methods with code for mapping from and to your DTO, respectively (the methods are named with respect to the underlying entity, not the DTO). Most properties probably map one-to-one in both directions, but you probably created a DTO because you wanted some sort of custom mapping - say, mapping a collection on your entity with a comma-delimited string on the DTO. This is also the place to perform any user-based, role-based, property-level security. You can access the current user on the `IMappingContext` object. 

``` c#
[Coalesce]
public class CaseDto : IClassDto<Case>
{
    [Key]
    public int CaseId { get; set; }

    public string Title { get; set; }

    [Read]
    public string AssignedToName { get; set; }

    public void MapTo(Case obj, IMappingContext context)
    {
        obj.Title = Title;
    }

    public void MapFrom(Case obj, IMappingContext context = null, IncludeTree tree = null)
    {
        CaseId = obj.CaseKey;
        Title = obj.Title;
        if (obj.AssignedTo != null)
        {
            AssignedToName = obj.AssignedTo.Name;
        }
    }
}
```

::: warning
Custom DTOs do not utilize property-level [Security Attributes](/modeling/model-components/attributes/security-attribute.md) nor [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md), since these are handled in the [Generated DTOs](/stacks/agnostic/dtos.md). If you need property-level security or trimming, you must write it yourself in the `MapTo` and `MapFrom` methods.
:::

If you have any child objects on your DTO, you can invoke the mapper for some other object using the static `Mapper` class. Also seen in this example is how to respect the [Include Tree](/concepts/include-tree.md) when mapping entity types; however, respecting the `IncludeTree` is optional. Since this DTO is a custom type that you've written, if you're certain your use cases don't need to worry about object graph trimming, then you can ignore the `IncludeTree`. If you do ignore the `IncludeTree`, you should pass `null` to calls to `Mapper` - don't pass in the incoming `IncludeTree`, as this could cause unexpected results.

``` c#
using IntelliTect.Coalesce.Mapping;

[Coalesce]
public class CaseDto : IClassDto<Case>
{
    public int ProductId { get; set; }
    public Product Product { get; set; }
    ...

    public void MapFrom(Case obj, IMappingContext context = null, IncludeTree tree = null)
    {
        ProductId = obj.ProductId;

        if (tree == null || tree[nameof(this.Product)] != null)
            Product = Mapper.MapToDto<Product, ProductDtoGen>(obj.Product, context, tree?[nameof(this.Product)]
        ...
    }
}
```

## Using Custom DataSources and Behaviors

### Declaring an IClassDto DataSource

When you create a custom DTO, it will use the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source) and [Standard Behaviors](/modeling/model-components/behaviors.md#standard-behaviors) just like any of your regular [Entity Models](/modeling/model-types/entities.md). If you wish to override this, your custom data source and/or behaviors MUST be declared in one of the following ways:

1. As a nested class of the DTO. The relationship between your data source or behaviors and your DTO will be picked up automatically.

    ``` c#
    [Coalesce]
    public class CaseDto : IClassDto<Case>
    {
        [Key]
        public int CaseId { get; set; }

        public string Title { get; set; }
        
        ...

        public class MyCaseDtoSource : StandardDataSource<Case, AppDbContext>
        {
            ...
        }
    }
    ```

2. With a `[DeclaredFor]` attribute that references the DTO type:

    ``` c#
    [Coalesce]
    public class CaseDto : IClassDto<Case>
    {
        [Key]
        public int CaseId { get; set; }

        public string Title { get; set; }
        
        ...
    }

    [Coalesce, DeclaredFor(typeof(CaseDto))]
    public class MyCaseDtoSource : StandardDataSource<Case, AppDbContext>
    {
        ...
    }
    ```

### ProjectedDtoDataSource

In addition to creating a [Data Source](/modeling/model-components/data-sources.md) by deriving from [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source), there also exists a class `ProjectedDtoDataSource` that can be used to easily perform projection from EF model types to your custom DTO types using EF query projections. `ProjectedDtoDataSource` inherits from [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source).

``` c#
[Coalesce, DeclaredFor(typeof(CaseDto))]
public class CaseDtoSource : ProjectedDtoDataSource<Case, CaseDto, AppDbContext>
{
    public CaseDtoSource(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<CaseDto> ApplyProjection(IQueryable<Case> query, IDataSourceParameters parameters)
    {
        return query.Select(c => new CaseDto
        {
            CaseId = c.CaseKey,
            Title = c.Title,
            AssignedToName = c.AssignedTo == null ? null : c.AssignedTo.Name
        });
    }
}
```

## Surgical Saves

<!-- MARKER:surgical-saves-warning -->
Surgical saves require DTOs on the server that are capable of determining which of their properties have been set by the model binder, as surgical saves are sent from the client by entirely omitting properties from the ``x-www-form-urlencoded`` body that is sent to the server.

The [Generated C# DTOs](/stacks/agnostic/dtos.md) implement the necessary logic for this; however, any [Custom DTOs](/modeling/model-types/dtos.md) must have this logic manually written by you, the developer. Either implement the same pattern that can be seen in the [Generated C# DTOs](/stacks/agnostic/dtos.md), or do not use surgical saves with Custom DTOs.
<!-- MARKER:end-surgical-saves-warning -->

The [Vue ViewModels](../../stacks/vue/layers/viewmodels.md) for custom DTOs have surgical saves disabled by default. This can be re-enabled through the [`$saveMode`](/stacks/vue/layers/viewmodels.md#member-_savemode) property if you've implemented the necessary logic on the server side.