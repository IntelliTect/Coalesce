# Audit Logging

Keeping a history of all (or most) of the changes that are made to records in your database can be invaluable, both for [non-repudiation](https://csrc.nist.gov/glossary/term/non_repudiation) (i.e. proving what happened and who did it), and for troubleshooting or debugging.

Coalesce provides a package `IntelliTect.Coalesce.AuditLogging` that adds an easy way to inject this kind of audit logging into your EF Core `DbContext`. It also includes an out-of-the-box view [`c-admin-audit-log-page`](/stacks/vue/coalesce-vue-vuetify/components/c-admin-audit-log-page.md) that enables browsing of this data on the frontend.

## Setup

In this setup process, we're going to add an additional Coalesce Nuget package, define a custom entity to hold our audit logs, install the audit logging extension into our `DbContext`, and add a pre-made interface on the frontend to view our logs.

### 1. Add the NuGet package

Add a reference to the Nuget package `IntelliTect.Coalesce.AuditLogging` to your data project:

``` xml:no-line-numbers{3}
<ItemGroup>
  <PackageReference Include="IntelliTect.Coalesce" Version="$(CoalesceVersion)" />
  <PackageReference Include="IntelliTect.Coalesce.AuditLogging" Version="$(CoalesceVersion)" />
</ItemGroup>
```

### 2. Define the log entity

Define the entity type that will hold the audit records in your database:

``` c#
using IntelliTect.Coalesce.AuditLogging;

[Read(Roles = "Administrator")]
public class AuditLog : DefaultAuditLog
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    // Other custom props as desired
}
```

This entity only needs to implement `IAuditLog`, but a default implementation of this interface `DefaultAuditLog` is provided for your convenience. `DefaultAuditLog` contains additional properties `ClientIp`, `Referrer`, and `Endpoint` for recording information about the HTTP request (if available), and also comes pre-configured for security with Create, Edit, and Delete APIs disabled.

You should further augment this type with any additional properties that you would like to track on each change record. A property to track the user who performed the change should be added, since it is not provided by the default implementation so that you can declare it yourself with the correct type for the foreign key and navigation property.

You should also apply security to restrict reading of these records to only the most privileged users with a [Read Attribute](/modeling/model-components/attributes/security-attribute.md#read) (as in the example above) and/or a [custom Default Data Source](/modeling/model-components/data-sources.md#defining-data-sources).

### 3. Configure your `DbContext`

On your `DbContext`, implement the `IAuditLogDbContext<AuditLog>` interface using the class you just created as the type parameter. Then register the Coalesce audit logging extension in your `DbContext`'s `OnConfiguring` method so that saves will be intercepted and audit log entries created.

``` c#
[Coalesce]
public class AppDbContext : DbContext, IAuditLogDbContext<AuditLog>
{
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AuditLogProperty> AuditLogProperties { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCoalesceAuditLogging<AuditLog>(x => x
            .WithAugmentation<OperationContext>()
        );
    }
}
```

You could also perform this setup in your web project when calling `.AddDbContext()`.

The above code also contains a reference to a class `OperationContext`. This is the service that will allow you to populate additional custom properties on your audit entries. You'll want to define it as follows:

``` c#
public class OperationContext : DefaultAuditOperationContext<AuditLog>
{
    // Inject any additional desired services in the constructor:
    public OperationContext(IHttpContextAccessor accessor) : base(accessor) { }

    public override void Populate(AuditLog auditEntry, EntityEntry changedEntity)
    {
        base.Populate(auditEntry, changedEntity);

        // Adjust as needed to retrieve your UserId from the ClaimsPrincipal.
        auditEntry.UserId = User.GetUserId();
    }
}
```

When you're inheriting from `DefaultAuditLog` for your `IAuditLog` implementation, you'll want to similarly inherit from `DefaultAuditOperationContext<>` for your operation context. It will take care of populating the HTTP request tracking fields on the `AuditLog` record. If you want a totally custom implementation, you only need to implement the `IAuditOperationContext<TAuditLog>` interface.

The operation context class passed to `WithAugmentation` will be injected from the application service provider if available; otherwise, a new instance will be constructed using dependencies from the application service provider. To make an injected dependency optional, make the constructor parameter nullable with a default value of `null`, or create [alternate constructors](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#multiple-constructor-discovery-rules).

### 4. Add the UI

For Vue applications, the [c-admin-audit-log-page](/stacks/vue/coalesce-vue-vuetify/components/c-admin-audit-log-page.md) component provides an out-of-the-box user interface for browsing through audit logs. Simply define the following route in your application's router:

``` ts
import { CAdminAuditLogPage } from 'coalesce-vue-vuetify3';

{
  path: '/admin/audit-logs',
  component: CAdminAuditLogPage,
  props: { type: 'AuditLog' }
}
```

## Configuration

### Suppression

You can turn audit logging on or off for individual operations by implementing the `SuppressAudit` property on your DbContext. For example, implement it as an auto-property as follows and then set it to `true` in application code when desired: 

``` c#
[Coalesce]
public class AppDbContext : DbContext, IAuditLogDbContext<AuditLog>
{
    ...
    public bool SuppressAudit { get; set; }
}
```

### Exclusions & Formatting

Coalesce's audit logging uses Entity Framework's change tracking capabilities to automatically detect and record changes to your entities. It intercepts the `SaveChanges` operation to capture entity state changes and property modifications before they're committed to the database.

You can configure which entities and properties to include or exclude from auditing, as well as customize how property values are formatted in the audit logs. Coalesce's configuration extensions allow for targeted configuration per context without relying on global static singletons. For example:

``` c#
public class AppDbContext : DbContext, IAuditLogDbContext<AuditLog>
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCoalesceAuditLogging<AuditLog>(x => x
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

### Property Descriptions

The `AuditLogProperty` children of your `IAuditLog` implementation have two properties `OldValueDescription` and `NewValueDescription` that can be used to hold a description of the old and new values. By default, Coalesce will populate the descriptions of foreign key properties with the [List Text](/modeling/model-components/attributes/list-text.md) of the referenced principal entity. This greatly improves the usability of the audit logs, which would otherwise only show meaningless numbers or GUIDs for foreign keys that changed.

This feature will load principal entities into the `DbContext` if they are not already loaded, which could inflict subtle differences in application functionality in rare edge cases if your application is making assumptions about navigation properties not being loaded. Typically though, this will not be an issue and will not lead to unintentional information disclosure to clients as long as [IncludeTree](/concepts/include-tree.md)s are used correctly.

This feature may be disabled by calling `.WithPropertyDescriptions(PropertyDescriptionMode.None)` inside your call to `.UseCoalesceAuditLogging(...)` in your DbContext configuration. You may also populate these descriptions in your `IAuditOperationContext` implementation that was provided to `.WithAugmentation<T>()`.


## Merging
When using a supported database provider (currently only SQL Server), audit records for changes to the same entity can be merged together when the change is identical in all aspects to the previous audit record for that entity, with the only allowed difference being the old/new property values.

In other words, if the same user is making repeated changes to the same property on the same entity from the same page, then those changes will merge together into one audit record.

This merging only happens together if the existing audit record is recent; the default cutoff for this is 30 seconds, but can be configured with `.WithMergeWindow(TimeSpan.FromSeconds(15))` when calling `UseCoalesceAuditLogging`. It can also be turned off by setting this value to `TimeSpan.Zero`. The merging logic respects all custom properties you add to your `IAuditLog` implementation, requiring their values to match between the existing and new audit records for a merge to occur.

By default, only non-discrete properties (those that are not foreign keys, booleans, or enums) are candidates for merging, since it is usually only such fields that will have repeated changes while a user is typing in an auto-save user interface. For other types of properties, it is usually better to capture each discrete change. This can be configured with `.WithMergeBehavior()` when calling `UseCoalesceAuditLogging`, and can be overridden on a case-by-case basis by setting `AuditLogProperty.CanMerge` in your `IAuditOperationContext.Populate` implementation.

## Caveats
Only changes that are tracked by the `DbContext`'s `ChangeTracker` can be audited. Changes that are made with raw SQL, or changes that are made with bulk update functions like [`ExecuteUpdate` or `ExecuteDelete`](https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7) will not be audited using this package.


## Audit Stamping

A lightweight alternative or addition to full audit logging is audit stamping - the process of setting fields like `CreatedBy` or `ModifiedOn` on each changed entity. This cannot record a history of exact changes, but can at least record the age of an entity and how recently it changed.

Coalesce offers a simple mechanism to register an Entity Framework save interceptor to perform this kind of action (this **does NOT** require the `IntelliTect.Coalesce.AuditLogging` package). This mechanism operates on all saves that go through Entity Framework, eliminating the need to perform this manually in individual Behaviors, Services, and Custom Methods:

``` c#
services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(connectionString) // (or other provider)
    .UseStamping<TrackingBase>((entity, user) => entity.SetTracking(user))
);
```

In the above example, `TrackingBase` is an interface or class that you would write as part of your application that defines the properties and mechanisms for performing the tracking operation. For example:

``` c#
public abstract class TrackingBase
{
    [Read, Display(Order = 1000)]
    public ApplicationUser CreatedBy { get; set; }
    
    [Read, Display(Order = 1001)]
    public string? CreatedById { get; set; }
    
    [Read, Display(Order = 1002)]
    public DateTimeOffset CreatedOn { get; set; }


    [Read, Display(Order = 1003)]
    public ApplicationUser ModifiedBy { get; set; }
    
    [Read, Display(Order = 1004)]
    public string? ModifiedById { get; set; }
    
    [Read, Display(Order = 1005)]
    public DateTimeOffset ModifiedOn { get; set; }


    public void SetTracking(ClaimsPrincipal? user) 
        => SetTracking(user?.GetApplicationUserId());
    
    public void SetTracking(int? userId)
    {
        if (this.CreatedById == null)
        {
            this.CreatedById = userId;
            this.CreatedOn = DateTimeOffset.Now;
        }

        this.ModifiedById = userId;
        this.ModifiedOn = DateTimeOffset.Now;
    }
}
```

The overload `UseStamping<TStampable>` will provide the `ClaimsPrincipal` from the current HTTP request if present, defaulting to `null` if an operation occurs outside an HTTP request (e.g. a background job). The overloads `UseStamping<TStampable, TService>` and `UseStamping<TStampable, TService1, TService2>` can be used to inject services into the operation. If more than two services are needed, you should wrap those dependencies into an additional service that takes them as dependencies.