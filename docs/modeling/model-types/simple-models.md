# Simple Models

In Coalesce, any data class exposed by Coalesce that is **not** a [CRUD Model](/modeling/model-types/crud.md) is considered to be a "simple model". Simple models are just plain data objects and do not have any API endpoints or other advanced functionality.

::: info
Simple Models used to be called External Types prior to Coalesce v6. Their functionality did not change.
:::

The set of simple models in a Coalesce application looks like this:
    
1. Take all of the exposed property types, method parameters, and method return types of your [CRUD Models](/modeling/model-types/crud.md), as well as method parameters and returns from [Services](/modeling/model-types/services.md).
2. Any of these types which are not built-in scalar types and not one of the aforementioned CRUD models are simple models.
3. For any simple model discovered, any of the property types which qualify under the above rules are also simple models.

::: warning
Be careful when using types that you do not own for properties and method returns in your data model. When Coalesce generates simple model ViewModels and DTOs, it will not stop until it has exhausted all paths that can be reached by following public property types and method returns.

In general, you should only expose types that you have created so that you will always have full control over them. Mark any properties you don't wish to expose with [[InternalUse]](/modeling/model-components/attributes/internal-use.md), or make those members non-public.
:::


## Generated Code

For each simple model found in your application's model, Coalesce will generate:

* A [Generated DTO](/stacks/agnostic/dtos.md)
* A [TypeScript Model](/stacks/vue/layers/models.md)


## Example Data Model

For example, in the following scenario, these classes are considered simple models:

* `ReportParameters`, exposed through a method parameter on `ReportService`.
* `ReportResponse`, exposed through a method return on `ReportService`.
* `ReportSummary`, exposed through a property on `ReportResponse`.

``` c#
[Coalesce, Service]
public class IReportService {
    public SalesReport GenerateSalesReport(ReportParameters parameters);
}

public class ReportParameters { 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Category { get; set; }
}

public class SalesReport { 
    public ReportSummary Summary { get; set; }
    public string[] Data { get; set; }
}

public class ReportSummary { 
    public int TotalRecords { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

## Loading & Serialization

Simple Models have slightly different behavior when undergoing serialization to be sent to the client. Unlike CRUD Models types which are subject to the rules of [Include Tree](/concepts/include-tree.md), simple models ignore the Include Tree when being mapped to DTOs for serialization. Read [Simple Model Caveats](/concepts/include-tree.md#simple-model-caveats) for a more detailed explanation of this exception.