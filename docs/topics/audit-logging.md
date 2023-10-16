# Audit Logging

Keeping a history of all (or most) of the changes that are made to records in your database can be invaluable, both for [non-repudiation](https://csrc.nist.gov/glossary/term/non_repudiation) (i.e. proving what happened and who did it), and for troubleshooting or debugging.

Coalesce provides a package `IntelliTect.Coalesce.AuditLogging` that adds an easy way to inject this kind of audit logging into your EF Core `DbContext`. It also includes an out-of-the-box view [`c-admin-audit-log-page`](TODO LINK) that enables  browsing of this data on the frontend.

## Setup

### 1. Add the NuGet package

Add a reference to the Nuget package `IntelliTect.Coalesce.AuditLogging` to your data project:

``` xml:no-line-numbers{3}
<ItemGroup>
  <PackageReference Include="IntelliTect.Coalesce.Vue" Version="$(CoalesceVersion)" />
  <PackageReference Include="IntelliTect.Coalesce.AuditLogging" Version="$(CoalesceVersion)" />
</ItemGroup>
```

### 2. Define the log entity

Define the entity type that will hold the audit records in your database:

``` c#
using IntelliTect.Coalesce.AuditLogging;

[Read(Roles = "Administrator")]
public class ObjectChange : DefaultObjectChange
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    // Other custom props as desired
}
```

This entity only needs to implement `IObjectChange`, but a default implementation of this interface  `DefaultObjectChange` is provided for your convenience. `DefaultObjectChange` contains additional properties `ClientIp`, `Referrer`, and `Endpoint` for recording information about the HTTP request (if available), and also has attributes to disable Create, Edit, and Delete APIs.

You should further augment this type with any additional fields that you would like to track on each change record. A property to track the user who performed the change should be added, since it is not provided by the default implementation so that you can declare it yourself with the correct type for the foreign key and navigation property.

You should also apply security to restrict reading of these records to only the most privileged users with a [Read Attribute](/modeling/model-components/attributes/security-attribute.md#read) (as in the example above) and/or a [custom Default Data Source](/modeling/model-components/data-sources.md#defining-data-sources).

### 3. Configure your `DbContext`

On your `DbContext`, implement the `IAuditLogContext<ObjectChange>` interface using the class you just created as the type parameter. Then register the Coalesce audit logging extension in your `DbContext`'s `OnConfiguring` method so that saves will be intercepted and audit log entries created.

``` c#
[Coalesce]
public class AppDbContext : DbContext, IAuditLogContext<ObjectChange>
{
    public DbSet<ObjectChange> ObjectChanges { get; set; }
    public DbSet<ObjectChangeProperty> ObjectChangeProperties { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCoalesceAuditLogging<ObjectChange>(x => x
            .WithAugmentation<OperationContext>()
        );
    }
}
```

You could also perform this setup in your web project when calling `.AddDbContext()`.

The above code also contains a reference to a class `OperationContext`. This is the service that will allow you to populate additional custom fields on your audit entries. You'll want to define it as follows:

``` c#
public class OperationContext : DefaultAuditOperationContext<ObjectChange>
{
    // Inject any additional desired services in the constructor:
    public OperationContext(IHttpContextAccessor accessor) : base(accessor) { }

    public override void Populate(ObjectChange auditEntry, EntityEntry changedEntity)
    {
        base.Populate(auditEntry, changedEntity);

        // Adjust as needed to retrieve your UserId from the ClaimsPrincipal.
        auditEntry.UserId = User.GetUserId();
    }
}
```

When you're inheriting from `DefaultObjectChange` for your `IObjectChange` implementation, you'll want to similarly inherit from `DefaultAuditOperationContext<>` for your operation context. It will take care of populating the HTTP request tracking fields on the `ObjectChange` record.

The operation context class passed to `WithAugmentation` will be injected from the application service provider if available; otherwise, a new instance will be constructed using dependencies from the application service provider. To make an injected dependency optional, make the constructor parameter nullable with a default value of `null`, or create [alternate constructors](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#multiple-constructor-discovery-rules).

## Configuration

### Suppression

You can turn audit logging on or off for individual operations by implementing the `SuppressAudit` property on your DbContext. For example, implement it as an auto-property as follows and then set it to `true` in application code when desired: 

``` c#
[Coalesce]
public class AppDbContext : DbContext, IAuditLogContext<ObjectChange>
{
    ...
    public bool SuppressAudit { get; set; }
}
```

### Exclusions & Formatting

Coalesce's audit logging is built on top of [Entity Framework Plus](https://entityframework-plus.net/ef-core-audit) and can be configured using all of its [configuration](https://entityframework-plus.net/ef-core-audit#scenarios), including [includes/excludes](https://entityframework-plus.net/ef-core-audit-exclude-include-entity) and [custom property formatting](https://entityframework-plus.net/ef-core-audit-format-value).

While Coalesce will respect EF Plus's global configuration, it is recommended that you instead use Coalesce's configuration extensions which allow for more targeted configuration that does not rely on a global static singleton. For example:

``` c#
public class AppDbContext : DbContext, IAuditLogContext<ObjectChange>
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCoalesceAuditLogging<ObjectChange>(x => x
            .WithAugmentation<OperationContext>()
            .ConfigureAudit(c => c
                .Exclude<DataProtectionKey>()
                .ExcludeProperty<TrackingBase>(x => new { x.CreatedById, x.CreatedOn, x.ModifiedById, x.ModifiedOn })
                .FormatType<DateTimeOffset>(d => d.ToTimeZone("America/Los_Angeles").ToString())
                .Format<Image>(x => x.Content, x => $"{Convert.ToHexString(SHA1.HashData(x))}, {x.Length} bytes")
            )
        );
    }
}
```

If you use `ConfigureAudit`, `AuditManager.DefaultConfiguration` will not be used.

## Merging
When using a supported database provider (currently only SQL Server), audit records for changes to the same entity will be merged together when the change is identical in all aspects to the previous audit record for that entity, with the sole exception of the old/new property values.

In other words, if the same user is making repeated changes to the same property on the same entity from the same page, then those changes will merge together into one audit record.

This merging only happens together if the existing audit record is recent; the default cutoff for this is 30 seconds, but can be configured with `.WithMergeWindow(TimeSpan.FromSeconds(15))` when calling `UseCoalesceAuditLogging`. It can also be turned off by setting this value to `TimeSpan.Zero`. The merging logic respects all custom properties you add to your `IObjectChange` implementation, requiring their values to match between the existing and new audit records for a merge to occur.

## Caveats
Only changes that are tracked by the `DbContext`'s `ChangeTracker` can be audited. Changes that are made with raw SQL, or changes that are made with bulk update functions like [`ExecuteUpdate` or `ExecuteDelete`](https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7) will not be audited using this package.

