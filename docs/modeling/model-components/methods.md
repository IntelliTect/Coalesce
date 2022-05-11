# Methods

Any public methods you place on your POCO classes that are annotated with the [[Coalesce]](/modeling/model-components/attributes/coalesce.md) will get built into your TypeScript ViewModels and ListViewModels, and API endpoints will be created for these methods to be called. Both instance methods and static methods are supported. Additionally, any instance methods on [Services](/modeling/model-types/services.md) will also have API endpoints and TypeScript generated.

[[toc]]


## Parameters

The following parameters can be added to your methods:

### Primitives, Scalars, & Dates
Most common built-in primitive and scalar data types (numerics, strings, booleans, enums, `DateTime`, `DateTimeOffset`), and their nullable variants, are accepted as parameters to be passed from the client to the method call. 

### Objects
Any object types may be passed to the method call. These may be existing [Entity Models](/modeling/model-types/entities.md) or [External Types](/modeling/model-types/external-types.md). When invoking the method on the client, the object's properties will only be serialized one level deep. If an object parameter has additional child object properties, they will not be included in the invocation of the method - only the object's primitive & date properties will be deserialized from the client.

### Files
Methods can accept file uploads by using a parameter of type `IntelliTect.Coalesce.Models.IFile` (or any derived type, like `IntelliTect.Coalesce.Models.File`).

### `ICollection<T>` or `IEnumerable<T>`
Collections of any of the above valid parameter types above are also valid parameter types.

### `DbContext db`
If the method has a parameter assignable to `Microsoft.EntityFrameworkCore.DbContext`, then the parameter will be implicitly `[Inject]`ed.

### `ClaimsPrincipal user`
If the method has a parameter of type ClaimsPrincipal, the value of `HttpContext.User` will be passed to the parameter.

### `[Inject] service`
If a parameter is marked with the [[Inject]](/modeling/model-components/attributes/inject.md) attribute, it will be injected from the application's `IServiceProvider`.

### `out IncludeTree includeTree`
If the method has an `out IncludeTree includeTree` parameter, then the `IncludeTree` that is passed out will be used to control serialization. See [Generated C# DTOs](/stacks/agnostic/dtos.md) and [Include Tree](/concepts/include-tree.md) for more information. If the method returns an `IQueryable`, the out parameter will supersede the include tree obtained from inspecting the query.


## Return Values

You can return virtually anything from these methods:

### Primitives, Scalars, & Dates
Most common built-in primitive and scalar data types (numerics, strings, booleans, enums, `DateTime`, `DateTimeOffset`), and their nullable variants, may be returned from methods.

### Model Types
Any of the types of your models may be returned. The generated TypeScript for calling the method will use the generated [TypeScript ViewModels](/stacks/disambiguation/view-model.md) of your models to store the returned value.

If the return type is the same as the type that the method is defined on, and the method is not static, then the results of the method call will be loaded into the calling TypeScript object.

### Custom Types
Any custom type you define may also be returned from a method. Corresponding [TypeScript ViewModels](/stacks/disambiguation/view-model.md) will be created for these types. See [External Types](/modeling/model-types/external-types.md).

::: warning
When returning custom types from methods, be careful of the types of their properties. As Coalesce generates the [TypeScript ViewModels](/stacks/disambiguation/view-model.md) for your [External Types](/modeling/model-types/external-types.md), it will also generate ViewModels for the types of any of its properties, and so on down the tree. If a type is encountered from the FCL/BCL or another package that your application uses, these generated types will get out of hand extremely quickly.

Mark any properties you don't want generated on these [TypeScript ViewModels](/stacks/disambiguation/view-model.md) with the [[InternalUse]](/modeling/model-components/attributes/internal-use.md) attribute, or give them a non-public access modifier. Whenever possible, don't return types that you don't own or control.
:::

### `ICollection<T>` or `IEnumerable<T>`
Collections of any of the above valid return types above are also valid return types. IEnumerables are useful for generator functions using `yield`. `ICollection` is highly suggested over `IEnumerable` whenever appropriate, though.

### `IQueryable<T>`
Queryables of the valid return types above are valid return types. The query will be evaluated, and Coalesce will attempt to pull an [Include Tree](/concepts/include-tree.md) from the queryable to shape the response. When [Include Tree](/concepts/include-tree.md) functionality is needed to shape the response but an `IQueryable<>` return type is not feasible, an `out IncludeTree includeTree` parameter will do the trick as well.

### Files
Methods can return file downloads using type `IntelliTect.Coalesce.Models.IFile` (or any derived type, like `IntelliTect.Coalesce.Models.File`). Please see the [File Downloads](/modeling/model-components/methods.md) section below for more details 

### `ItemResult<T>` or `ItemResult`
An `IntelliTect.Coalesce.Models.ItemResult<T>`, or its non-generic variant `ItemResult` of any of the valid return types above, including collections, is valid. The `WasSuccessful` and `Message` properties on the result object will be sent along to the client to indicate success or failure of the method. The type `T` will be mapped to the appropriate DTO object before being serialized as normal.

### `ListResult<T>`
A `IntelliTect.Coalesce.Models.ListResult<T>` of any of the non-collection, non-file types above, is valid. The `WasSuccessful` `Message`, and all paging information on the result object will be sent along to the client. The type `T` will be mapped to the appropriate DTO objects before being serialized as normal.

The class created for the method in TypeScript will be used to hold the paging information included in the ListResult. See below for more information about this class.


## Security

You can implement role-based security on a method by placing the [[Execute]](/modeling/model-components/attributes/execute.md) on the method. Placing this attribute on the method with no roles specified will simply require that the calling user be authenticated. 

Security for instance methods is also controlled by the data source that loads the instance - if the data source can't provide an instance of the requested model, the method won't be executed.

## Generated TypeScript

See [API Callers](/stacks/vue/layers/api-clients.md) and [ViewModel Layer](/stacks/vue/layers/viewmodels.md) (Vue) or [TypeScript Method Objects](/stacks/ko/client/methods.md) (Knockout) for details on the code that is generated for your custom methods.

::: tip Note
Any Task-returning methods with "Async" as a suffix to the C# method's name will have the "Async" suffix stripped from the generated Typescript.
:::


## Instance Methods

The instance of the model will be loaded using the data source specified by an attribute `[LoadFromDataSource(typeof(MyDataSource))]` if present. Otherwise, the model instance will be loaded using the default data source for the POCO's type. If you have a [Custom Data Source](/modeling/model-components/data-sources.md) annotated with `[DefaultDataSource]`, that data source will be used. Otherwise, the [Standard Data Source](/modeling/model-components/data-sources.md) will be used.

Instance methods are generated onto the TypeScript ViewModels.


## Static Methods

Static methods are generated onto the TypeScript ListViewModels. All of the same members that are generated for instance methods are also generated for static methods.

If a static method returns the type that it is declared on, it will also be generated on the TypeScript ViewModel of its class (Knockout only).

``` c#
public static ICollection<string> NamesStartingWith(string characters, AppDbContext db)
{
    return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
}
```


## Method Annotations

Methods can be annotated with attributes to control API exposure and TypeScript generation. The following attributes are available for model methods. General annotations can be found on the [Attributes](/modeling/model-components/attributes.md) page.

### `[Coalesce]`
The [[Coalesce]](/modeling/model-components/attributes/coalesce.md) attribute causes the method to be exposed via a generated API controller. This is not needed for methods defined on an interface marked with `[Service]` - Coalesce assumes that all methods on the interface are intended to be exposed. If this is not desired, create a new, more restricted interface with only the desired methods to be exposed.

### `[ControllerAction(Method = HttpMethod, VaryByProperty = string)]`
The [[ControllerAction]](/modeling/model-components/attributes/controller-action.md) attribute controls how this method is exposed via HTTP. Can be used to customize the HTTP method/verb for the method, as well as caching behavior.

### `[Execute(string roles)]`
The [[Execute]](/modeling/model-components/attributes/execute.md) attribute specifies which roles can execute this method from the generated API controller.

### `[Hidden(Areas area)]`
The [[Hidden]](/modeling/model-components/attributes/hidden.md) attribute allows for hiding this method on the admin pages both for list/card views and the editor.
        
### `[LoadFromDataSource(Type dataSourceType)]`
The [[LoadFromDataSource]](/modeling/model-components/attributes/load-from-data-source.md) attribute specifies that the targeted model instance method should load the instance it is called on from the specified data source when invoked from an API endpoint. By default, the default data source for the model's type will be used.
    

## File Downloads

Coalesce supports exposing file downloads via custom methods. Simply return a `IntelliTect.Coalesce.Models.IFile` (or any derived type, like `IntelliTect.Coalesce.Models.File`), or an `ItemResult<>` of such.

### Consuming file downloads

There are a few conveniences for easily consuming downloaded files from your custom pages.

<CodeTabs>
<template #vue>

The [API Callers](/stacks/vue/layers/api-clients.md) have a property `url`. This can be provided directly to your HTML template, with the browser invoking the endpoint automatically.


``` ts
import { PersonViewModel } from '@/viewmodels.g'

var viewModel = new PersonViewModel();
viewModel.$load(1);
```
``` html
<img :src="downloadPicture.url">
```
----
Alternatively, the [API Callers](/stacks/vue/layers/api-clients.md) for file-returning methods have a method `getResultObjectUrl(vue)`. If the method was invoked programmatically (i.e. via `caller()`, `caller.invoke()`, or `caller.invokeWithArgs()`), this property contains an [Object URL](https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL) that can be set as the `src` of an `image` or `video` HTML tag.

``` ts
import { PersonViewModel } from '@/viewmodels.g'

var viewModel = new PersonViewModel();
await viewModel.$load(1);
await viewModel.downloadPicture();
```
``` html
<img :src="downloadPicture.getResultObjectUrl(this)">
```

</template>
<template #knockout>

The [TypeScript Method Objects](/stacks/ko/client/methods.md) for HTTP GET methods have a property `url`. This can be provided directly to your HTML, with the browser invoking the endpoint as normal.
        
``` ts
var viewModel = new ViewModels.Person();
viewModel.load(1);
```
``` html
<img data-bind="attr: {src: downloadPicture.url }">
```

----
Alternatively, the [TypeScript Method Objects](/stacks/ko/client/methods.md) for file-returning methods have a property `resultObjectUrl`. If the method is invoked programmatically (i.e. via `.invoke()` or `.invokeWithArgs()`), this property contains an [Object URL](https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL) that can be set as the `src` of an `image` or `video` HTML tag.

``` ts
var viewModel = new ViewModels.Person();
viewModel.load(1, () => {
    viewModel.downloadPicture.invoke();
});
```
``` html
<img data-bind="attr: {src: downloadPicture.resultObjectUrl }">
```

</template>
</CodeTabs>


### Database-stored Files

When storing large `byte[]` objects in your EF models, it is important that these are never loaded unless necessary. Loading these can cause significant garbage collector churn, or even [bring your app to a halt](https://github.com/dotnet/SqlClient/issues/593). To achieve this with EF, you can either utilize [Table Splitting](https://docs.microsoft.com/en-us/ef/core/modeling/table-splitting), or you can use an entire dedicated table that only contains a primary key and the binary content, and nothing else.

::: warning
Storing large binary objects in relational databases comes with significant drawbacks. For large-volume cloud solutions, it is much more costly than dedicated cloud-native file storage like Azure Storage or S3. Also of note is that the larger a database is, the more difficult its backup process becomes.
:::

For files that are stored in your database, Coalesce supports a pattern that allows the file to be streamed directly to the HTTP response without needing to allocate a chunk of memory for the whole file at once. Simply pass an EF `IQueryable<byte[]>` to the constructor of `IntelliTect.Coalesce.Models.File`. This implementation, however, is specific to the underlying EF database provider. Currently, only SQL Server and SQLite are supported. Please open a Github issue to request support for other providers. An example of this mechanism is included in the `DownloadAttachment` method in the code sample below.

The following is an example of utilizing Table Splitting for database-stored files. Generally speaking, metadata about the file should be stored on the "main" entity, and only the bytes of the content should be split into a separate entity.

``` c#
public class AppDbContext : DbContext
{
    public DbSet<Case> Cases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Case>()
            .ToTable("Cases")
            .HasOne(c => c.AttachmentContent)
            .WithOne()
            .HasForeignKey<CaseAttachmentContent>(c => c.CaseId);
        modelBuilder
            .Entity<CaseAttachmentContent>()
            .ToTable("Cases")
            .HasKey(d => d.CaseId);
    }
}

public class Case
{
    public int CaseId { get; set; }

    [Read]
    public string AttachmentName { get; set; }

    [Read]
    public long AttachmentSize { get; set; }

    [Read]
    public string AttachmentType { get; set; }

    [Read, MaxLength(32)] // Adjust max length based on chosen hash algorithm.
    public byte[] AttachmentHash { get; set; } // Could also be a base64 string if so desired.

    [InternalUse]
    public CaseAttachmentContent AttachmentContent { get; set; } = new();

    [Coalesce]
    public async Task UploadAttachment(AppDbContext db, IFile file)
    {
        if (file.Content == null) return;

        var content = new byte[file.Length];
        await file.Content.ReadAsync(content.AsMemory());

        AttachmentContent = new () { CaseId = CaseId, Content = content };
        AttachmentName = file.Name;
        AttachmentSize = file.Length;
        AttachmentType = file.ContentType;
        AttachmentHash = SHA256.HashData(content);
    }

    [Coalesce]
    [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(AttachmentHash))]
    public IFile DownloadAttachment(AppDbContext db)
    {
        return new IntelliTect.Coalesce.Models.File(db.Cases
            .Where(c => c.CaseId == this.CaseId)
            .Select(c => c.AttachmentContent.Content)
        )
        {
            Name = AttachmentName,
            ContentType = AttachmentType,
        };
    }
}

public class CaseAttachmentContent
{
    public int CaseId { get; set; }

    [Required]
    public byte[] Content { get; set; }
}
```

### Other File Storage

For any other storage mechanism, implementations are similar to the database storage approach above. However, instead of table splitting or using a whole separate table, the file contents are simply stored elsewhere. Continue storing metadata about the file on the primary entity, and implement upload/download methods as desired that wrap the storage provider. 

For downloads, prefer directly providing the underlying `Stream` to the `IFile` versus wrapping a `byte[]` in a `MemoryStream`. This will reduce server memory usage and garbage collector churn.

For cloud storage providers where complex security logic is not needed, consider having clients consume the URL of the cloud resource directly rather than passing the file content through your own server.