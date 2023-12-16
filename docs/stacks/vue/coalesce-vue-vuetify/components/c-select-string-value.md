# c-select-string-value

<!-- MARKER:summary -->
    
A dropdown component that will present a list of suggested string values from a custom API endpoint. Allows users to input values that aren't provided by the endpoint.

Effectively, this is a server-driven autocomplete list.

<!-- MARKER:summary-end -->

## Examples

``` vue-html
<c-select-string-value 
    :model="person" 
    for="jobTitle"
    method="getSuggestedJobTitles"
/>
```

``` vue-html
<c-select-string-value 
    v-model="title"
    label="Job Title"
    for="Person"
    method="getSuggestedJobTitles"
/>
```

``` c#
class Person 
{
    public int PersonId { get; set; } 

    public string JobTitle { get; set; }

    [Coalesce]
    public static async Task<ICollection<string>> GetSuggestedJobTitles(AppDbContext db, string search) 
    {
        return await db.People
            .Select(p => p.JobTitle)
            .Distinct()
            .Where(t => t.StartsWith(search))
            .OrderBy(t => t)
            .Take(100)
            .ToListAsync()
    }
}
```

## Props

<Prop def="for: string | Property | Value" lang="ts" />

A metadata specifier for the value being bound. One of:
    
- A string with the name of the value belonging to `model`.
- A direct reference to a metadata object.
- A string in dot-notation that starts with a type name.

<Prop def="model: Model" lang="ts" />

An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

<Prop def="method: string" lang="ts" />

The camel-cased name of the [Custom Method](/modeling/model-components/methods.md) to invoke to get the list of valid values. Will be passed a single string parameter `search`. Must be a static method on the type of the provided `model` object that returns a collection of strings.

<Prop def="params?: DataSourceParameters" lang="ts" />

An optional set of [Data Source Standard Parameters](/modeling/model-components/data-sources.md#standard-parameters) to pass to API calls made to the server.

<Prop def="listWhenEmpty?: boolean = false" lang="ts" />

True if the method should be invoked and the list displayed when the entered search term is blank.

<Prop def="eager?: boolean = false" lang="ts" />

True if the bound value should be updated as the user types. Otherwise, the bound value is updated when focus is lost or when a suggested value is chosen. This is only applicable for Vuetify 2 - in Vuetify 3, this is the default behavior.



