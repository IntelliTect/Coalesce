# Behaviors

*In a CRUD system, creating, updating, and deleting are considered especially different from reading. In Coalesce, the dedicated classes that perform these operations are derivatives of a special interface known as the* `IBehaviors<T>`. *These are their stories*.

----

Coalesce separates out the parts of your API that read your data from the parts that mutate it. The read portion is performed by [Data Sources](/modeling/model-components/data-sources.md), and the mutations are performed by behaviors. Like data sources, there exists a standard set of behaviors that Coalesce provides out-of-the-box that cover the most common use cases for creating, updating, and deleting objects in your data model.

Also like data sources, these functions can be easily overridden on a per-model basis, allowing complete control over the ways in which your data is mutated by the APIs that Coalesce generates. However, unlike data sources which can have as many implementations per model as you like, you can only have one set of behaviors.


[[toc]]

## Defining Behaviors

By default, each of your models that Coalesce exposes will utilize the standard behaviors (`IntelliTect.Coalesce.StandardBehaviors<T, TContext>`) for the out-of-the-box API endpoints that Coalesce provides. These behaviors provide a set of create, update, and delete methods for an EF Core `DbContext`, as well as a plethora of virtual methods that make the `StandardBehaviors` a great base class for your custom implementations. Unlike data sources which require an annotation to override the Coalesce-provided standard class, the simple presence of an explicitly declared set of behaviors will suppress the standard behaviors.

::: tip Note
When you define a set of custom behaviors, take note that these are only used by the standard set of API endpoints that Coalesce always provides. They will not be used to handle any mutations in any [Methods](/modeling/model-components/methods.md) you write for your models.
:::

To create your own behaviors, you simply need to define a class that implements `IntelliTect.Coalesce.IBehaviors<T>`. To expose your behaviors to Coalesce, either place it as a nested class of the type `T` that your behaviors are for, or annotate it with the `[Coalesce]` attribute. Of course, the easiest way to create behaviors that doesn't require you to re-engineer a great deal of logic would be to inherit from `IntelliTect.Coalesce.StandardBehaviors<T, TContext>`, and then override only the parts that you need.

``` c#
public class Case
{
    public int CaseId { get; set; }
    public int OwnerId { get; set; }
    public bool IsDeleted { get; set; }
    ...
}

[Coalesce]
public class CaseBehaviors : StandardBehaviors<Case, AppDbContext>
{
    public CaseBehaviors(CrudContext<AppDbContext> context) : base(context) { }

    public override ItemResult BeforeSave(SaveKind kind, Case oldItem, Case item)
    {
        // Allow admins to bypass all validation.
        if (User.IsInRole("Admin")) return true;

        if (kind == SaveKind.Update && oldItem.OwnerId != item.OwnerId)
            return "The owner of a case may not be changed";

        // This is a new item, OR its an existing item and the owner isn't being modified.
        if (item.CreatedById != User.GetUserId())
            return "You are not the owner of this item."

        return true;
    }

    public override ItemResult BeforeDelete(Case item) 
        => User.IsInRole("Manager") ? true : "Unauthorized";

    public override Task ExecuteDeleteAsync(Case item)
    {
        // Soft delete the item.
        item.IsDeleted = true;
        return Db.SaveChangesAsync();
    }
}
```

### Dependency Injection

All behaviors are instantiated using dependency injection and your application's `IServiceProvider`. As a result, you can add whatever constructor parameters you desire to your behaviors as long as a value for them can be resolved from your application's services. The single parameter to the `StandardBehaviors` is resolved in this way - the `CrudContext<TContext>` contains the common set of objects most commonly used, including the `DbContext` and the `ClaimsPrincipal` representing the current user.


## Standard Behaviors

The standard behaviors, `IntelliTect.Coalesce.StandardBehaviors<T>` and its EntityFramework-supporting sibling `IntelliTect.Coalesce.StandardBehaviors<T, TContext>`, contain a significant number of properties and methods that can be utilized and/or overridden at your leisure.

### Properties

<Prop def="CrudContext<TContext> Context" />

The object passed to the constructor that contains the set of objects needed by the standard behaviors, and those that are most likely to be used in custom implementations.

<Prop def="TContext Db" />

An instance of the db context that contains a `DbSet<T>` for the entity handled by the behaviors

<Prop def="ClaimsPrincipal User" />

The user making the current request.

<Prop def="IDataSource<T> OverrideFetchForUpdateDataSource" />

A data source that, if set, will override the data source that is used to retrieve the target of an update operation from the database. The incoming values will then be set on this retrieved object. Null by default; override by setting a value in the constructor.

<Prop def="IDataSource<T> OverridePostSaveResultDataSource" />

A data source that, if set, will override the data source that is used to retrieve a newly-created or just-updated object from the database after a save. The retrieved object will be returned to the client. Null by default; override by setting a value in the constructor.

<Prop def="IDataSource<T> OverrideFetchForDeleteDataSource" />

A data source that, if set, will override the data source that is used to retrieve the target of an delete operation from the database. The retrieved object will then be deleted. Null by default; override by setting a value in the constructor.

<Prop def="IDataSource<T> OverridePostDeleteResultDataSource" />

A data source that, if set, will override the data source that is used to retrieve the target of an delete operation from the database after it has been deleted. If an object is able to be retrieved from this data source, it will be sent back to the client. This allows soft-deleted items to be returned to the client when the user is able to see them. Null by default; override by setting a value in the constructor.

### Method Overview

The standard behaviors implementation contains many different methods which can be overridden in your derived class to control functionality. 

These methods often call one another, so overriding one method may cause some other method to no longer be called. The hierarchy of method calls, ignoring any logic or conditions contained within, is as follows:

``` :no-line-numbers
SaveAsync
    DetermineSaveKindAsync
    GetDbSet
    ValidateDto
    MapIncomingDto
    BeforeSaveAsync
        BeforeSave
    ExecuteSaveAsync
    AfterSave

DeleteAsync
    BeforeDeleteAsync
        BeforeDelete
    ExecuteDeleteAsync
        GetDbSet
    AfterDelete
```

### Method Details(?:^|> )(\w+)(?:<|\(| \{).*$

All of the methods outlined above can be overridden. A description of each of the methods is as follows:


<Prop def="Task<ItemResult<TDto?>> SaveAsync<TDto>(TDto incomingDto, IDataSource<T> dataSource, IDataSourceParameters parameters)" />

Save the given item. This is the main entry point for saving, and takes a DTO as a parameter. This method is responsible for performing mapping to your EF models and ultimately saving to your database. If it is required that you access properties from the incoming DTO in this method, a set of extension methods `GetValue` and `GetObject` are available on the DTO for accessing properties that are mapped 1:1 with your EF models.


<Prop def="Task<(SaveKind Kind, object? IncomingKey)> DetermineSaveKindAsync<TDto>(TDto incomingDto, IDataSource<T> dataSource, IDataSourceParameters parameters)" />

Given the incoming DTO on which Save has been called, examine its properties to determine if the operation is meant to be a create or an update operation. Return this distinction along with the key that was used to make the distinction.

This method is called outside of the standard data source by the base API controller to perform role-based security on saves at the controller level.


<Prop def="DbSet<T> GetDbSet()" />

Returns a `DbSet<T>` that items can be added to (creates) or remove from (deletes).


<Prop def="ItemResult ValidateDto(SaveKind kind, IClassDto<T> dto)" />

Provides a chance to validate the properties of the DTO object itself, as opposed to the properties of the model after the DTO has been mapped to it in `BeforeSave`. A number of extension methods on `IClassDto<T>` can be used to access the value of the properties of [Generated C# DTOs](/stacks/agnostic/dtos.md). For behaviors on [Custom DTOs](/modeling/model-types/dtos.md) where the DTO type is known, simply cast to the correct type. 


<Prop def="void MapIncomingDto<TDto>(SaveKind kind, T item, TDto dto, IDataSourceParameters parameters)" />

Map the properties of the incoming DTO to the model that will be saved to the database. By default, this will call the `MapTo` method on the DTO, but if more precise control is needed, the `IClassDto<T>` extension methods or a cast to a known type can be used to get specific values. If all else fails, the DTO can be reflected upon.


<Prop def="Task<ItemResult> BeforeSaveAsync(SaveKind kind, T? oldItem, T item);
ItemResult BeforeSave(SaveKind kind, T? oldItem, T item)" />

Provides an easy way for derived classes to intercept a save attempt and either reject it by returning an unsuccessful result, or approve it by returning success. The incoming item can also be modified at will in this method to override changes that the client made as desired.    


<Prop def="ItemResult AfterSave(SaveKind kind, T? oldItem, ref T item, ref IncludeTree? includeTree)" />

Provides an easy way for derived classes to perform actions after a save operation has been completed. Failure results returned here will present an error to the client, but will not prevent modifications to the database since changes have already been saved at this point. This method can optionally modify or replace the item that is sent back to the client after a save by setting `ref T item` to another object or to null. Setting `ref IncludeTree includeTree` will override the [Include Tree](/concepts/include-tree.md) used to shape the response object.

::: warning
Setting `ref T item` to null will prevent the new object from being returned - be aware that this can be harmful in create scenarios since it prevents the client from receiving the primary key of the newly created item. If autoSave is enabled on the client, this could cause a large number of duplicate objects to be created in the database, since each subsequent save by the client will be treated as a create when the incoming object lacks a primary key.
:::


<Prop def="Task<ItemResult<TDto?>> DeleteAsync<TDto>(object id, IDataSource<T> dataSource, IDataSourceParameters parameters)" />

Deletes the given item.


<Prop def="Task<ItemResult> BeforeDeleteAsync(T item);
ItemResult BeforeDelete(T item)" />

Provides an easy way to intercept a delete request and potentially reject it (by returning a non-success ItemResult).


<Prop def="Task ExecuteDeleteAsync(T item)" />

Performs the delete action against the database. The implementation of this method removes the item from its corresponding `DbSet<T>`, and then calls `Db.SaveChangesAsync()`. 

Overriding this allows for changing this row-deletion implementation to something else, like setting of a soft delete flag, or copying the data into another archival table before deleting.


<Prop def="void AfterDelete(ref T item, ref IncludeTree? includeTree)" />

Allows for performing any sort of cleanup actions after a delete has completed. If the item was still able to be retrieved from the database after the delete operation completed, this method allows lets you modify or replace the item that is sent back to the client by setting `ref T item` to another object or to null. Setting `ref IncludeTree includeTree` will override the [Include Tree](/concepts/include-tree.md) used to shape the response object.



## Globally Replacing the Standard Behaviors

You can, of course, create a custom base behaviors class that all your custom implementations inherit from. But, what if you want to override the standard behaviors across your entire application, so that `StandardBehaviors<,>` will never be instantiated? You can do that too!

Simply create a class that implements `IEntityFrameworkBehaviors<,>` (the `StandardBehaviors<,>` already does - feel free to inherit from it), then register it at application startup like so:


``` c#
public class MyBehaviors<T, TContext> : StandardBehaviors<T, TContext>
    where T : class, new()
    where TContext : DbContext
{
    public MyBehaviors(CrudContext<TContext> context) : base(context)
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
        b.UseDefaultBehaviors(typeof(MyBehaviors<,>));
    });
```

Your custom behaviors class must have the same generic type parameters - `<T, TContext>`. Otherwise, the Microsoft.Extensions.DependencyInjection service provider won't know how to inject it.
