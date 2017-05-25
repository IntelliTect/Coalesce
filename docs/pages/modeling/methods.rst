
Methods
=======

Any public methods you place on your POCO classes that are not annotated with the :ref:`InternalUse` attribute will get built into your TypeScript ViewModels and ListViewModels, and API endpoints will be created for these methods to be called. Both instance methods and static methods are supported.

Parameters
----------

The following additional parameters can be added to your methods:

    :csharp:`<YourDbContext> db`
        If the method has a parameter of the same type as your DbContext class, the current DbContext will be passed to the method.
    :csharp:`ClaimsPrincipal user`
        If the method has a parameter of type ClaimsPrincipal, the current user will be added to the method call automatically. This does not appear on the client side.
    :csharp:`[Inject] <anything>`
        If a parameter is marked with the :ref:`InjectAttribute` attribute, it will be injected from the application's :csharp:`IServiceProvider`.
    :csharp:`out IncludeTree includeTree`
        If the method has an :csharp:`out IncludeTree includeTree` parameter, then the :csharp:`IncludeTree` that is passed out will be used to control serialization. See :ref:`ControllingLoading` and :ref:`IncludeTree` for more information about this.


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
        



Generated TypeScript
--------------------

For each method you define, a number of members will be created on the corresponding TypeScript ViewModel (instance methods) or ListViewModel (static methods). If there are any parameters on the method, an class with the type of :ts:`<MethodName>Args` will be created, and the ViewModel or ListViewModel will have a property for this class that can be easily bound to.

Here's an example for a method called Move that takes a single parameter 'int feet' and returns a string.

.. code-block:: c#

        public string Move(int feet)
        {
            return "I moved " + feet.ToString();
        }

:ts:`public move: (<method parameters>, callback: () => void = null, reload: boolean = false)`
    Function that takes a number and a callback. This callback function
    is called when the call is complete.
:ts:`moveWithArgs`
    Function that takes an object that contains all the parameters.
    Object is of type [Name]Args which is included as a sub-class on
    this class. If null, the built in instance of this class will be
    used. This is named [name]Args
:ts:`moveResult`
    Observable with the result of the method call. This can be data
    bound to show the result.
:ts:`moveResultRaw`
    Observable with the result of the method call. This can be data
    bound to show the result.
:ts:`moveIsLoading`
    Observable boolean which is true while the call to the server is
    happening.
:ts:`moveMessage`
    If the method was not successful, this contains exception
    information.
:ts:`moveWasSuccessful`
    Observable boolean with true if the method was successful, or false
    if an exception occurred.
:ts:`moveUi`
    Simple interface using JavaScript input boxes to prompt the user for
    the required data for the method call. The call is then made with
    the data provided.
:ts:`moveModal`
    Simple modal interface to prompt the user for the required data for
    the method call. The call is then made with the data provided.


Instance Methods
----------------

Instance methods can use information contained in the object during the execution of the method. These methods are created as functions on the object's TypeScript ViewModel. 

The model instance that the method is called on will be loaded according to the following rules:

- :ref:`CustomDataSources` are not used - even if one is set, it is not passed to the server in the API call, and will not be considered. If you would like to load additional data using one of your :ref:`CustomDataSources`, you'll need to manually call it inside your method.
- If your model implements :ref:`IIncludable`, the :csharp:`Include` method will be called with a includes string of :csharp:`null`. 
- Otherwise, the model is loaded according to the :ref:`Default Loading Behavior`.

| 

Static Methods
--------------

Static methods exist on a class without respect to an instance of that
class. These methods are created as functions on the object's **list**
view model on the client side. The example below is for a method called
NameStartingWith that takes a single parameter 'string characters' and
returns a list of strings. The DbContext parameter is injected
automatically by the controller.

::

        public static IEnumerable<string> NamesStartingWith(string characters, DbContext db)
        {
            return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
        }

namesStartingWith
    Function that takes a string and a callback. This callback function
    is called when the call is complete.
namesStartingWithWithArgs
    Function that takes an object that contains all the parameters.
    Object is of type [Name]Args which is included as a sub-class on
    this class. If null, the built in instance of this class will be
    used. This is named [name]Args
namesStartingWithResult
    Observable with the result of the method call. This can be data
    bound to show the result.
namesStartingWithIsLoading
    Observable boolean which is true while the call to the server is
    happening.
namesStartingWithMessage
    If the method was not successful, this contains exception
    information.
namesStartingWithWasSuccessful
    Observable boolean with true if the method was successful, or false
    if an exception occurred.
namesStartingWithUi
    Simple interface to prompt the user for the required data for the
    method call. The call is then made with the data provided.
namesStartingWithModal
    Simple modal interface to prompt the user for the required data for
    the method call. The call is then made with the data provided.
