
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
    :csharp:`<YourDbContext> db`
        If the method has a parameter of the same type as your DbContext class, the current DbContext will be passed to the method call. For :ref:`Services` which don't have a defined backing EF context, this is treated as having an implicit :csharp:`[Inject]` attribute.
    :csharp:`ClaimsPrincipal user`
        If the method has a parameter of type ClaimsPrincipal, the current user will be passed to the method call.
    :csharp:`[Inject] <anything>`
        If a parameter is marked with the :ref:`InjectAttribute` attribute, it will be injected from the application's :csharp:`IServiceProvider`.
    :csharp:`out IncludeTree includeTree`
        If the method has an :csharp:`out IncludeTree includeTree` parameter, then the :csharp:`IncludeTree` that is passed out will be used to control serialization. See :ref:`GenDTOs` and :ref:`IncludeTree` for more information. If the method returns an :csharp:`IQueryable`, this will supercede the include tree obtained from inspecting the query.

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

        A special object in TypeScript will be used to hold the paging information included in the ListResult.


|

Security
--------

You can implement role-based security on a method by placing the :ref:`ExecuteAttribute` on the method. Placing this attribute on the method with no roles specified will simply require that the calling user be authenticated. 

Security for instance methods is also controlled by the data source that loads the instance - if the data source can't provide an instance of the requested model, the method won't be executed.

.. _ModelMethodTypeScript:

Generated TypeScript
--------------------

For each method you define, a class will be created on the corresponding TypeScript ViewModel (instance methods) or ListViewModel (static methods) that contains the properties and functions for interaction with the method. This class is accessible through a static property named after the method. An instance of this class will also be created on each instance of its parent - this instance is in a property with the camel-cased name of the method.

Here's an example for a method called Rename that takes a single parameter 'string name' and returns a string.

.. code-block:: c#

        public string Rename(string name)
        {
            FirstName = name;
            return FullName; // Return the new full name of the person.
        }

Method-specific Members
.......................

:ts:`public static Rename = class Rename extends Coalesce.ClientMethod<Person, string> { ... }`
    Declaration of class that provides invocation methods and status properties for the method.
:ts:`public readonly rename = new Person.Rename(this)`
    Default instance of the method for easy calling of the method without needing to manually instantiate the class.
:ts:`rename.invoke: (name: string, callback: (result: string) => void = null, reload: boolean = true): JQueryPromise<any>`
    Function that takes all the method parameters and a callback. If :ts:`reload` is true, the ViewModel or ListViewModel that owns the method will be reloaded after the call is complete, and only after that happens will the callback be called.

The following members are only generated for methods with arguments:

:ts:`Rename.Args = class Args { public name: KnockoutObservable<string> = ko.observable(null); }`
    Class with one observable member per method argument for binding method arguments to user input.
:ts:`rename.args = new Rename.Args()`
    Default instance of the args class.
:ts:`rename.invokeWithArgs: (args = this.args, callback?: (result: string) => void, reload: boolean = true) => JQueryPromise<any>`
    Function for invoking the method using the args class. The default instance of the args class will be used if none is provided.
:ts:`rename.invokeWithPrompts: (callback: (result: string) => void = null, reload: boolean = true) => JQueryPromise<any>`
    Simple interface using browser :ts:`prompt()` input boxes to prompt the user for the required data for the method call. The call is then made with the data provided.

Base Class Members
..................

:ts:`rename.result: KnockoutObservable<string>`
    Observable that will contain the results of the method call after it is complete.
:ts:`rename.rawResult: KnockoutObservable<any>`
    Observable with the raw, deserialized JSON result of the method call. If the method call returns an object, this will contain the deserialized JSON object from the server before it has been loaded into ViewModels and its properties loaded into observables.
:ts:`rename.isLoading: KnockoutObservable<boolean>`
    Observable boolean which is true while the call to the server is pending.
:ts:`rename.message: KnockoutObservable<string>`
    If the method was not successful, this contains exception information.
:ts:`rename.wasSuccessful: KnockoutObservable<boolean>`
    Observable boolean that indicates whether the method call was successful or not.

|

Instance Methods
----------------

Instance methods generate the members above on the TypeScript ViewModel.

The instance of the model will be loaded using the data source specified by an attribute :csharp:`[LoadFromDataSource(typeof(MyDataSource))]` if present. Otherwise, the model instance will be loaded using the default data source for the POCO's type. If you have a :ref:`Custom Data Source <CustomDataSources>` annotated with :csharp:`[DefaultDataSource]`, that data source will be used. Otherwise, the :ref:`StandardDataSource` will be used.

| 

Static Methods
--------------

Static methods are created as functions on the TypeScript ListViewModel. All of the same members that are generated for instance methods are also generated for static methods.

If a static method returns the type that it is declared on, it will also be generated on the TypeScript ViewModel of its class.

.. code-block:: c#

    public static ICollection<string> NamesStartingWith(string characters, AppDbContext db)
    {
        return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
    }

| 

Method Annotations
------------------

Methods can be annotated with attributes to control API exposure and TypeScript generation. The following attributes are available for model methods. General annotations can be found on the :ref:`Annotations` page.

    :csharp:`[Coalesce]`
        The :ref:`CoalesceAttribute` attribute causes the method to be exposed via a generated API controller. This is not needed for methods defined on an interface marked with :csharp:`[Service]` - Coalesce assumes that all methods on the interface are intended to be exposed. If this is not desired, create a new, more restricted interface with only the desired methods to be exposed.

    :csharp:`[ApiActionHttpMethod(HttpMethod method)]`
        The :ref:`ApiActionHttpMethod` attribute controls how this method is exposed via HTTP. By default all controller method actions use the POST HTTP method. This behavior can be overridden with this attribute to use GET, POST, PUT, DELETE, or PATCH HTTP methods. Keep in mind that when using the GET method, all parameters are sent as part of the URL, so the typical considerations with sensitive data in a query string applies.

    :csharp:`[Execute(string roles)]`
        The :ref:`ExecuteAttribute` attribute specifies which roles can execute this method from the generated API controller.

    :csharp:`[Hidden(Areas area)]`
        The :ref:`HiddenAttribute` attribute allows for hiding this method on the admin pages both for list/card views and the editor.
           
    :csharp:`[LoadFromDataSource(Type dataSourceType)]`
        The :ref:`LoadFromDataSourceAttribute` attribute specifies that the targeted model instance method should load the instance it is called on from the specified data source when invoked from an API endpoint. By default, whatever the default data source for the model's type will be used.
    
        
        
       

