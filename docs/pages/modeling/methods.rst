
.. _ModelMethods:

Methods
=======

Any public methods you place on your POCO classes that are not annotated with the :ref:`InternalUse` attribute will get built into your TypeScript ViewModels and ListViewModels, and API endpoints will be created for these methods to be called. Both instance methods and static methods are supported.

.. contents:: Contents
    :local:


Parameters
----------

The following parameters can be added to your methods:

    Primitives & Dates
        Only primitive values and dates are accepted as parameters to be passed from the client to the method call. Complex objects or collections are not supported at this time.
    :csharp:`<YourDbContext> db`
        If the method has a parameter of the same type as your DbContext class, the current DbContext will be passed to the method.
    :csharp:`ClaimsPrincipal user`
        If the method has a parameter of type ClaimsPrincipal, the current user will be added to the method call automatically. This does not appear on the client side.
    :csharp:`[Inject] <anything>`
        If a parameter is marked with the :ref:`InjectAttribute` attribute, it will be injected from the application's :csharp:`IServiceProvider`.
    :csharp:`out IncludeTree includeTree`
        If the method has an :csharp:`out IncludeTree includeTree` parameter, then the :csharp:`IncludeTree` that is passed out will be used to control serialization. See :ref:`ControllingLoading` and :ref:`IncludeTree` for more information about this.

|
Return Values
-------------

You can return virtually anything from these methods:

    Primitives
        Any primitive data types may be returned - :csharp:`string`, :csharp:`int`, etc.
    Model Types
        Any of the types of your models may be returned. The generated TypeScript for calling the method will use the generated TypeScript ViewModels of your models to store the returned value.
    Custom Types
        Any custom type you define may also be returned from a method. Corresponding TypeScript ViewModels will be created for these types.

        .. warning::
            When returning custom types from methods, be careful of the types of the properties. As Coalesce generates the TypeScript ViewModels for your custom type (:ref:`ExternalTypes`), it will also generate ViewModels for the types of any of its properties, and so on down the tree. If something like :csharp:`System.Threading.Timer`, for example, is encountered as a property, these generated types will get out of hand extremely quickly.

            Mark any properties you don't want generated on these TypeScript ViewModels with the :ref:`InternalUse` attribute.
    :csharp:`IEnumerable<T>`
        Enumerables and collections of any of the valid return types above may be returned. Any derived type of :csharp:`IEnumerable<T>` is valid in the signature, but :csharp:`ICollection<T>` is recommended where possible.
        


|
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
    Shows a modal with HTML ``id="method-Move"`` to prompt the user for the required data for
    the method call. The call is then made with the data provided.
    The generated modal only exists on the generated editor views. If you need it elsewhere, you should copy it from the generated HTML for the editor and place it in your custom page.

|
Instance Methods
----------------

Instance methods can use information contained in the object during the execution of the method. These methods generate the members above on the TypeScript ViewModel.

The model instance that the method is called on will be loaded according to the following rules:

- :ref:`CustomDataSources` are not used - even if one is set, it is not passed to the server in the API call, and will not be considered. If you would like to load additional data using one of your :ref:`CustomDataSources`, you'll need to manually call it inside your method.
- If your model implements :ref:`IIncludable`, the :csharp:`Include` method will be called with a includes string of :csharp:`null`. 
- Otherwise, the model is loaded according to the :ref:`DefaultLoadingBehavior`.

| 
Static Methods
--------------

Static methods are created as functions on the TypeScript ListViewModel.

.. code-block:: c#

    public static IEnumerable<string> NamesStartingWith(string characters, DbContext db)
    {
        return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
    }
