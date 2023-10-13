# Audit Logging

Keeping a history of all (or most) of the changes that are made to records in your database can be invaluable, both for [non-repudiation](https://csrc.nist.gov/glossary/term/non_repudiation) and for troubleshooting or debugging.

Coalesce provides a package `IntelliTect.Coalesce.AuditLogging` that adds an easy way to inject this kind of audit logging into your EF Core `DbContext`. It also includes an out-of-the-box view [`c-admin-audit-log-page`](TODO LINK) that enables  browsing of this data on the frontend.

## Setup

1. Add a reference to the Nuget package `IntelliTect.Coalesce.AuditLogging` to your data project:

``` xml:no-line-numbers{3}
<ItemGroup>
  <PackageReference Include="IntelliTect.Coalesce.Vue" Version="$(CoalesceVersion)" />
  <PackageReference Include="IntelliTect.Coalesce.AuditLogging" Version="$(CoalesceVersion)" />
</ItemGroup>
```

2. Define the entity type that will hold the audit history in your database:

``` c#
using IntelliTect.Coalesce.AuditLogging;

[Read(Roles = "Administrator")]
public class ObjectChange : DefaultObjectChange
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
}
```

This entity only needs to implement `IObjectChange`, but a default implementation of this interface  `DefaultObjectChange` is also provided for your convenience. `DefaultObjectChange` also contains additional properties `ClientIp`, `Referrer`, and `Endpoint` for recording information about the HTTP request (if available).

You should augment this type with any additional fields that you would like to track on each change record. A property to track the user who performed the change should be added, since it is not provided by the default implementation so that you can declare it yourself with the correct type for the foreign key and navigation property.


## Caveats
- Can only track SaveChanges. Raw SQL and EF bulk update/delete are not tracked.

## Database customization
- use model configuring