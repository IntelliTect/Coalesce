# Coalesce Swashbuckle

When using Coalesce to generate API endpoints the default Swagger generation can sometimes result in verbose and confusing API definitions, especially when dealing with DataSources and Behaviors. To address these issues, the `IntelliTect.Coalesce.Swashbuckle` package offers enhancements for Swagger definitions, making your Coalesce-generated APIs clearer and more manageable.

## Setup

In this setup process, we're going to add an additional Coalesce NuGet package, configure Swagger in your ASP.NET Core application, and specify a Coalesce-specific config property to improve the Swagger documentation for Coalesce-generated APIs.

### 1. Add the NuGet Package

Add a reference to the `IntelliTect.Coalesce.Swashbuckle` NuGet package to your web project:

```xml:no-line-numbers{3}
<ItemGroup>
  <PackageReference Include="IntelliTect.Coalesce.Vue" Version="$(CoalesceVersion)" />
  <PackageReference Include="IntelliTect.Coalesce.Swashbuckle" Version="$(CoalesceVersion)" />
</ItemGroup>
```

### Configure Swagger in Program.cs

Update your Program.cs file to configure Swagger and include Coalesce-specific enhancements. This involves [setting up Swagger as usual](https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio) and then applying the Coalesce configuration (Note: You do not need to install the `Swashbuckle.AspNetCore` package if you are using the coalesce one).

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

### Default Swagger Generation

By default, Swagger in ASP.NET Core offers a simple way to document APIs. It generates API documentation based on the structure of your controller actions and data models. While this default setup is functional for many scenarios, it may fall short in representing more complex cases, especially when dealing with Coalesce-generated endpoints that include DataSources and Behaviors. These scenarios can lead to verbose and sometimes confusing Swagger documentation.

### Coalesce Enhancements

The `IntelliTect.Coalesce.Swashbuckle` package addresses the limitations of the default Swagger generation by providing custom Swagger filters. These filters enhance the readability and usability of your Swagger documentation for Coalesce-generated APIs.

#### CoalesceDocumentFilter:

**Purpose:** Cleans up unnecessary or redundant schema definitions in the Swagger document.

**Functionality:** The filter removes empty or irrelevant definitions related to IDataSource and IBehaviors. These are interfaces used internally by Coalesce but do not need to appear in the Swagger documentation. By removing these definitions, the Swagger UI becomes more focused and less cluttered.

#### CoalesceApiOperationFilter

**Purpose:** Helps refine the operational details in the Swagger documentation for Coalesce-generated endpoints.

**Functionality:** The filter processes various aspects of the API operations to provide cleaner generated API docs:

- DTO Parameters: Adjusts the representation of DTO parameters to ensure only relevant properties are included.

- Data Sources: Enhances the description and the schema of data source parameters so they are accurately represented.

- Standard Parameters: It removes unnecessary parameters related to behaviors and data sources, improving the clarity of the generated swagger API documentation.

## Visual Comparison 

To illustrate the impact of the `IntelliTect.Coalesce.Swashbuckle` package, let's examine the Patient model and its representation in Swagger.

```c#
public class Patient
{
    public int PatientId { get; init; }
    public DateTime NextAppoitment { get; set; }
    // Additional parameters

    [DefaultDataSource]
    public class PatientDataSource(CrudContext<AppDbContext> context) : StandardDataSource<Patient, AppDbContext>(context)
    {
        // ...
    }

    public class PatientsWithUpcommingAppoitmentsDataSource(CrudContext<AppDbContext> context) : StandardDataSource<Patient, AppDbContext>(context)
    {
        [Coalesce]
        public int MonthsOut { get; set; }

        // ...
    }
}
```

#### Without `IntelliTect.Coalesce.Swashbuckle`
In the default Swagger configuration, DataSource and Behavior parameters are represented as generic objects. DataSource names are also shown as plain strings, hindering the discoverability of available data sources.
![](./coalesce-swashbuckle-without.png)

#### With `IntelliTect.Coalesce.Swashbuckle`
With the `IntelliTect.Coalesce.Swashbuckle` package, Swagger can interpret the DataSource as a dropdown menu and provides individual fields for each DataSource property. Additionally, it eliminates unnecessary behavior parameters.
![](./coalesce-swashbuckle-with.png)