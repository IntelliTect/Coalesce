
.. _ModelMethods:

Methods
=======

Any public methods you place on your POCO classes that are annotated with the :ref:`CoalesceAttribute` will get built into your TypeScript ViewModels and ListViewModels, and API endpoints will be created for these methods to be called. Both instance methods and static methods are supported. Additionally, any instance methods on :ref:`Services` will also have API endpoints and TypeScript generated.

.. contents:: Contents
    :local:


Parameters
----------

The following parameters can be added to your methods:

Primitives & Dates
    Primitive values (numerics, strings, booleans, enums) and dates (:csharp:`DateTime`, :csharp:`DateTimeOffset`, and nullable variants) are accepted as parameters to be passed from the client to the method call. 

Objects
    Any object types may be passed to the method call. These may be existing :ref:`EntityModels` or :ref:`ExternalTypes`. When invoking the method on the client, the object's properties will only be serialized one level deep. If an object parameter has additional child object properties, they will not be included in the invocation of the method - only the object's primitive & date properties will be deserialized from the client.

Files
    Methods can accept file uploads by using a parameter of type :csharp:`IntelliTect.Coalesce.Models.IFile` (or any derived type, like :csharp:`IntelliTect.Coalesce.Models.File`).

:csharp:`<YourDbContext> db`
    If the method has a parameter of the same type as your DbContext class, the current DbContext will be passed to the method call. For :ref:`Services` which don't have a defined backing EF context, this is treated as having an implicit :csharp:`[Inject]` attribute.

:csharp:`ClaimsPrincipal user`
    If the method has a parameter of type ClaimsPrincipal, the current user will be passed to the method call.

:csharp:`[Inject] <anything>`
    If a parameter is marked with the :ref:`InjectAttribute` attribute, it will be injected from the application's :csharp:`IServiceProvider`.

:csharp:`out IncludeTree includeTree`
    If the method has an :csharp:`out IncludeTree includeTree` parameter, then the :csharp:`IncludeTree` that is passed out will be used to control serialization. See :ref:`GenDTOs` and :ref:`IncludeTree` for more information. If the method returns an :csharp:`IQueryable`, the out parameter will supersede the include tree obtained from inspecting the query.

|

Return Values
-------------

You can return virtually anything from these methods:

Primitives & Dates
    Any primitive data types may be returned - :csharp:`string`, :csharp:`int`, etc.

Model Types
    Any of the types of your models may be returned. The generated TypeScript for calling the method will use the generated TypeScript ViewModels of your models to store the returned value.

    If the return type is the same as the type that the method is defined on, and the method is not static, then the results of the method call will be loaded into the calling TypeScript object.

Custom Types
    Any custom type you define may also be returned from a method. Corresponding TypeScript ViewModels will be created for these types. See :ref:`ExternalTypes`.

    .. warning::
        When returning custom types from methods, be careful of the types of their properties. As Coalesce generates the TypeScript ViewModels for your :ref:`ExternalTypes`, it will also generate ViewModels for the types of any of its properties, and so on down the tree. If a type is encountered from the FCL/BCL or another package that your application uses, these generated types will get out of hand extremely quickly.

        Mark any properties you don't want generated on these TypeScript ViewModels with the :ref:`InternalUse` attribute, or give them a non-public access modifier. Whenever possible, don't return types that you don't own or control.

:csharp:`ICollection<T>` or :csharp:`IEnumerable<T>`
    Collections of any of the above valid return types above are also valid return types. IEnumerables are useful for generator functions using :csharp:`yield`. :csharp:`ICollection` is highly suggested over :csharp:`IEnumerable` whenever appropriate, though.

:csharp:`IQueryable<T>`
    Queryables of the valid return types above are valid return types. The query will be evaluated, and Coalesce will attempt to pull an :ref:`IncludeTree` from the queryable to shape the response. When :ref:`IncludeTree` functionality is needed to shape the response but an :csharp:`IQueryable<>` return type is not feasible, an :csharp:`out IncludeTree includeTree` parameter will do the trick as well.

:csharp:`IntelliTect.Coalesce.Models.ItemResult<T>` or :csharp:`ItemResult`
    An :csharp:`ItemResult<T>` of any of the valid return types above, including collections, is valid. The :csharp:`WasSuccessful` and :csharp:`Message` properties on the result object will be sent along to the client to indicate success or failure of the method. The type :csharp:`T` will be mapped to the appropriate DTO object before being serialized as normal.

:csharp:`IntelliTect.Coalesce.Models.ListResult<T>`
    A :csharp:`ListResult<T>` of any of the non-collection types above, is valid. The :csharp:`WasSuccessful` :csharp:`Message`, and all paging information on the result object will be sent along to the client. The type :csharp:`T` will be mapped to the appropriate DTO objects before being serialized as normal.

    The class created for the method in TypeScript will be used to hold the paging information included in the ListResult. See below for more information about this class.


*Downloading files from custom methods is currently unsupported. Please open a feature request on* GitHub_ *if this would be useful for you.*

|

Security
--------

You can implement role-based security on a method by placing the :ref:`ExecuteAttribute` on the method. Placing this attribute on the method with no roles specified will simply require that the calling user be authenticated. 

Security for instance methods is also controlled by the data source that loads the instance - if the data source can't provide an instance of the requested model, the method won't be executed.

Generated TypeScript
--------------------

See :ref:`VueApiCallers` and :ref:`VueViewModels` (Vue) or :ref:`KoModelMethodTypeScript` (Knockout) for details on the code that is generated for your custom methods.

|

Instance Methods
----------------

The instance of the model will be loaded using the data source specified by an attribute :csharp:`[LoadFromDataSource(typeof(MyDataSource))]` if present. Otherwise, the model instance will be loaded using the default data source for the POCO's type. If you have a :ref:`Custom Data Source <DataSources>` annotated with :csharp:`[DefaultDataSource]`, that data source will be used. Otherwise, the :ref:`StandardDataSource` will be used.

Instance methods are generated onto the TypeScript ViewModels.

| 

Static Methods
--------------

Static methods are generated onto the TypeScript ListViewModels. All of the same members that are generated for instance methods are also generated for static methods.

If a static method returns the type that it is declared on, it will also be generated on the TypeScript ViewModel of its class (Knockout only).

.. code-block:: c#

    public static ICollection<string> NamesStartingWith(string characters, AppDbContext db)
    {
        return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
    }

| 

Method Annotations
------------------

Methods can be annotated with attributes to control API exposure and TypeScript generation. The following attributes are available for model methods. General annotations can be found on the :ref:`ModelAttributes` page.

:csharp:`[Coalesce]`
    The :ref:`CoalesceAttribute` attribute causes the method to be exposed via a generated API controller. This is not needed for methods defined on an interface marked with :csharp:`[Service]` - Coalesce assumes that all methods on the interface are intended to be exposed. If this is not desired, create a new, more restricted interface with only the desired methods to be exposed.

:csharp:`[ControllerAction(Method = HttpMethod)]`
    The :ref:`ControllerActionAttribute` attribute controls how this method is exposed via HTTP. By default all controller method actions use the POST HTTP method. This behavior can be overridden with this attribute to use GET, POST, PUT, DELETE, or PATCH HTTP methods. Keep in mind that when using the GET method, all parameters are sent as part of the URL, so the typical considerations with sensitive data in a query string applies.

:csharp:`[Execute(string roles)]`
    The :ref:`ExecuteAttribute` attribute specifies which roles can execute this method from the generated API controller.

:csharp:`[Hidden(Areas area)]`
    The :ref:`HiddenAttribute` attribute allows for hiding this method on the admin pages both for list/card views and the editor.
        
:csharp:`[LoadFromDataSource(Type dataSourceType)]`
    The :ref:`LoadFromDataSourceAttribute` attribute specifies that the targeted model instance method should load the instance it is called on from the specified data source when invoked from an API endpoint. By default, whatever the default data source for the model's type will be used.
    
        
        
       

