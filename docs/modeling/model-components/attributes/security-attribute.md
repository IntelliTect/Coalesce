# Security Attributes

Coalesce provides a collection of attributes which can provide class-level (and property-level, where appropriate) security controls over the generated API.

::: tip
This page provides API-level documentation for a specific set of attributes. For a complete overview of all the security-focused techniques that can be used in a Coalesce application, see the [Security](/topics/security.md) page.
:::


## Class vs. Property Security

There are important differences between class-level security and property-level security, beyond the usage of the attributes themselves. In general, class-level security is implemented in the generated API Controllers as `[Authorize]` attributes on the generated actions. Property security attributes are implemented in the [Generated C# DTOs](/stacks/agnostic/dtos.md).


## Implementations

### [Read]
Controls permissions for reading of objects and properties through the API.

For **property-level** security only, if a `[Read]` attribute is present without an `[Edit]` attribute, the property is read-only. 

Additionally, you can set  `NoAutoInclude = true` the `[Read]` attribute to suppress the [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior).

#### Example Usage
``` c#
[Read(Roles = "Management", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
public class Employee
{
    public int EmployeeId { get; set; }

    [Read("Payroll")]
    public string LastFourSsn { get; set; }
    
    ...
}
```

### [Edit]
Controls permissions for editing of objects and properties through the API.

For **property-level** security only, if a `[Read]` attribute is present, one of its roles must be fulfilled in addition to the roles specified (if any) for the `[Edit]` attribute.

#### Example Usage
``` c#
[Edit(Roles = "Management,Payroll", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
public class Employee
{
    public int EmployeeId { get; set; }

    [Read("Payroll,HumanResources"), Edit("Payroll")]
    public string LastFourSsn { get; set; }
    
    ...
}
```

### [Create]

Controls permissions for creation of an object of the targeted type through the API.

#### Example Usage
``` c#
[Create(Roles = "HumanResources", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
public class Employee
{
    ...
}
```

### [Delete]
Controls permissions for deletion of an object of the targeted type through the API.

#### Example Usage
``` c#
[Delete(Roles = "HumanResources,Management", PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
public class Employee
{
    ...
}
```

### [Execute]
A separate attribute for controlling method execution exists. Its documentation may be found on the [[Execute]](/modeling/model-components/attributes/execute.md) page.

### [Restrict]

For property security, `[Read]` and `[Edit]` can be used to apply role-based security. If you need logic more complicated than checking for the presence of a role, [[Restrict]](/modeling/model-components/attributes/restrict.md) offers the ability to write custom code to control the read and/or write permissions of a property.

## Attribute Properties

<Prop def="public string Roles { get; set; }" ctor=1 /> 

A comma-delimited list of roles that are authorized to take perform the action represented by the attribute. If the current user belongs to any of the listed roles, the action will be allowed.

The string set for this property will be outputted as an `[Authorize(Roles="RolesString")]` attribute on generated API controller actions.

<Prop def="public SecurityPermissionLevels PermissionLevel { get; set; }" ctor=2 /> 

The level of access to allow for the action for **class-level security** only. Has no effect for property-level security.

Enum values are:
- `SecurityPermissionLevels.AllowAll` Allow all users to perform the action for the attribute, including users who are not authenticated at all.
- `SecurityPermissionLevels.AllowAuthorized` Allow only users who are members of the roles specified on the attribute to perform the action. If no roles are specified on the attribute, then all authenticated users are allowed (no anonymous access). 
- `SecurityPermissionLevels.DenyAll` Deny the action to all users, regardless of authentication status or authorization level. If `DenyAll` is used, no API endpoint for the action will be generated.
