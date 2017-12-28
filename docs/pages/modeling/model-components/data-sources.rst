
.. _CustomDataSources:

Data Sources
------------

Coalesce allows you to create custom data sources that provide complete control over the way data is loaded and serialized for transfer to a requesting client. These data sources are defined on a per-model basis, and you can have as many of them as you like for each model.

Defining Data Sources
.....................

By default, each of your models that Coalesce exposes will expose the standard data source (:csharp:`IntelliTect.Coalesce.StandardDataSource<T, TContext>`). This data source provides all the standard functionality one would expect - paging, sorting, searching, filtering, and so on. Each of these component pieces is implemented in one or more virtual methods, making the :csharp:`StandardDataSource` a great place to start from when implementing your own data source. To suppress this behavior of always exposing the raw :csharp:`StandardDataSource`, create your own custom data source and annotate it with :csharp:`[DefaultDataSource]`.

To implement your own custom data source, you simply need to define a class that implements :csharp:`IntelliTect.Coalesce.IDataSource<T>`. To expose your data source to Coalesce, either place it as a nested class of the type :csharp:`T` that you data source serves, or annotate it with the :csharp:`[Coalesce]` attribute. Of course, the easiest way to create a data source that doesn't require you to re-engineer a great deal of logic would be to inherit from :csharp:`IntelliTect.Coalesce.StandardDataSource<T, TContext>`, and then override only the parts that you need.

    .. code-block:: c#

        public class Person
        {
            [DefaultDataSource]
            public class IncludeFamily : StandardDataSource<Person, AppDbContext>
            {
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
            public override IQueryable<Person> GetQuery(IDataSourceParameters parameters) 
                => Db.People.Include(f => f.Siblings).Where(f => f.FirstName.StartsWith("A"));
        }

The structure of the :csharp:`IQueryable` built by the various methods of :csharp:`StandardDataSource` is used to shape and trim the structure of the DTO as it is serialized and sent out to the client. One may also override method :csharp:`IncludeTree GetIncludeTree(IQueryable<Person> query, IDataSourceParameters parameters)` to control this explicitly. See :ref:`IncludeTree` for more information on how this works.

.. warning::
    If you create a custom data source that has custom logic for securing your data, be aware that the default implementation of :csharp:`StandardDataSource` (or your custom default implementation - see below) is still exposed unless you annotate one of your custom data sources with :csharp:`[DefaultDataSource]`. Doing so will replace the default data source with the annotated class for your type :csharp:`T`.

Consuming Data Sources
......................

The TypeScript ViewModels and ListViewModels have a property called :ts:`dataSource`. These properties accept an instance of a :ts:`Coalesce.DataSource<T>`. Generated classes that satisfy this relationship for all the data sources that were defined in C# may be found in the :ts:`dataSources` property on an instance of a ViewModel or ListViewModel, or in :ts:`ListViewModels.<ModelName>DataSources`

    .. code-block:: typescript

        var viewModel = new ViewModels.Person();
        viewModel.dataSource = new viewModel.dataSources.IncludeFamily();
        viewModel.load(1);

        var list = new ListViewModels.PersonList();
        list.dataSource = new list.dataSources.NamesStartingWith();
        list.load();


Standard Parameters
...................

All methods on :csharp:`IDataSource<T>` take a parameter that contains all the client-specified parameters for things paging, searching, sorting, and filtering information. Almost all overridable methods on :csharp:`StandardDataSource` are also passed the relevant set of parameters. 


Custom Parameters
.................

On any data source that you create, you may add additional properties annotated with :csharp:`[Coalesce]` that will then be exposed as parameters to the client. These property parameters are currently restricted to primitives (numeric types, strings) and dates (DateTime, DateTimeOffset). Property parameter primitives may be expanded to allow for more types in the future.
    
    .. code-block:: c#

        [Coalesce]
        public class NamesStartingWith : StandardDataSource<Person, AppDbContext>
        {
            [Coalesce]
            public string StartsWith { get; set; }

            public override IQueryable<Person> GetQuery(IDataSourceParameters parameters) 
                => Db.People.Include(f => f.Siblings)
                .Where(f => string.IsNullOrWhitespace(StartsWith) ? true : f.FirstName.StartsWith(StartsWith));
        }

The properties created on the TypeScript objects are observables so they may be bound to directly. In order to automatically reload a list when a data source parameter changes, you must explicitly subscribe to it:

    .. code-block:: typescript

        var list = new ListViewModels.PersonList();
        var dataSource = new list.dataSources.NamesStartingWithA();
        dataSource.startsWith("Jo");
        dataSource.subscribe(list); // Optional - call to enable automatic reloading.
        list.dataSource = dataSource;
        list.load();


.. _StandardDataSource:

Standard Data Source
....................

The standard data source, :csharp:`IntelliTect.Coalesce.StandardDataSource<T, TContext>`, contains a significant number of properties and methods that can be utilized and/or overridden at your leisure.

Properties
''''''''''

The following properties are availble for use on the :csharp:`StandardDataSource`

    :csharp:`TContext Db`
        An instance of the db context that contains a :csharp:`DbSet<T>` for the entity served by the data source.
    :csharp:`ClaimsPrincipal User`
        The user making the current request.
    :csharp:`int MaxSearchTerms`
        The max number of search terms to process when interpreting a search term word-by-word. Override by setting a value in the constructor.
    :csharp:`int DefaultPageSize`
        The page size to use if none is specified by the client.  Override by setting a value in the constructor.
    :csharp:`int MaxPageSize`
        The maximum page size that will be served. By default, client-specified page sizes will be clamped to this value. Override by setting a value in the constructor.

Method Overview
'''''''''''''''

The standard data source contains 19 different methods which can be overridden in your derived class to control its behavior. 

These methods often call one another, so overriding one method may cause some other method to no longer be called. The hierarchy of method calls, ignoring any logic or conditions contained within, is as follows:

    .. code-block:: c#

        IsAuthorized
        CanEvalQueryAsynchronously

        GetMappedItemAsync
            GetItemAsync
                GetQuery
                GetIncludeTree
            TransformResults

        GetMappedListAsync
            GetListAsync
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
            TransformResults
        
        GetCountAsync
            GetQuery
            ApplyListFiltering
                ApplyListPropertyFilters
                    ApplyListPropertyFilter
                ApplyListSearchTerm
            GetListTotalCountAsync

Method Details
''''''''''''''

All of the methods outlined above can be overridden. A description of each of the non-interface inner methods is as follows:
    
    :csharp:`GetQuery`
        The method is the one that you will most commonly be override in order to implement custom query logic. From this method, one could:

            - Specify additional query filtering such as row-level security or soft-delete logic.
            - Include additional data using EF's :csharp:`.Include()` and :csharp:`.ThenInclude()`.
            - Add additional edges to the serialized object graph using Coalesce's :csharp:`.IncludedSeparately()` and :csharp:`.ThenIncluded()`.
        
        .. note::

            When :csharp:`GetQuery` is overridden, the :ref:`DefaultLoadingBehavior` is suppressed. To restore this behavior, use the :csharp:`IQueryable<T>.IncludeChildren()` extension method to build your query.

    :csharp:`IsAuthorized`
        Allows for user-level control over whether or not the data source can be used. Use :csharp:`this.User` to get the current user. This method is called by the model binder that is responsible for injecting data sources into controller actions. If a failure result is returned by this method, a model state error will be added (and handled by default by Coalesce's default implementation of :csharp:`IApiActionFilter`), and no data source instance will be made available to the controller action.

    :csharp:`TransformResults`
        Allows for transformation of a result set after the query has been evaluated. 
        This will be called for both lists of items and for single items. This can be used for things like populating non-mapped properties on a model. This method is only called immediately before mapping to a DTO - if the data source is serving data without mapping (e.g. when invoked by :ref:`CustomBehaviors`) to a DTO, this will not be called..

        .. warning::
            
            It is STRONGLY RECOMMENDED that this method does not modify any database-mapped properties, as any such changes could be inadvertently persisted to the database.

    :csharp:`GetIncludeTree`
        Allows for explicitly specifying the :ref:`IncludeTree` that will be used when serializing results obtained from this data source into DTOs. By default, the query that is build up through all the other methods in the data source will be used to build the include tree.

    :csharp:`CanEvalQueryAsynchronously`
        Called by other methods in the standard data source to determine whether or not EF Core async methods will be used to evaluate queries. This may be globally disabled when bugs like https://github.com/aspnet/EntityFrameworkCore/issues/9038 are present in EF Core.

    :csharp:`ApplyListFiltering`
        A simple wrapper that calls :csharp:`ApplyListPropertyFilters` and :csharp:`ApplyListSearchTerm`.

    :csharp:`ApplyListPropertyFilters`
        For each value in :csharp:`parameters.Filter`, invoke :csharp:`ApplyListPropertyFilter` to apply a filter to the query.

    :csharp:`ApplyListPropertyFilter`
        Given a property and a client-provided string value, perform some filtering on that property.
         
            - Dates with a time component will be matched exactly.
            - Dates with no time component will match any dates that fell on that day.
            - Strings will match exactly unless an asterisk is found, in which case they will be matched with :csharp:`string.StartsWith`.
            - Enums will match by string or numeric value. Mutliple comma-delimited values will create a filter that will match on any of the provided values.
            - Numeric values will match exactly. Mutliple comma-delimited values will create a filter that will match on any of the provided values.

    :csharp:`ApplyListSearchTerm`
        Applies filters to the query based on the specified search term. See :ref:`Searching` for a detailed look at how searching works in Coalesce.

    :csharp:`ApplyListSorting`
        If any client-specified sort orders are present, invokes :csharp:`ApplyListClientSpecifiedSorting`. Otherwise, invokes :csharp:`ApplyListDefaultSorting`.

    :csharp:`ApplyListClientSpecifiedSorting`
        Applies sorting to the query based on sort orders specified by the client. If the client specified :code:`"none"` as the sort field, no sorting will take place.
        
    :csharp:`ApplyListDefaultSorting`
        Applies default sorting behavior to the query, including behavior defined with use of :csharp:`[DefaultOrderBy]` in C# POCOs, as well as fallback sorting to :code:`"Name"` or primary key properties.

        .. TODO - need a centralized doc page about sorting in Coalesce.

    :csharp:`ApplyListPaging`
        Applies paging to the query based on incoming parameters. Provides the actual page and pageSize that were used as out parameters.
        
    :csharp:`GetListTotalCountAsync`
        Simple wrapper around invoking :csharp:`.Count()` on a query. 
    


Replacing the Standard Data Source
..................................

You can, of course, create a custom base data source that all your custom implementations inherit from. But, what if you want to override the standard data source across your entire application, so that :csharp:`StandardDataSource<,>` will never be instantiated? You can do that too!

Simply create a class that implements :csharp:`IEntityFrameworkDataSource<,>` (the :csharp:`StandardDataSource<,>` already does - feel free to inherit from it), then register it at application startup like so:


    .. code-block:: c#

        public class MyDataSource<T, TContext> : StandardDataSource<T, TContext>
            where T : class, new()
            where TContext : DbContext
        {
            public MyDataSource(CrudContext<TContext> context) : base(context)
            {
            }

            ...
        }

    .. code-block:: c#

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCoalesce(b =>
            {
                b.AddContext<AppDbContext>();
                b.UseDefaultDataSource(typeof(MyDataSource<,>));
            });

Your custom data source must have the same generic type parameters - :csharp:`<T, TContext>`. Otherwise, the Microsoft.Extensions.DependencyInjection service provider won't know how to inject it.
