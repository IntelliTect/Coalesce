# Custom DTOs

In addition to the generated [Generated C# DTOs](/stacks/agnostic/dtos.md) that Coalesce will create for you, you may also create your own implementations of an `IClassDto`. These types are first-class citizens in Coalesce - you will get a full suite of features surrounding them as if they were entities. This includes generated API Controllers, Admin Views, and full [TypeScript ViewModels](/stacks/disambiguation/view-model.md) and [TypeScript List ViewModels](/stacks/disambiguation/list-view-model.md).

[[toc]]

The difference between a Custom DTO and the underlying entity that they represent is as follows:

- The only time your custom DTO will be served is when it is requested directly from one of the endpoints on its generated controller, or when its type is explicitly used by a [method](/modeling/model-components/methods.md) or [property](../model-components/properties.md) of another type.

- When mapping data from your database, or mapping data incoming from the client, the DTO itself must manually map all properties, since there is no corresponding [Generated DTO](/stacks/agnostic/dtos.md). Attributes like [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md) and property-level security through [Security Attributes](/modeling/model-components/attributes/security-attribute.md) have no effect on custom DTOs, since those attribute only affect what get generated for [Generated C# DTOs](/stacks/agnostic/dtos.md).


## Creating a Custom DTO

To create a custom DTO, define a class annotated with [[Coalesce]](/modeling/model-components/attributes/coalesce.md) that implements `IClassDTO<T>`, where `T` is an EF Core POCO with a corresponding `DbSet<T>` on a `DbContext` that has also been exposed with [[Coalesce]](/modeling/model-components/attributes/coalesce.md). Add any [Properties](/modeling/model-components/properties.md) to it just as you would add [model properties](/modeling/model-components/properties.md) to a regular EF model. If you are not exposing a `DbContext` class with [[Coalesce]](/modeling/model-components/attributes/coalesce.md) but still wish to create a Custom DTO based upon one of its entities, you can inherit from `IClassDTO<T, TContext>` instead as a means of explicitly declaring the type of the DbContext.

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

The [Vue ViewModels](../../stacks/vue/layers/viewmodels.md) support surgical saves through their `$saveMode` property, and [Knockout ViewModels](../../stacks/ko/client/view-model.md) through the [`saveIncludedFields` configuration](../../stacks/ko/client/model-config.md#viewmodelconfiguration).

<!-- MARKER:surgical-saves-warning -->
Surgical saves require DTOs on the server that are capable of determining which of their properties have been set by the model binder, as surgical saves are sent from the client by entirely omitting properties from the ``x-www-form-urlencoded`` body that is sent to the server.

The [Generated C# DTOs](/stacks/agnostic/dtos.md) implement the necessary logic for this; however, any [Custom DTOs](/modeling/model-types/dtos.md) must have this logic manually written by you, the developer. Either implement the same pattern that can be seen in the [Generated C# DTOs](/stacks/agnostic/dtos.md), or do not use surgical saves with Custom DTOs.
<!-- MARKER:end-surgical-saves-warning -->