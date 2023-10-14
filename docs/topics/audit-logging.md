# Audit Logging

Keeping a history of all (or most) of the changes that are made to records in your database can be invaluable, both for [non-repudiation](https://csrc.nist.gov/glossary/term/non_repudiation) and for troubleshooting or debugging.

Coalesce provides a package `IntelliTect.Coalesce.AuditLogging` that adds an easy way to inject this kind of audit logging into your EF Core `DbContext`. It also includes an out-of-the-box view [`c-admin-audit-log-page`](TODO LINK) that enables  browsing of this data on the frontend.

## Setup

### 1. Add NuGet package

Add a reference to the Nuget package `IntelliTect.Coalesce.AuditLogging` to your data project:

``` xml:no-line-numbers{3}
<ItemGroup>
  <PackageReference Include="IntelliTect.Coalesce.Vue" Version="$(CoalesceVersion)" />
  <PackageReference Include="IntelliTect.Coalesce.AuditLogging" Version="$(CoalesceVersion)" />
</ItemGroup>
```

### 2. Define the log entity

Define the entity type that will hold the audit history in your database:

``` c#
using IntelliTect.Coalesce.AuditLogging;

[Read(Roles = "Administrator")]
public class ObjectChange : DefaultObjectChange
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
}
```

This entity only needs to implement `IObjectChange`, but a default implementation of this interface  `DefaultObjectChange` is provided for your convenience. `DefaultObjectChange` contains additional properties `ClientIp`, `Referrer`, and `Endpoint` for recording information about the HTTP request (if available), and also has attributes to disable Create, Edit, and Delete APIs.

You should further augment this type with any additional fields that you would like to track on each change record. A property to track the user who performed the change should be added, since it is not provided by the default implementation so that you can declare it yourself with the correct type for the foreign key and navigation property.

You should also apply security to restrict reading of these records to only the most privileged users with a [Read Attribute](/modeling/model-components/attributes/security-attribute.md#read) (as in the example above) and/or a [custom Data Source](/modeling/model-components/data-sources.md#defining-data-sources).

### 3. Configure your `DbContext`

On your `DbContext`, implement the `IAuditLogContext<ObjectChange>` interface, using the class you just created as the type parameter. Additionally, register the Coalesce audit logging extension in your `DbContext`'s `OnConfiguring` method so that saves will be intercepted in order to generate and save the audit log entries.

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

The above code also contains a reference to a class `OperationContext`. This is the service that will allow you to populate additional custom fields on your audit entries. You'll want to define it as follows:

``` c#
public class OperationContext : DefaultAuditOperationContext<ObjectChange>
{
    public OperationContext(IHttpContextAccessor accessor) : base(accessor) { }

    public override void Populate(ObjectChange auditEntry, EntityEntry changedEntity)
    {
        base.Populate(auditEntry, changedEntity);

        // Adjust as needed to retrieve your UserId from the ClaimsPrincipal.
        auditEntry.UserId = User.GetUserId();
    }
}
```

When you're inheriting from `DefaultObjectChange` for your `IObjectChange` implementation, you'll want to similarly inherit from `DefaultAuditOperationContext<>` for your operation context, as `DefaultAuditOperationContext` will take care of populating the HTTP request tracking fields on the `ObjectChange` record.

The operation context class passed to `WithAugmentation` will be injected from the application service provider if available; otherwise, it will be constructed using dependencies from the application service provider.

## Suppression
DbContext.SuppressAudit

## Merging
- explain how merging with sql server works.
- .WithMergeWindow(TimeSpan.FromSeconds(15))

## Caveats
- Can only track SaveChanges. Raw SQL and EF bulk update/delete are not tracked.

## Database customization
// TODO: add nice spot here to setup Z EFPLUS config. Explain in docs that Z is used.