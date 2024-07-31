# [DtoIncludes] & [DtoExcludes]

`IntelliTect.Coalesce.DataAnnotations.DtoIncludesAttribute`
<br>
`IntelliTect.Coalesce.DataAnnotations.DtoExcludesAttribute`

Allows for easily controlling what data gets set to the client. When requesting data from the generated client-side list view models, you can specify an `includes` property on the ViewModel or ListViewModel. 

For more information about the includes string, see [Includes String](/concepts/includes.md).

When the database entries are returned to the client they will be trimmed based on the requested includes string and the values in `DtoExcludes` and `DtoIncludes`.

::: danger
These attributes are **not security attributes** - consumers of your application's API can set the includes string to any value when making a request.

Do not use them to keep certain data private - use the [Security Attributes](/modeling/model-components/attributes/security-attribute.md) family of attributes for that.
:::   

It is important to note that the value of the includes string will match against these attributes on *any* of your models that appears in the object graph being mapped to DTOs - it is not limited only to the model type of the root object.

::: tip Important
`DtoIncludes` does not ensure that specific data will be loaded from the database. It only *permits* what is *already* loaded into the current EF DbContext to be returned from the API. See [Data Sources](/modeling/model-components/data-sources.md) to learn how to control what data gets loaded from the database.
:::

## Example Usage

Server code:

``` c#
public class Person
{
    // Don't include CreatedBy when editing - will be included for all other views
    [DtoExcludes("Editor")]
    public AppUser CreatedBy { get; set; }

    // Only include the Person's Department when `includes == "details"` on the TypeScript ViewModel.
    [DtoIncludes("details")]
    public Department Department { get; set; }

    // LastName will be included in all views
    public string LastName { get; set; }
}

public class Department
{
    [DtoIncludes("details")]
    public ICollection<Person> People { get; set; }
}
```

Client code:

<CodeTabs>
<template #vue>

``` ts
import { PersonListViewModel } from '@/viewmodels.g'

const personList = new PersonListViewModel();
personList.$includes = "Editor";
await personList.$load();
// Objects in personList.$items will not contain CreatedBy nor Department objects.

const personList2 = new PersonListViewModel();
personList2.$includes = "details";
await personList.$load();
// Objects in personList2.items will be allowed to contain both CreatedBy and Department objects. 
// Department will be allowed to include its other Person objects.
```

</template>
</CodeTabs>


## Properties

<Prop def="public string ContentViews { get; set; }" ctor=1 /> 

A comma-delimited list of values of [`includes`](/concepts/includes.md) on which to operate.

For `DtoIncludes`, this will be the values of `includes` for which this property will be **allowed** to be serialized and sent to the client.

For `DtoExcludes`, this will be the values of `includes` for which this property will **not** be serialized and sent to the client.

