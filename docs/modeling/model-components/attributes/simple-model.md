# [SimpleModel]

`IntelliTect.Coalesce.SimpleModelAttribute`

Used to mark a class as a [Simple Model](/modeling/model-types/simple-models.md) for generation by Coalesce. Must be used in conjunction with the `[Coalesce]` attribute.

## Usage

The `[SimpleModel]` attribute is used alongside the `[Coalesce]` attribute to explicitly include a standalone class as a Simple Model in your Coalesce application. This is particularly useful for data classes like JSON objects that are not naturally discovered through the typical Coalesce discovery process.

``` c#
[Coalesce]
[SimpleModel]
public class ReportSettings
{
    public string Format { get; set; }
    public bool IncludeCharts { get; set; }
    public string Theme { get; set; }
}
```

## Generated Code

When a type is marked with `[SimpleModel]`, Coalesce will generate:

* A [Generated DTO](/stacks/agnostic/dtos.md) for server-side mapping
* A [TypeScript Model](/stacks/vue/layers/models.md) for client-side usage

## Restrictions

The `[SimpleModel]` attribute should only be used on regular class types. It cannot be used on:

- Interfaces
- Enums  
- Types that already have special Coalesce roles (like `[Service]`, `[StandaloneEntity]`, etc.)
- Types that implement Coalesce interfaces (like `IDataSource<>`, `IBehaviors<>`, etc.)

Using `[SimpleModel]` on inappropriate types will generate analyzer warnings to help guide correct usage.