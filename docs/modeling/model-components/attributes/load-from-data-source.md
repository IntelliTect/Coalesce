# [LoadFromDataSource]

Specifies that the targeted model instance method should load the instance it is called on from the 
specified data source when invoked from an API endpoint. By default, the default data source for the model's type will be used.

## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }
    public string FirstName { get; set; }

    [Coalesce, LoadFromDataSource(typeof(WithoutCases))]
    public void ChangeSpacesToDashesInName()
    {
        FirstName = FirstName.Replace(" ", "-");
    }
}
```

## Properties

<Prop def="public Type DataSourceType { get; }" ctor=1 />

The name of the [Data Source](/modeling/model-components/data-sources.md) to load the instance object from.
