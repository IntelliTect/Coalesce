---
deprecated: true
---

# [TypeScriptPartial]

::: tip Note
This attribute only applies to the Knockout front-end stack. It is not applicable to the Vue stack.
:::

If defined on a model, a typescript file will be generated in
./Scripts/Partials if one does not already exist. This 'Partial' TypeScript file contains a class which inherits from the generated TypeScript ViewModel. The partial class has the same name as the generated ViewModel would normally have, and the generated ViewModel is renamed to ``"<ClassName>Partial"``.

This behavior allows you to extend the behavior of the generated TypeScript view models with your own properties and methods for defining more advanced behavior on the client. One of the most common use cases of this is to define additional Knockout `ComputedObservable` properties for information that is only useful in the browser - for example, computing a css class based on data in the object.

## Example Usage

``` c#
[TypeScriptPartial]
public class Employee
{
    public int EmployeeId { get; set; }

    ...
}
```

## Properties

<Prop def="public string BaseClassName { get; set; }" />

If set, overrides the name of the generated ViewModel which becomes the base class for the generated 'Partial' TypeScript file.