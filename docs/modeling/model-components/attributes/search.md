
# [Search]

`IntelliTect.Coalesce.DataAnnotations.SearchAttribute`

Coalesce supports searching through the generated API in its various implementations, including the generated list views (Table & Cards), in Select2 dropdowns, and directly through the TypeScript ListViewModels' `search` property.

The `search` parameter of the API can also be formatted as ``PropertyName:SearchTerm`` in order to search on an arbitrary property of a model. For example, a value of ``Nickname:Steve-o`` for a search term would search the `Nickname` property, even through it is not marked as searchable using this attribute.

By default, the system will search any field with the name 'Name'. If this doesn't exist, the ID is used as the only searchable field. Once you place the `Search` attribute on one or more properties on a model, only those annotated properties will be searched.

Searching will not search on properties that are hidden from the user by [Security Attributes](./security-attribute.md).

## Searchable Property Types

#### Strings
String fields will be searched based on the `SearchMethod` property on the attribute. See below.

#### Numeric Types
If the input is numeric, numeric fields will be searched for the exact value.

#### Enums
If the input is a valid name of an enum value for an enum property and that property is searchable, rows will be searched for the exact value.

#### Dates
If the input is a parsable date, rows will be searched based on that date.

Date search will do its best to guess at the user's intentions:

* Various forms of year/month combos are supported, and if only a year/month is inputted, it will look for all dates in that month, e.g. "Feb 2017" or "2016-11".
* A date without a time (or a time of exactly midnight) will search the entire day, e.g. "2017/4/18". 
* A date/time with minutes and seconds equal to 0 will search the entire hour, e.g. "April 7, 2017 11 AM".

::: tip
When searching on date properties, you should almost always set `IsSplitOnSpaces = false` on the `Search` attribute. This allows natural inputs like "July 21, 2017" to search correctly. Otherwise, only non-whitespace date formats will work, like "2017/21/07".
:::

#### Reference Navigation Properties
When a reference navigation property is marked with `[Search]`, searchable properties on the referenced object will also be searched. This behavior will go up to two levels away from the root object, and can be controlled with the `RootWhitelist` and `RootBlacklist` properties on the `[Search]` attribute that are outlined below.

#### Collection Navigation Properties
When a collection navigation property is marked with `[Search]`, searchable properties on the child objects will also be searched. This behavior will go up to two levels away from the root object, and can be controlled with the `RootWhitelist` and `RootBlacklist` properties on the `[Search]` attribute that are outlined below.

::: warning
Searches on collection navigation properties usually don't translate well with EF Core, leading to potentially degraded performance. Use this feature cautiously.
:::


## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }

    [Search]
    public string FirstName { get; set; }

    [Search]
    public string LastName { get; set; }

    [Search(IsSplitOnSpaces = false)]
    public string BirthDate { get; set; }

    public string Nickname { get; set; }

    [Search(RootWhitelist = nameof(Person))]
    public ICollection<Address> Addresses { get; set; }
}
```

## Properties

<Prop def="public bool IsSplitOnSpaces { get; set; } = true;" />

If set to true (the default), each word in the search terms will be searched for in each searchable field independently, and a row will only be considered a match if each word in the search term is a match on at least one searchable property where `IsSplitOnSpaces == true`

This is useful when searching for a full name across two or more fields. In the above example, using `IsSplitOnSpaces = true` would provide more intuitive behavior since it will search both first name and last name for each word entered into the search field. But, [you probably shouldn't be doing that](https://www.kalzumeus.com/2010/06/17/falsehoods-programmers-believe-about-names/).

<Prop def="public SearchMethods SearchMethod { get; set; } = SearchMethods.BeginsWith;" />

For string properties, specifies how the value in the property/column will be matched.

- `BeginsWith`: Search term will be checked for at the beginning of the field's value in a case insensitive manner.
- `Equals`: Search term must match the field exactly in a case insensitive manner.
- `EqualsNatural`: Search term must match exactly, using the natural casing handling of the evaluation environment. Default database collation will be used if evaluated in SQL, and exact casing will be used if evaluated in memory. This allows index seeks to be used instead of index scans, providing extra high performance searches against indexed columns
- `Contains`: Search term will be checked for anywhere inside the field's value in a case insensitive manner. **Will be slow against large databases - performance cannot be improved with database indexing.**


<Prop def="public string RootWhitelist { get; set; } = null;" />

A comma-delimited list of model class names that, if set, will prevent the targeted property from being searched unless the root object of the API call was one of the specified class names.

<Prop def="public string RootBlacklist { get; set; } = null;" />

A comma-delimited list of model class names that, if set, will prevent the targeted property from being searched if the root object of the API call was one of the specified class names.