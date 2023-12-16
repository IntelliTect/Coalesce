
# TypeScript Method Objects

For each [Custom Method](/modeling/model-components/methods.md) you define, a class will be created on the corresponding TypeScript ViewModel (instance methods) or ListViewModel (static methods) that contains the properties and functions for interaction with the method. This class is accessible through a static property named after the method. An instance of this class will also be created on each instance of its parent - this instance is in a property with the camel-cased name of the method.

Here's an example for a method called Rename that takes a single parameter 'string name' and returns a string.

``` c#
public string Rename(string name)
{
    FirstName = name;
    return FullName; // Return the new full name of the person.
}
```

## Base Members

The following members are available on the method object for all client methods:

<Prop def="public result: KnockoutObservable<string>" lang="ts" />

Observable that will contain the results of the method call after it is complete.

<Prop def="public rawResult: KnockoutObservable<Coalesce.ApiResult>" lang="ts" />

Observable with the raw, deserialized JSON result of the method call. If the method call returns an object, this will contain the deserialized JSON object from the server before it has been loaded into ViewModels and its properties loaded into observables.

<Prop def="public isLoading: KnockoutObservable<boolean>" lang="ts" />

Observable boolean which is true while the call to the server is pending.

<Prop def="public message: KnockoutObservable<string>" lang="ts" />

If the method was not successful, this contains exception information.

<Prop def="public wasSuccessful: KnockoutObservable<boolean>" lang="ts" />

Observable boolean that indicates whether the method call was successful or not.


## `ListResult<T>` Base Members

For methods that return a `ListResult<T>`, the following additional members on the method object will be available:

<Prop def="public page: KnockoutObservable<number>" lang="ts" />

Page number of the results.

<Prop def="public pageSize: KnockoutObservable<number>" lang="ts" />

Page size of the results.

<Prop def="public pageCount: KnockoutObservable<number>" lang="ts" />

Total number of possible result pages.

<Prop def="public totalCount: KnockoutObservable<number>" lang="ts" />

Total number of results.


## Method-specific Members

<Prop def="public static Rename = class Rename extends Coalesce.ClientMethod<Person, string> { ... }" lang="ts" id="method-object-class-declaration" />

Declaration of the method object class. This will be generated on the parent [ViewModel](./view-model.md) or [ListViewModel](./list-view-model.md).


<Prop def="public readonly rename = new Person.Rename(this)" lang="ts" id="method-object-instance" />

Default instance of the method for easy calling of the method without needing to manually instantiate the class. This will be generated on the parent [ViewModel](./view-model.md) or [ListViewModel](./list-view-model.md).


<Prop def="public invoke: (name: string, callback: (result: string) => void = null, reload: boolean = true): JQueryPromise<any>" lang="ts" />

Function that takes all the method parameters and a callback. If `reload` is true, the ViewModel or ListViewModel that owns the method will be reloaded after the call is complete, and only after that happens will the callback be called.



<Prop def="public static Args = class Args { public name: KnockoutObservable<string> = ko.observable(null); }" lang="ts" id="method-args-class-declaration" />

Class with one observable member per method argument for binding method arguments to user input. Only generated for methods with arguments.


<Prop def="public args = new Rename.Args()" lang="ts" id="method-args-instance" />

Default instance of the args class. Only generated for methods with arguments.

<Prop def="public invokeWithArgs: (args = this.args, callback?: (result: string) => void, reload: boolean = true) => JQueryPromise<any>" lang="ts" />

Function for invoking the method using the args class. The default instance of the args class will be used if none is provided. Only generated for methods with arguments.

<Prop def="public invokeWithPrompts: (callback: (result: string) => void = null, reload: boolean = true) => JQueryPromise<any>" lang="ts" />

Simple interface using browser `prompt()` input boxes to prompt the user for the required data for the method call. The call is then made with the data provided. Only generated for methods with arguments.



<Prop def="public resultObjectUrl: KnockoutObservable<string | null>" lang="ts" />

Observable that will contain an [Object URL](https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL) representing the last successful invocation result. Only generated for [methods that return a file](/modeling/model-components/methods.md#file-downloads).



<Prop def="public url: KnockoutComputed<string>" lang="ts" />

The URL for the method. Can be useful for using as the `src` attribute of an `image` or `video` HTML element for file-downloading methods. Any arguments will be populated from `this.args`. Only generated for HTTP GET methods, as configured by [[ControllerAction]](/modeling/model-components/attributes/controller-action.md).
