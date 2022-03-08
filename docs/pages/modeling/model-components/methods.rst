
.. _ModelMethods:

Methods
=======

Any public methods you place on your POCO classes that are annotated with the :ref:`CoalesceAttribute` will get built into your TypeScript ViewModels and ListViewModels, and API endpoints will be created for these methods to be called. Both instance methods and static methods are supported. Additionally, any instance methods on :ref:`Services` will also have API endpoints and TypeScript generated.

.. contents:: Contents
    :local:


Parameters
----------

The following parameters can be added to your methods:

Primitives & Dates
    Primitive values (numerics, strings, booleans, enums) and dates (:csharp:`DateTime`, :csharp:`DateTimeOffset`, and nullable variants) are accepted as parameters to be passed from the client to the method call. 

Objects
    Any object types may be passed to the method call. These may be existing :ref:`EntityModels` or :ref:`ExternalTypes`. When invoking the method on the client, the object's properties will only be serialized one level deep. If an object parameter has additional child object properties, they will not be included in the invocation of the method - only the object's primitive & date properties will be deserialized from the client.

Files
    Methods can accept file uploads by using a parameter of type :csharp:`IntelliTect.Coalesce.Models.IFile` (or any derived type, like :csharp:`IntelliTect.Coalesce.Models.File`).

:csharp:`ICollection<T>` or :csharp:`IEnumerable<T>`
    Collections of any of the above valid parameter types above are also valid parameter types.

:csharp:`<YourDbContext> db`
    If the method has a parameter of the same type as your DbContext class, the current DbContext will be passed to the method call. For :ref:`Services` which don't have a defined backing EF context, this is treated as having an implicit :csharp:`[Inject]` attribute.

:csharp:`ClaimsPrincipal user`
    If the method has a parameter of type ClaimsPrincipal, the current user will be passed to the method call.

:csharp:`[Inject] <anything>`
    If a parameter is marked with the :ref:`InjectAttribute` attribute, it will be injected from the application's :csharp:`IServiceProvider`.

:csharp:`out IncludeTree includeTree`
    If the method has an :csharp:`out IncludeTree includeTree` parameter, then the :csharp:`IncludeTree` that is passed out will be used to control serialization. See :ref:`GenDTOs` and :ref:`IncludeTree` for more information. If the method returns an :csharp:`IQueryable`, the out parameter will supersede the include tree obtained from inspecting the query.

|

Return Values
-------------

You can return virtually anything from these methods:

Primitives & Dates
    Any primitive data types may be returned - :csharp:`string`, :csharp:`int`, etc.

Model Types
    Any of the types of your models may be returned. The generated TypeScript for calling the method will use the generated :ref:`TypeScriptViewModels` of your models to store the returned value.

    If the return type is the same as the type that the method is defined on, and the method is not static, then the results of the method call will be loaded into the calling TypeScript object.

Custom Types
    Any custom type you define may also be returned from a method. Corresponding :ref:`TypeScriptViewModels` will be created for these types. See :ref:`ExternalTypes`.

    .. warning::
        When returning custom types from methods, be careful of the types of their properties. As Coalesce generates the :ref:`TypeScriptViewModels` for your :ref:`ExternalTypes`, it will also generate ViewModels for the types of any of its properties, and so on down the tree. If a type is encountered from the FCL/BCL or another package that your application uses, these generated types will get out of hand extremely quickly.

        Mark any properties you don't want generated on these :ref:`TypeScriptViewModels` with the :ref:`InternalUse` attribute, or give them a non-public access modifier. Whenever possible, don't return types that you don't own or control.

:csharp:`ICollection<T>` or :csharp:`IEnumerable<T>`
    Collections of any of the above valid return types above are also valid return types. IEnumerables are useful for generator functions using :csharp:`yield`. :csharp:`ICollection` is highly suggested over :csharp:`IEnumerable` whenever appropriate, though.

:csharp:`IQueryable<T>`
    Queryables of the valid return types above are valid return types. The query will be evaluated, and Coalesce will attempt to pull an :ref:`IncludeTree` from the queryable to shape the response. When :ref:`IncludeTree` functionality is needed to shape the response but an :csharp:`IQueryable<>` return type is not feasible, an :csharp:`out IncludeTree includeTree` parameter will do the trick as well.

Files
    Methods can return file downloads using type :csharp:`IntelliTect.Coalesce.Models.IFile` (or any derived type, like :csharp:`IntelliTect.Coalesce.Models.File`). Please see the :ref:`FileDownloads` section below for more details 

:csharp:`IntelliTect.Coalesce.Models.ItemResult<T>` or :csharp:`ItemResult`
    An :csharp:`ItemResult<T>` of any of the valid return types above, including collections, is valid. The :csharp:`WasSuccessful` and :csharp:`Message` properties on the result object will be sent along to the client to indicate success or failure of the method. The type :csharp:`T` will be mapped to the appropriate DTO object before being serialized as normal.

:csharp:`IntelliTect.Coalesce.Models.ListResult<T>`
    A :csharp:`ListResult<T>` of any of the non-collection, non-file types above, is valid. The :csharp:`WasSuccessful` :csharp:`Message`, and all paging information on the result object will be sent along to the client. The type :csharp:`T` will be mapped to the appropriate DTO objects before being serialized as normal.

    The class created for the method in TypeScript will be used to hold the paging information included in the ListResult. See below for more information about this class.


|

Security
--------

You can implement role-based security on a method by placing the :ref:`ExecuteAttribute` on the method. Placing this attribute on the method with no roles specified will simply require that the calling user be authenticated. 

Security for instance methods is also controlled by the data source that loads the instance - if the data source can't provide an instance of the requested model, the method won't be executed.

Generated TypeScript
--------------------

See :ref:`VueApiCallers` and :ref:`VueViewModels` (Vue) or :ref:`KoModelMethodTypeScript` (Knockout) for details on the code that is generated for your custom methods.

.. tip::

    Any Task-returning methods with "Async" as a suffix to the C# method's name will have the "Async" suffix stripped from the generated Typescript.

|

Instance Methods
----------------

The instance of the model will be loaded using the data source specified by an attribute :csharp:`[LoadFromDataSource(typeof(MyDataSource))]` if present. Otherwise, the model instance will be loaded using the default data source for the POCO's type. If you have a :ref:`Custom Data Source <DataSources>` annotated with :csharp:`[DefaultDataSource]`, that data source will be used. Otherwise, the :ref:`StandardDataSource` will be used.

Instance methods are generated onto the TypeScript ViewModels.

| 

Static Methods
--------------

Static methods are generated onto the TypeScript ListViewModels. All of the same members that are generated for instance methods are also generated for static methods.

If a static method returns the type that it is declared on, it will also be generated on the TypeScript ViewModel of its class (Knockout only).

.. code-block:: c#

    public static ICollection<string> NamesStartingWith(string characters, AppDbContext db)
    {
        return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
    }

| 

Method Annotations
------------------

Methods can be annotated with attributes to control API exposure and TypeScript generation. The following attributes are available for model methods. General annotations can be found on the :ref:`ModelAttributes` page.

:csharp:`[Coalesce]`
    The :ref:`CoalesceAttribute` attribute causes the method to be exposed via a generated API controller. This is not needed for methods defined on an interface marked with :csharp:`[Service]` - Coalesce assumes that all methods on the interface are intended to be exposed. If this is not desired, create a new, more restricted interface with only the desired methods to be exposed.

:csharp:`[ControllerAction(Method = HttpMethod, VaryByProperty = string)]`
    The :ref:`ControllerActionAttribute` attribute controls how this method is exposed via HTTP. Can be used to customize the HTTP method/verb for the method, as well as caching behavior.

:csharp:`[Execute(string roles)]`
    The :ref:`ExecuteAttribute` attribute specifies which roles can execute this method from the generated API controller.

:csharp:`[Hidden(Areas area)]`
    The :ref:`HiddenAttribute` attribute allows for hiding this method on the admin pages both for list/card views and the editor.
        
:csharp:`[LoadFromDataSource(Type dataSourceType)]`
    The :ref:`LoadFromDataSourceAttribute` attribute specifies that the targeted model instance method should load the instance it is called on from the specified data source when invoked from an API endpoint. By default, the default data source for the model's type will be used.
    
        
        
.. _FileDownloads:

File Downloads
--------------

Coalesce supports exposing file downloads via custom methods. Simply return a :csharp:`IntelliTect.Coalesce.Models.IFile` (or any derived type, like :csharp:`IntelliTect.Coalesce.Models.File`), or an :csharp:`ItemResult<>` of such.

Consuming file downloads
************************

There are a few conveniences for easily consuming downloaded files from your custom pages.

.. tabs::

    .. group-tab:: Vue
                    
        The :ref:`VueApiCallers` have a property :ts:`url`. This can be provided directly to your HTML template, with the browser invoking the endpoint automatically.

        .. code-block:: vue

            import { PersonViewModel } from '@/viewmodels.g'
        
            var viewModel = new PersonViewModel();
            viewModel.$load(1);

        .. code-block:: html

            <img :src="downloadPicture.url">

        Alternatively, the :ref:`VueApiCallers` for file-returning methods have a method :ts:`getResultObjectUrl(vue)`. If the method was invoked programmatically (i.e. via :ts:`caller()`, :ts:`caller.invoke()`, or :ts:`caller.invokeWithArgs()`), this property contains an `Object URL <https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL>`_ that can be set as the `src` of an `image` or `video` HTML tag.

        .. code-block:: vue

            import { PersonViewModel } from '@/viewmodels.g'
        
            var viewModel = new PersonViewModel();
            await viewModel.$load(1);
            await viewModel.downloadPicture();

        .. code-block:: html

            <img :src="downloadPicture.getResultObjectUrl(this)">

    .. group-tab:: Knockout
        
        The :ref:`KoModelMethodTypeScript` for HTTP GET methods have a property :ts:`url`. This can be provided directly to your HTML, with the browser invoking the endpoint as normal.
        
        .. code-block:: knockout

            var viewModel = new ViewModels.Person();
            viewModel.load(1);

        .. code-block:: html

            <img data-bind="attr: {src: downloadPicture.url }">

        Alternatively, the :ref:`KoModelMethodTypeScript` for file-returning methods have a property :ts:`resultObjectUrl`. If the method is invoked programmatically (i.e. via :ts:`.invoke()` or :ts:`.invokeWithArgs()`), this property contains an `Object URL <https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL>`_ that can be set as the `src` of an `image` or `video` HTML tag.

        .. code-block:: knockout

            var viewModel = new ViewModels.Person();
            viewModel.load(1, () => {
                viewModel.downloadPicture.invoke();
            });

        .. code-block:: html

            <img data-bind="attr: {src: downloadPicture.resultObjectUrl }">


Database-stored Files
*********************

When storing large :csharp:`byte[]` objects in your EF models, it is important that these are never loaded unless necessary. Loading these can cause significant garbage collector churn, or even `bring your app to a halt <https://github.com/dotnet/SqlClient/issues/593>`_. To achieve this with EF, you can either utilize `Table Splitting <https://docs.microsoft.com/en-us/ef/core/modeling/table-splitting>`_, or you can use an entire dedicated table that only contains a primary key and the binary content, and nothing else.

.. warning::

    Storing large binary objects in relational databases comes with significant drawbacks. For large-volume cloud solutions, it is much more costly than dedicated cloud-native file storage like Azure Storage or S3. Also of note is that the larger a database is, the more difficult its backup process becomes.

For files that are stored in your database, Coalesce supports a pattern that allows the file to be streamed directly to the HTTP response without needing to allocate a chunk of memory for the whole file at once. Simply pass an EF :csharp:`IQueryable<byte[]>` to the constructor of :csharp:`IntelliTect.Coalesce.Models.File`. This implementation, however, is specific to the underlying EF database provider. Currently, only SQL Server and SQLite are supported. Please open a Github issue to request support for other providers. An example of this mechanism is included in the `DownloadAttachment` method in the code sample below.

The following is an example of utilizing Table Splitting for database-stored files. Generally speaking, metadata about the file should be stored on the "main" entity, and only the bytes of the content should be split into a separate entity.

.. code-block:: c#

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


Other File Storage
******************

For any other storage mechanism, implementations are similar to the database storage approach above. However, instead of table splitting or using a whole separate table, the file contents are simply stored elsewhere. Continue storing metadata about the file on the primary entity, and implement upload/download methods as desired that wrap the storage provider. 

For downloads, prefer directly providing the underlying :csharp:`Stream` to the :csharp:`IFile` versus wrapping a :csharp:`byte[]` in a :csharp:`MemoryStream`. This will reduce server memory usage and garbage collector churn.

For cloud storage providers where complex security logic is not needed, consider having clients consume the URL of the cloud resource directly rather than passing the file content through your own server.