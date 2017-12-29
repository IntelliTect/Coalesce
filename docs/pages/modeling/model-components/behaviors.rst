.. _CustomBehaviors:



Behaviors
=========

    *In a CRUD system, creating, updating, and deleting are considered especially different from reading. In Coalesce, the dedicated classes that perform these operations are derivatives of a special interface known as the* :csharp:`IBehaviors<T>`. *These are their stories*.

.. please dont get rid of my law & order copypasta. - andrew

----

Coalesce separates out the parts of your API that read your data from the parts that mutate it. The read portion is performed by :ref:`CustomDataSources`, and the mutations are performed by behaviors. Like data sources, there exists a standard set of behaviors that Coalesce provides out-of-the-box that cover the most common use cases for creating, updating, and deleting objects in your data model.

Also like data sources, these functions can be easily overriden on a per-model basis, allowing complete control over the ways in which your data is mutated by the APIs that Coalesce generates. However, unlike data sources which can have as many implementations per model as you like, you can only have one set of behaviors. The rationale is quite simple: it is acceptable for clients to be able to load your data in different ways for different pages or other usage scenarios, but the client should not be responsible nor be able to choose the underlying mechanisms by which they mutate that data. Such decisions should only ever be the concern of the server, decided upon by examining the incoming request, incoming data, and the state of existing data prior to mutation.


.. contents:: Contents
    :local:

Defining Behaviors
------------------

By default, each of your models that Coalesce exposes will expose the standard behaviors (:csharp:`IntelliTect.Coalesce.StandardBehaviors<T, TContext>`). These behaviors provide the standard set of features one would expect - creating, updating, and deleting from an EF Core :csharp:`DbContext`. Each of the three parts are implemented in one or more virtual methods, making the :csharp:`StandardBehaviors` a great place to start from when implementing your own behaviors. To override the standard behaviors, simple create your own custom behaviors class, outlined below. Unlike data sources which require an annotation to override the standard Coalesce-provided class, the simple presence of an explicitly declared set of behaviors will suppress the standard behaviors.

To create your own behaviors for mutation of a model, you simply need to define a class that implements :csharp:`IntelliTect.Coalesce.IBehaviors<T>`. To expose your behaviors to Coalesce, either place it as a nested class of the type :csharp:`T` that your behaviors are for, or annotate it with the :csharp:`[Coalesce]` attribute. Of course, the easiest way to create behaviors that doesn't require you to re-engineer a great deal of logic would be to inherit from :csharp:`IntelliTect.Coalesce.StandardBehaviors<T, TContext>`, and then override only the parts that you need.

    .. code-block:: c#

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
            public Behaviors(CrudContext<AppDbContext> context) : base(context) { }

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


Dependency Injection
''''''''''''''''''''

All behaviors are instantiated using dependency injection and your application's :csharp:`IServiceProvider`. As a result, you can add whatever constructor parameters you desire to your behaviors as long as a value for them can be resolved from your application's services. The single parameter to the :csharp:`StandardBehaviors` is resolved in this way - the :csharp:`CrudContext<TContext>` contains the common set of objects most commonly used, including the :csharp:`DbContext` and the :csharp:`ClaimsPrincipal` representing the current user.


.. _StandardBehaviors:

Standard Behaviors
------------------

The standard behaviors, :csharp:`IntelliTect.Coalesce.StandardBehaviors<T, TContext>`, contains a significant number of properties and methods that can be utilized and/or overridden at your leisure.

Properties
''''''''''

    :csharp:`CrudContext<TContext> Context`
        The object passed to the constructor that contains the set of objects needed by the standard behaviors, and those that are most likely to be used in custom implementations.
    :csharp:`TContext Db`
        An instance of the db context that contains a :csharp:`DbSet<T>` for the entity handled by the behaviors
    :csharp:`ClaimsPrincipal User`
        The user making the current request.
    :csharp:`IDataSource<T> OverrideFetchForUpdateDataSource`
        A data source that, if set, will override the data source that is used to retrieve the target of an update operation from the database. The incoming values will then be set on this retrieved object. Null by default; override by setting a value in the constructor.
    :csharp:`IDataSource<T> OverrideFetchForDeleteDataSource`
        A data source that, if set, will override the data source that is used to retrieve the target of an delete operation from the database. The retrieved object will then be deleted. Null by default; override by setting a value in the constructor.
    :csharp:`IDataSource<T> OverridePostSaveResultDataSource`
        A data source that, if set, will override the data source that is used to retrieve a newly-created or just-updated object from the database after a save. The retrieved object will be returned to the client. Null by default; override by setting a value in the constructor.

Method Overview
'''''''''''''''

The standard behaviors implementation contains 9 different methods which can be overridden in your derived class to control functionality. 

These methods often call one another, so overriding one method may cause some other method to no longer be called. The hierarchy of method calls, ignoring any logic or conditions contained within, is as follows:

    .. code-block:: c#

        SaveAsync
            DetermineSaveKind
            GetDbSet
            BeforeSave
            AfterSave

        DeleteAsync
            BeforeDelete
            ExecuteDeleteAsync
                GetDbSet
            AfterDelete

Method Details
''''''''''''''

All of the methods outlined above can be overridden. A description of each of the methods is as follows:

    :csharp:`DetermineSaveKind`
        Given the incoming DTO on which Save has been called, examine its properties to determine if the operation is meant to be a create or an update operation. Return this distinction along with the key that was used to make the distinction.

        This method is called outside of the standard data source by the base API controller to perform role-based security on saves at the controller level.

    :csharp:`SaveAsync`
        Save the given item.

    :csharp:`GetDbSet`
        Fetch a :csharp:`DbSet<T>` that items can be added to (creates) or remove from (deletes).
    
    :csharp:`BeforeSave`
        Provides an easy way for derived classes to intercept a save attempt and either reject it by returning an unsuccessful result, or approve it by returning success. The incoming item can also be modified at will in this method to override changes that the client made as desired.    

    :csharp:`AfterSave`
        Provides an easy way for derived classes to perform actions after a save operation has been completed. Failure results returned here will present an error to the client, but will not prevent modifications to the database since changes have already been saved at this point. This method can optionally modify or replace the item that is sent back to the client after a save by setting :csharp:`ref T item` to another object or to null. Setting :csharp:`ref IncludeTree includeTree` will override the :ref:`IncludeTree` used to shape the response object.

        .. warning::

            Setting :csharp:`ref T item` to null will prevent the new object from being returned - be aware that this can be harmful in create scenarios since it prevents the client from recieving the primary key of the newly created item. If autoSave is enabled on the client, this could cause a large number of duplicate objects to be created in the database, since each subsequent save by the client will be treated as a create when the incoming object lacks a primary key.

    :csharp:`DeleteAsync`
        Deletes the given item.

    :csharp:`BeforeDelete`
        Provides an easy way to intercept a delete request and potentially reject it.

    :csharp:`ExecuteDeleteAsync`
        Performs the delete action aginst the database. The implementation of this method removes the item from its corresponding :csharp:`DbSet<T>`, and then calls :csharp:`Db.SaveChangesAsync()`. 

        Overriding this allows for changing this row-deletion implementation to something else, like setting of a soft delete flag, or copying the data into another archival table before deleting.

    :csharp:`AfterDelete`
        Allows for performing any sort of cleanup actions after a delete has completed. This method offers no chance to return feedback to the client, so make sure any necessary feedback is done in :csharp:`BeforeDelete`.



Replacing the Standard Behaviors
................................

You can, of course, create a custom base behaviors class that all your custom implementations inherit from. But, what if you want to override the standard behaviors across your entire application, so that :csharp:`StandardBehaviors<,>` will never be instantiated? You can do that too!

Simply create a class that implements :csharp:`IEntityFrameworkBehaviors<,>` (the :csharp:`StandardBehaviors<,>` already does - feel free to inherit from it), then register it at application startup like so:


    .. code-block:: c#

        public class MyBehaviors<T, TContext> : StandardBehaviors<T, TContext>
            where T : class, new()
            where TContext : DbContext
        {
            public MyMyBehaviors(CrudContext<TContext> context) : base(context)
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
                b.UseDefaultBehaviors(typeof(MyBehaviors<,>));
            });

Your custom behaviors class must have the same generic type parameters - :csharp:`<T, TContext>`. Otherwise, the Microsoft.Extensions.DependencyInjection service provider won't know how to inject it.