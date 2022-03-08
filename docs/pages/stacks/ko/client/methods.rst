
.. _KoModelMethodTypeScript:

TypeScript Method Objects
=========================

For each :ref:`Custom Method <ModelMethods>` you define, a class will be created on the corresponding TypeScript ViewModel (instance methods) or ListViewModel (static methods) that contains the properties and functions for interaction with the method. This class is accessible through a static property named after the method. An instance of this class will also be created on each instance of its parent - this instance is in a property with the camel-cased name of the method.

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
:ts:`public invoke: (name: string, callback: (result: string) => void = null, reload: boolean = true): JQueryPromise<any>`
    Function that takes all the method parameters and a callback. If :ts:`reload` is true, the ViewModel or ListViewModel that owns the method will be reloaded after the call is complete, and only after that happens will the callback be called.

The following members are only generated for methods with arguments:

:ts:`public static Args = class Args { public name: KnockoutObservable<string> = ko.observable(null); }`
    Class with one observable member per method argument for binding method arguments to user input.
:ts:`public args = new Rename.Args()`
    Default instance of the args class.
:ts:`public invokeWithArgs: (args = this.args, callback?: (result: string) => void, reload: boolean = true) => JQueryPromise<any>`
    Function for invoking the method using the args class. The default instance of the args class will be used if none is provided.
:ts:`public invokeWithPrompts: (callback: (result: string) => void = null, reload: boolean = true) => JQueryPromise<any>`
    Simple interface using browser :ts:`prompt()` input boxes to prompt the user for the required data for the method call. The call is then made with the data provided.

The following member is generated for methods that return a file:

:ts:`public resultObjectUrl: KnockoutObservable<string | null>`
    Observable that will contain an `Object URL <https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL>`_ representing the last successful invocation result.

The following member is generated for methods exposed by HTTP GET:

:ts:`public url: KnockoutComputed<string>`
    The URL for the method. Can be useful for using as the `src` attribute of an `image` or `video` HTML element for file-downloading methods. Any arguments will be populated from :ts:`this.args`.

Base Class Members
..................

:ts:`public result: KnockoutObservable<string>`
    Observable that will contain the results of the method call after it is complete.
:ts:`public rawResult: KnockoutObservable<Coalesce.ApiResult>`
    Observable with the raw, deserialized JSON result of the method call. If the method call returns an object, this will contain the deserialized JSON object from the server before it has been loaded into ViewModels and its properties loaded into observables.
:ts:`public isLoading: KnockoutObservable<boolean>`
    Observable boolean which is true while the call to the server is pending.
:ts:`public message: KnockoutObservable<string>`
    If the method was not successful, this contains exception information.
:ts:`public wasSuccessful: KnockoutObservable<boolean>`
    Observable boolean that indicates whether the method call was successful or not.

ListResult<T> Method Members
............................

:ts:`public page: KnockoutObservable<number>`
    Page number of the results.
:ts:`public pageSize: KnockoutObservable<number>`
    Page size of the results.
:ts:`public pageCount: KnockoutObservable<number>`
    Total number of possible result pages.
:ts:`public totalCount: KnockoutObservable<number>`
    Total number of results.

|
