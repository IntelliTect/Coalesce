
.. _ModelMethods:

Methods
=======

Any public methods you place on your POCO classes that are annotated with the :ref:`CoalesceAttribute` will get built into your TypeScript ViewModels and ListViewModels, and API endpoints will be created for these methods to be called. Both instance methods and static methods are supported.

.. contents:: Contents
    :local:


Parameters
----------

The following parameters can be added to your methods:

    Primitives & Dates
        Only primitive values and dates are accepted as parameters to be passed from the client to the method call. Complex objects or collections are not supported at this time.
    :csharp:`<YourDbContext> db`
        If the method has a parameter of the same type as your DbContext class, the current DbContext will be passed to the method call.
    :csharp:`ClaimsPrincipal user`
        If the method has a parameter of type ClaimsPrincipal, the current user will be passed to the method call.
    :csharp:`[Inject] <anything>`
        If a parameter is marked with the :ref:`InjectAttribute` attribute, it will be injected from the application's :csharp:`IServiceProvider`.
    :csharp:`out IncludeTree includeTree`
        If the method has an :csharp:`out IncludeTree includeTree` parameter, then the :csharp:`IncludeTree` that is passed out will be used to control serialization. See :ref:`GenDTOs` and :ref:`IncludeTree` for more information.

|

Return Values
-------------

You can return virtually anything from these methods:

    Primitives
        Any primitive data types may be returned - :csharp:`string`, :csharp:`int`, etc.
    Model Types
        Any of the types of your models may be returned. The generated TypeScript for calling the method will use the generated TypeScript ViewModels of your models to store the returned value.

        If the return type is the same as the type that the method is defined on, and the method is not static, then the results of the method call will be loaded into the calling TypeScript object.
    Custom Types
        Any custom type you define may also be returned from a method. Corresponding TypeScript ViewModels will be created for these types. See :ref:`ExternalTypes`.

        .. warning::
            When returning custom types from methods, be careful of the types of their properties. As Coalesce generates the TypeScript ViewModels for your :ref:`ExternalTypes`, it will also generate ViewModels for the types of any of its properties, and so on down the tree. If a type is encountered from the FCL/BCL or another package that your application uses, these generated types will get out of hand extremely quickly.

            Mark any properties you don't want generated on these TypeScript ViewModels with the :ref:`InternalUse` attribute, or give them a non-public access modifier. Whenever possible, don't return types that you don't own or control.
    :csharp:`ICollection<T>`
        Collections of any of the valid return types above are also valid return types.
        


|

Security
--------

You can implement role-based security on a method by placing the :ref:`ExecuteAttribute` on the method. Placing this attribute on the method with no roles specified will simply require that the calling user be authenticated. 

Security for instance methods is also controlled by the data source that loads the instance - if the data source can't provide an instance of the requested model, the method won't be executed.

.. _ModelMethodTypeScript:

Generated TypeScript
--------------------

For each method you define, a number of members will be created on the corresponding TypeScript ViewModel (instance methods) or ListViewModel (static methods). If there are any parameters on the method, an class with the type of :ts:`<MethodName>Args` will be created, and the ViewModel or ListViewModel will have a property for this class that can be easily bound to.

Here's an example for a method called Move that takes a single parameter 'int feet' and returns a string.

.. code-block:: c#

        public string Move(int feet)
        {
            return "I moved " + feet.ToString();
        }

:ts:`public move: (feet: number, callback: () => void = null, reload: boolean = true) => JQueryPromise<any>`
    Function that takes all the method parameters and a callback. If :ts:`reload` is true, the ViewModel or ListViewModel that owns the method will be reloaded after the call is complete, and only after that happens will the callback be called.
:ts:`public moveArgs: Person.MoveArgs`
    Instance of a generated class that contains observable fields for each parameter that the method takes.
:ts:`public moveWithArgs: (args?: Person.MoveArgs, callback: () => void = null, reload: boolean = true) => JQueryPromise<any>`
    Function that takes an object that contains all the parameters.
    Object is of type [Name]Args which is included as a nested class on the ViewModel.
    If null, the built in instance of this class named [name]Args will be used.
:ts:`public moveResult: KnockoutObservable<string>`
    Observable that will contain the results of the method call after it is complete.
:ts:`public moveResultRaw: KnockoutObservable<any>`
    Observable with the raw, deserialized JSON result of the method call. If the method call returns an object, this will contain the deserialized JSON object from the server before it has been loaded into ViewModels and its properties loaded into observables.
:ts:`public moveIsLoading: KnockoutObservable<boolean>`
    Observable boolean which is true while the call to the server is pending.
:ts:`public moveMessage: KnockoutObservable<string>`
    If the method was not successful, this contains exception information.
:ts:`public moveWasSuccessful: KnockoutObservable<boolean>`
    Observable boolean that indicates whether the method call was successful or not.
:ts:`public moveUi: (callback: () => void = null, reload: boolean = true) => JQueryPromise<any>`
    Simple interface using JavaScript input boxes to prompt the user for
    the required data for the method call. The call is then made with
    the data provided.
:ts:`public moveModal: (callback: () => void = null, reload: boolean = true) => void`
    Shows a Bootstrap modal with HTML ``id="method-Move"`` to prompt the user for the required data for the method call. The call is then made with the data provided.
    The generated modal only exists on the generated editor views. If you need it elsewhere, you should copy it from the generated HTML for the editor and place it in your custom page.

|

Instance Methods
----------------

Instance methods generate the members above on the TypeScript ViewModel.

The instance of the model will be loaded using the data source specified by an attribute :csharp:`[LoadFromDataSource(typeof(MyDataSource))]` if present. Otherwise, the model instance will be loaded using the default data source for the POCO's type. If you have a :ref:`Custom Data Source <CustomDataSources>` annotated with :csharp:`[DefaultDataSource]`, that data source will be used. Otherwise, the :ref:`StandardDataSource` will be used.

| 

Static Methods
--------------

Static methods are created as functions on the TypeScript ListViewModel. All of the same members that are generated for instance methods are also generated for static methods.

.. code-block:: c#

    public static ICollection<string> NamesStartingWith(string characters, AppDbContext db)
    {
        return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
    }

| 

Method Annotations
------------------

Methods can be annotated with attributes to control API exposure and TypeScript generation. The following attributes are available for model methods. General annotations can be found on the :ref:`Annotations` page.

    :csharp:`[Coalesce()]`
        The :ref:`CoalesceAttribute` attribute causes the method to be exposed via a generated API controller.

    :csharp:`[ApiActionHttpMethod(HttpMethod method)]`
        The :ref:`ApiActionHttpMethod` attribute controls how this method is exposed via HTTP. By default all controller method actions use the POST HTTP method. This behavior can be overridden with this attribute to use GET, POST, PUT, DELETE, or PATCH HTTP methods. Note that when using the GET method, all parameters are sent as part of the URL and are as clear text regardless of encryption.

    :csharp:`[Execute(string roles)]`
        The :ref:`ExecuteAttribute` attribute specifies which roles can execute this method from the generated API controller.

    :csharp:`[Hidden(Areas area)]`
        The :ref:`HiddenAttribute` attribute allows for hiding this method on the admin pages both for list/card views and the editor.
           
    :csharp:`[LoadFromDataSource(Type dataSourceType)]`
        The :ref:`LoadFromDataSourceAttribute` attribute specifies that the targeted model instance method should load the instance it is called on from the specified data source when invoked from an API endpoint. By default, whatever the default data source for the model's type will be used.
    
        
        
       

