# OpenAPI/Swagger

When using Coalesce to generate API endpoints, the default OpenAPI generation _(sometimes referred to as its pre-2015 name "Swagger")_ can sometimes result in verbose and confusing API definitions, especially when dealing with DataSources and Behaviors. To address these issues, the `IntelliTect.Coalesce.Swashbuckle` package offers enhancements for OpenAPI definitions, making your Coalesce-generated APIs clearer and more manageable.

## Setup

In this setup process, we're going to add an additional Coalesce NuGet package, configure OpenAPI in your ASP.NET Core application, and specify a Coalesce-specific config property to improve the OpenAPI documentation for Coalesce-generated APIs.

### 1. Add the NuGet Package

Add a reference to the `IntelliTect.Coalesce.Swashbuckle` NuGet package to your web project:

```xml:no-line-numbers{3}
<ItemGroup>
  <PackageReference Include="IntelliTect.Coalesce.Vue" Version="$(CoalesceVersion)" />
  <PackageReference Include="IntelliTect.Coalesce.Swashbuckle" Version="$(CoalesceVersion)" />
</ItemGroup>
```

### Configure OpenAPI in Program.cs

Update your Program.cs file to configure OpenAPI and include Coalesce-specific enhancements. This involves [setting up OpenAPI as usual](https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio) and then applying the Coalesce configuration (Note: You do not need to install the `Swashbuckle.AspNetCore` package if you are using the coalesce one).

```c#:no-line-numbers
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    config.AddCoalesce(); // Add coalesce specific configuration
});
```

```c#
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Improvements

### Default OpenAPI Generation

By default, OpenAPI in ASP.NET Core offers a simple way to document APIs. It generates API documentation based on the structure of your controller actions and data models. While this default setup is functional for many scenarios, it may fall short in representing more complex cases, especially when dealing with Coalesce-generated endpoints that include DataSources and Behaviors. These scenarios can lead to verbose and sometimes confusing OpenAPI documentation.

### Coalesce Enhancements

The `IntelliTect.Coalesce.Swashbuckle` package addresses the limitations of the default OpenAPI generation by providing custom OpenAPI filters. These filters enhance the readability and usability of your OpenAPI documentation for Coalesce-generated APIs.

The primary effect is an adjustment of parameter definitions to account for Coalesce's custom model binders that create instances of Data Sources and Behaviors on each request. These parameters will be updated in the OpenAPI document to account for data source parameters, filter parameters, and other model-specific customizations.

## Visual Comparison 

To illustrate the impact of the `IntelliTect.Coalesce.Swashbuckle` package, let's examine the Patient model and its representation in OpenAPI.

```c#
public class Patient
{
    public int PatientId { get; init; }
    public DateTime NextAppointment { get; set; }
    // Additional properties

    [DefaultDataSource]
    public class PatientDataSource(CrudContext<AppDbContext> context) : StandardDataSource<Patient, AppDbContext>(context)
    {
        // ...
    }

    public class PatientsWithUpcomingAppointmentsDataSource(CrudContext<AppDbContext> context) : StandardDataSource<Patient, AppDbContext>(context)
    {
        [Coalesce]
        public int MonthsOut { get; set; }

        // ...
    }
}
```

#### Without `IntelliTect.Coalesce.Swashbuckle`
In the default OpenAPI configuration, DataSource and Behavior parameters are represented as generic objects. DataSource names are also shown as plain strings, hindering the discoverability of available data sources.
![](./coalesce-swashbuckle-without.jpg)

#### With `IntelliTect.Coalesce.Swashbuckle`
With the `IntelliTect.Coalesce.Swashbuckle` package, OpenAPI can interpret the DataSource as a dropdown menu and provides individual fields for each DataSource property. Additionally, it eliminates unnecessary behavior parameters.
![](./coalesce-swashbuckle-with.jpg)