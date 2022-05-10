.. _LoadFromDataSourceAttribute:

[LoadFromDataSource]
=====================

Specifies that the targeted model instance method should load the instance it is called on from the 
specified data source when invoked from an API endpoint. By default, the default data source for the model's type will be used.

Example Usage
-------------

``` c#

    public class Person
    {
        public int PersonId { get; set; }
        public string {get; set; }

        [Coalesce, LoadFromDataSource(typeof(WithoutCases))]
        public void ChangeSpacesToDashesInName()
        {
            FirstName = FirstName.Replace(" ", "-");
        }
    }


```

Properties
----------

`public Type DataSourceType { get; set; }` :ctor:`1`
    The DataSource to load the instance object from.
