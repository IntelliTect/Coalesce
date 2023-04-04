# Data Sources

In Coalesce, all data that is retrieved from your database through the generated controllers is done so by a data source. These data sources control what data gets loaded and how it gets loaded. By default, there is a single generic data source that serves all data for your models in a generic way that fits many of the most common use cases - the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source).

In addition to this standard data source, Coalesce allows you to create custom data sources that provide complete control over the way data is loaded and serialized for transfer to a requesting client. These data sources are defined on a per-model basis, and you can have as many of them as you like for each model.


[[toc]]

## Defining Data Sources

By default, each of your models that Coalesce exposes will expose the standard data source (`IntelliTect.Coalesce.StandardDataSource<T, TContext>`). This data source provides all the standard functionality one would expect - paging, sorting, searching, filtering, and so on. Each of these component pieces is implemented in one or more virtual methods, making the `StandardDataSource` a great place to start from when implementing your own data source. To suppress this behavior of always exposing the raw `StandardDataSource`, create your own custom data source and annotate it with `[DefaultDataSource]`.

To implement your own custom data source, you simply need to define a class that implements `IntelliTect.Coalesce.IDataSource<T>`. To expose your data source to Coalesce, either place it as a nested class of the type `T` that you data source serves, or annotate it with the `[Coalesce]` attribute. Of course, the easiest way to create a data source that doesn't require you to re-engineer a great deal of logic would be to inherit from `IntelliTect.Coalesce.StandardDataSource<T, TContext>`, and then override only the parts that you need.

``` c#
public class Person
{
    [DefaultDataSource]
    public class IncludeFamily : StandardDataSource<Person, AppDbContext>
    {
        public IncludeFamily(CrudContext<AppDbContext> context) : base(context) { }

        public override IQueryable<Person> GetQuery(IDataSourceParameters parameters) 
            => Db.People
            .Where(f => User.IsInRole("Admin") || f.CreatedById == User.GetUserId())
            .Include(f => f.Parents).ThenInclude(s => s.Parents)
            .Include(f => f.Cousins).ThenInclude(s => s.Parents);
    }
}

[Coalesce]
public class NamesStartingWithA : StandardDataSource<Person, AppDbContext>
{
    public NamesStartingWithA(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<Person> GetQuery(IDataSourceParameters parameters) 
        => Db.People.Include(f => f.Siblings).Where(f => f.FirstName.StartsWith("A"));
}
```

The structure of the `IQueryable` built by the various methods of `StandardDataSource` is used to shape and trim the structure of the DTO as it is serialized and sent out to the client. One may also override method `IncludeTree GetIncludeTree(IQueryable<Person> query, IDataSourceParameters parameters)` to control this explicitly. See [Include Tree](/concepts/include-tree.md) for more information on how this works.

::: warning
If you create a custom data source that has custom logic for securing your data, be aware that the default implementation of `StandardDataSource` (or your custom default implementation - see below) is still exposed unless you annotate one of your custom data sources with `[DefaultDataSource]`. Doing so will replace the default data source with the annotated class for your type `T`.
:::


### Dependency Injection

All data sources are instantiated using dependency injection and your application's `IServiceProvider`. As a result, you can add whatever constructor parameters you desire to your data sources as long as a value for them can be resolved from your application's services. The single parameter to the `StandardDataSource` is resolved in this way - the `CrudContext<TContext>` contains the common set of objects most commonly used, including the `DbContext` and the `ClaimsPrincipal` representing the current user.


## Consuming Data Sources

<CodeTabs>
<template #vue>

The [ViewModels](/stacks/vue/layers/viewmodels.md#viewmodels) and [ListViewModels](/stacks/vue/layers/viewmodels.md#listviewmodels) each have a property called `$dataSource`. This property accepts an instance of a [DataSource](/stacks/vue/layers/models.md) class generated in the [Model Layer](/stacks/vue/layers/models.md).

``` ts
import { Person } from '@/models.g'
import { PersonViewModel, PersonListViewModel } from '@/viewmodels.g'

var viewModel = new PersonViewModel();
viewModel.$dataSource = new Person.DataSources.IncludeFamily();
viewModel.$load(1);

var list = new PersonListViewModel();
list.$dataSource = new Person.DataSources.NamesStartingWith();
list.$load(1);
```

</template>
<template #knockout>

The [TypeScript ViewModels](/stacks/ko/client/view-model.md) and [TypeScript ListViewModels](/stacks/ko/client/list-view-model.md) each have a property called `dataSource`. These properties accept an instance of a `Coalesce.DataSource<T>`. Generated classes that satisfy this relationship for all the data sources that were defined in C# may be found in the `dataSources` property on an instance of a ViewModel or ListViewModel, or in `ListViewModels.<ModelName>DataSources`

``` ts
var viewModel = new ViewModels.Person();
viewModel.dataSource = new viewModel.dataSources.IncludeFamily();
viewModel.load(1);

var list = new ListViewModels.PersonList();
list.dataSource = new list.dataSources.NamesStartingWith();
list.load();
```

</template>
</CodeTabs>



## Standard Parameters

All methods on `IDataSource<T>` take a parameter that contains all the client-specified parameters for things paging, searching, sorting, and filtering information. Almost all virtual methods on `StandardDataSource` are also passed the relevant set of parameters. 


## Custom Parameters

On any data source that you create, you may add additional properties annotated with `[Coalesce]` that will then be exposed as parameters to the client. These property parameters are currently restricted to primitives (numeric types, strings) and dates (DateTime, DateTimeOffset). Property parameter primitives may be expanded to allow for more types in the future.
    
``` c#
[Coalesce]
public class NamesStartingWith : StandardDataSource<Person, AppDbContext>
{
    public NamesStartingWith(CrudContext<AppDbContext> context) : base(context) { }

    [Coalesce]
    public string StartsWith { get; set; }

    public override IQueryable<Person> GetQuery(IDataSourceParameters parameters) 
        => Db.People.Include(f => f.Siblings)
        .Where(f => string.IsNullOrWhitespace(StartsWith) ? true : f.FirstName.StartsWith(StartsWith));
}
```

### List Auto-loading

You can setup [TypeScript List ViewModels](/stacks/disambiguation/list-view-model.md) to automatically reload from the server when data source parameters change:

<CodeTabs>
<template #vue>

To automatically reload a [ListViewModel](/stacks/vue/layers/viewmodels.md) when data source parameters change, simply use the list's `$useAutoLoad` or `$startAutoLoad` function:

``` ts
import { Person } from '@/models.g';
import { PersonListViewModel } from '@/viewmodels.g';

const list = new PersonListViewModel;
const dataSource = list.$dataSource = new Person.DataSources.NamesStartingWith
list.$useAutoLoad(); // When using options API, use $startAutoLoad(this)

// Trigger a reload:
dataSource.startsWith = "Jo";
```

</template>
<template #knockout>

The properties created on the TypeScript objects are observables so they may be bound to directly. In order to automatically reload a list when a data source parameter changes, you must explicitly subscribe to it:

``` ts
var list = new ListViewModels.PersonList();
var dataSource = new list.dataSources.NamesStartingWith();
dataSource.startsWith("Jo");
dataSource.subscribe(list); // Enables automatic reloading.
list.dataSource = dataSource;
list.load();
```

</template>
</CodeTabs>



## Standard Data Source

The standard data sources, `IntelliTect.Coalesce.StandardDataSource<T>` and its EntityFramework-supporting sibling `IntelliTect.Coalesce.StandardDataSource<T, TContext>`, contain a significant number of properties and methods that can be utilized and/or overridden at your leisure.




### Default Loading Behavior

When an object or list of objects is requested, the default behavior of the the `StandardDataSource` is to load all of the immediate relationships of the object (parent objects and child collections), as well as the far side of [many-to-many](attributes/many-to-many.md) relationships. This is performed in `StandardDataSource.GetQuery()`, so in order to suppress this behavior in a custom data source, don't build you query off of `base.GetQuery()`, but instead start directly from the `DbSet` for your entity when building your custom query.

Clients can suppress this per-request by setting `includes = "none"` on your TypeScript [ViewModel](/stacks/disambiguation/view-model.md) or [ListViewModel](/stacks/disambiguation/list-view-model.md), but note this is not a security mechanism and should only be used to reduce payload size or improve response time.

On the server, you can suppress this behavior by placing `[Read(NoAutoInclude = true)]` on either an entire class (affecting all navigation properties of that type), or on specific navigation properties. When placed on a entity class that holds sensitive data, this can help ensure you don't accidentally leak records due to forgetting to customize the data sources of the types whose navigation properties reference your sensitive entity.



### Properties

The following properties are available for use on the `StandardDataSource` any any derived instances.

<Prop def="CrudContext<TContext> Context" />

The object passed to the constructor that contains the set of objects needed by the standard data source, and those that are most likely to be used in custom implementations.

<Prop def="TContext Db" />

An instance of the DbContext that contains a `DbSet<T>` for the entity served by the data source.

<Prop def="ClaimsPrincipal User" />

The user making the current request.

<Prop def="int MaxSearchTerms" />

The max number of search terms to process when interpreting a search term word-by-word. Override by setting a value in the constructor.

<Prop def="int DefaultPageSize" />

The page size to use if none is specified by the client.  Override by setting a value in the constructor.

<Prop def="int MaxPageSize" />

The maximum page size that will be served. By default, client-specified page sizes will be clamped to this value. Override by setting a value in the constructor.

### Method Overview

The standard data source contains 19 different methods which can be overridden in your derived class to control its behavior. 

These methods often call one another, so overriding one method may cause some other method to no longer be called. The hierarchy of method calls, ignoring any logic or conditions contained within, is as follows:

``` :no-line-numbers
GetMappedItemAsync
    GetItemAsync
        GetQueryAsync
            GetQuery
        GetIncludeTree
    TransformResults

GetMappedListAsync
    GetListAsync
        GetQueryAsync
            GetQuery
        ApplyListFiltering
            ApplyListPropertyFilters
                ApplyListPropertyFilter
            ApplyListSearchTerm
        GetListTotalCountAsync
        ApplyListSorting
            ApplyListClientSpecifiedSorting
            ApplyListDefaultSorting
        ApplyListPaging
        GetIncludeTree
    TrimListFields
    TransformResults

GetCountAsync
    GetQueryAsync
        GetQuery
    ApplyListFiltering
        ApplyListPropertyFilters
            ApplyListPropertyFilter
        ApplyListSearchTerm
    GetListTotalCountAsync
```

### Method Details

All of the methods outlined above can be overridden. A description of each of the non-interface inner methods is as follows:
    
<Prop def="IQueryable<T> GetQuery(IDataSourceParameters parameters);
Task<IQueryable<T>> GetQueryAsync(IDataSourceParameters parameters);" />

The method is the one that you will most commonly be override in order to implement custom query logic. The default implementation of GetQueryAsync simply calls GetQuery - be aware of this in cases of complex overrides/inheritance. From this method, you could:

- Specify additional query filtering such as row-level security or soft-delete logic. Or, restrict the data source entirely for users or whole roles by returning an empty query.
- Include additional data using EF's `.Include()` and `.ThenInclude()`.
- Add additional edges to the serialized object graph using Coalesce's `.IncludedSeparately()` and `.ThenIncluded()`.

::: tip Note
When `GetQuery` is overridden, the [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior) is overridden as well. To restore this behavior, use the `IQueryable<T>.IncludeChildren()` extension method to build your query.
:::

<Prop def="IncludeTree? GetIncludeTree(IQueryable<T> query, IDataSourceParameters parameters)" />

Allows for explicitly specifying the [Include Tree](/concepts/include-tree.md) that will be used when serializing results obtained from this data source into DTOs. By default, the query that is build up through all the other methods in the data source will be used to build the include tree.

<Prop def="bool CanEvalQueryAsynchronously(IQueryable<T> query)" />

Called by other methods in the standard data source to determine whether or not EF Core async methods will be used to evaluate queries. This may be globally disabled when bugs like https://github.com/dotnet/SqlClient/issues/593 are present in EF Core.


<Prop def="IQueryable<T> ApplyListFiltering(IQueryable<T> query, IFilterParameters parameters)" />

A simple wrapper that calls `ApplyListPropertyFilters` and `ApplyListSearchTerm`.


<Prop def="IQueryable<T> ApplyListPropertyFilters(IQueryable<T> query, IFilterParameters parameters)" />

For each value in `parameters.Filter`, invoke `ApplyListPropertyFilter` to apply a filter to the query.


<Prop def="IQueryable<T> ApplyListPropertyFilter(IQueryable<T> query, PropertyViewModel prop, string value)" />

Given a property and a client-provided string value, perform some filtering on that property.

<!-- MARKER:filter-behaviors -->
- Dates with a time component will be matched exactly.
- Dates with no time component will match any dates that fell on that day.
- Strings will match exactly unless an asterisk is found, in which case they will be matched with `string.StartsWith`.
- Enums will match by string or numeric value. Multiple comma-delimited values will create a filter that will match on any of the provided values.
- Numeric values will match exactly. Multiple comma-delimited values will create a filter that will match on any of the provided values.
<!-- MARKER:end-filter-behaviors -->


<Prop def="IQueryable<T> ApplyListSearchTerm(IQueryable<T> query, IFilterParameters parameters)" />

Applies filters to the query based on the specified search term. See [[Search]](/modeling/model-components/attributes/search.md) for a detailed look at how searching works in Coalesce.


<Prop def="IQueryable<T> ApplyListSorting(IQueryable<T> query, IListParameters parameters)" />

If any client-specified sort orders are present, invokes `ApplyListClientSpecifiedSorting`. Otherwise, invokes `ApplyListDefaultSorting`.


<Prop def="IQueryable<T> ApplyListClientSpecifiedSorting(IQueryable<T> query, IListParameters parameters)" />

Applies sorting to the query based on sort orders specified by the client. If the client specified `"none"` as the sort field, no sorting will take place.


<Prop def="IQueryable<T> ApplyListDefaultSorting(IQueryable<T> query)" />

Applies default sorting behavior to the query, including behavior defined with use of `[DefaultOrderBy]` in C# POCOs, as well as fallback sorting to `"Name"` or primary key properties.

<!-- .. TODO - need a centralized doc page about sorting in Coalesce. -->


<Prop def="IQueryable<T> ApplyListPaging(IQueryable<T> query, IListParameters parameters, int? totalCount, out int page, out int pageSize)" />

Applies paging to the query based on incoming parameters. Provides the actual page and pageSize that were used as out parameters.
    

<Prop def="Task<int> GetListTotalCountAsync(IQueryable<T> query, IFilterParameters parameters)" />

Simple wrapper around invoking `.Count()` on a query. 


<Prop def="void TransformResults(IReadOnlyList<T> results, IDataSourceParameters parameters);
Task TransformResultsAsync(IReadOnlyList<T> results, IDataSourceParameters parameters);" />

Allows for transformation of a result set after the query has been evaluated. 
This will be called for both lists of items and for single items. This can be used for populating non-mapped properties on a model, or conditionally loading navigation properties using logic that depends upon the contents of each loaded record.

This method is only called immediately before mapping to a DTO; it does not affect operations that don't involve mapping to a DTO - e.g. when loading the target of a `/save` operation or when loading the invocation target of an [instance method](/modeling/model-components/methods.md#instance-methods).

See the [Security](/topics/security.md#transform-results) page for an example on how to use TransformResults to [apply filtered includes](/topics/security.md#transform-results).

Do not use `TransformResults` to modify any database-mapped scalar properties, since such changes could be inadvertently persisted to the database.


<Prop def="IList<TDto> TrimListFields<TDto>(IList<TDto> mappedResult, IListParameters parameters)" />

Performs trimming of the fields of the result set based on the parameters given to the data source. Can be overridden to forcibly disable this, override the behavior to always trim specific fields, or any other functionality desired.


## Globally Replacing the Standard Data Source

You can, of course, create a custom base data source that all your custom implementations inherit from. But, what if you want to override the standard data source across your entire application, so that `StandardDataSource<,>` will never be instantiated? You can do that too!

Simply create a class that implements `IEntityFrameworkDataSource<,>` (the `StandardDataSource<,>` already does - feel free to inherit from it), then register it at application startup like so:


``` c#
public class MyDataSource<T, TContext> : StandardDataSource<T, TContext>
    where T : class
    where TContext : DbContext
{
    public MyDataSource(CrudContext<TContext> context) : base(context)
    {
    }

    ...
}
```

``` c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddCoalesce(b =>
    {
        b.AddContext<AppDbContext>();
        b.UseDefaultDataSource(typeof(MyDataSource<,>));
    });
}
```

Your custom data source must have the same generic type parameters - `<T, TContext>`. Otherwise, the Microsoft.Extensions.DependencyInjection service provider won't know how to inject it.
