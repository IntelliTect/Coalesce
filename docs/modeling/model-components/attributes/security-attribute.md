# Security Attributes


Coalesce provides a collection of attributes which can provide class-level (and property-level, where appropriate) security controls over the generated API.

::: tip
This page provides API-level documentation for a specific set of attributes. For a complete overview of all the security-focused techniques that can be used in a Coalesce application, see the [Security](/topics/security.md) page.
:::


## Class vs. Property Security

There are important differences between class-level security and property-level security, beyond the usage of the attributes themselves: 

- Class-level security is enforced in the generated API Controllers, primarily as `[Authorize]` attributes on the generated actions.
- Property security is enforced in the [Generated C# DTOs](/stacks/agnostic/dtos.md).

This means that class-level security only affects calls made to that type's standard API endpoints (and any type's bulk save endpoint), but does not affect usages of that type on other types' navigation properties. For details on how to control navigation properties, see the [Security page section on Entity Reads](/topics/security.md) 

### Complex Property Logic

For property security, [`[Read]`](#read) and [`[Edit]`](#edit) can be used to apply role-based security. If you need logic more complicated than checking for the presence of a role, [[Restrict]](/modeling/model-components/attributes/restrict.md) offers the ability to write custom code to control the read and write permissions of a property.

## Implementations

### [Read]
`IntelliTect.Coalesce.DataAnnotations.ReadAttribute`

For **class-level** security, controls access to the type's generated `/get`, `/list`, and `/count` endpoints, as well as stacking with `[Edit]`/`[Save]` to control the `/bulkSave` endpoint.

For **property-level** security, controls reading of that property any time it would be returned by any Coalesce built-in or custom endpoint. If a `[Read]` attribute is present without an `[Edit]` attribute, the property is read-only. 

Additionally, you can set `NoAutoInclude = true` on the `[Read]` attribute to suppress the [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior). When targeted at a class, prevents all navigation properties of that class' type from being auto-included. When targeted at a navigation property, only that specific navigation property is affected.

#### Example Usage
``` c#
[Read(Roles = "Management")]
public class Employee
{
    public int EmployeeId { get; set; }

    [Read("Payroll")]
    public string LastFourSsn { get; set; }
    
    [Read("Payroll", NoAutoInclude = true)]
    public List<Paycheck> Paychecks { get; set; }
    ...
}
```

### [Edit]
`IntelliTect.Coalesce.DataAnnotations.EditAttribute`

For **class-level** security, controls saving of existing instances of the type through the generated `/save` and `/bulkSave` endpoints.

For **property-level** security, controls access to the property any time it accepted as input by any Coalesce built-in or custom endpoint. If a `[Read]` attribute is present, one of its roles must be fulfilled in addition to the roles specified (if any) for the `[Edit]` attribute.

#### Example Usage
``` c#
[Edit(Roles = "Management,Payroll")]
public class Employee
{
    public int EmployeeId { get; set; }

    [Read("Payroll,HumanResources"), Edit("Payroll")]
    public string LastFourSsn { get; set; }
    
    ...
}

[Edit(PermissionLevel = SecurityPermissionLevels.DenyAll)]
public class Paycheck { ... }
```

### [Create]
`IntelliTect.Coalesce.DataAnnotations.CreateAttribute`

For **class-level** security, controls saving of new instances of the type through the generated `/save` and `/bulkSave` endpoints. 

Not applicable to properties.

#### Example Usage
``` c#
[Create(Roles = "HumanResources")]
public class Employee { ... }

[Create(PermissionLevel = SecurityPermissionLevels.DenyAll)]
public class Paycheck { ... }
```

### [Delete]
`IntelliTect.Coalesce.DataAnnotations.DeleteAttribute`

For **class-level** security, controls deleting of existing instances of the type through the generated `/delete` and `/bulkSave` endpoints. 

Not applicable to properties.

#### Example Usage
``` c#
[Delete("HumanResources", "Management")]
public class Employee { ... }

[Delete(PermissionLevel = SecurityPermissionLevels.DenyAll)]
public class Paycheck { ... }
```

### [Execute]

A separate attribute for controlling method execution exists. Its documentation may be found on the [[Execute]](/modeling/model-components/attributes/execute.md) page.


## Attribute Properties

<Prop def="public string Roles { get; set; }" ctor=1 /> 

A comma-delimited list of roles that are authorized to take perform the action represented by the attribute. If the current user belongs to any of the listed roles, the action will be allowed.

<Prop def="public SecurityPermissionLevels PermissionLevel { get; set; }" ctor=1 /> 

The level of access to allow for the action for **class-level security** only. Has no effect for property-level security.

Enum values are:
- `SecurityPermissionLevels.AllowAll` Allow all users to perform the action for the attribute, including users who are not authenticated at all.
- `SecurityPermissionLevels.AllowAuthorized` **Default**. Allow only users who are members of the roles specified on the attribute to perform the action. If no roles are specified on the attribute, then all authenticated users are allowed (no anonymous access). 
- `SecurityPermissionLevels.DenyAll` Deny the action to all users, regardless of authentication status or authorization level. If `DenyAll` is used on a class, no API endpoint for the governed actions will be generated.
