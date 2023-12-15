
# [Execute]

Controls permissions for executing of a static or instance method through the API.

For other security controls, see [Security Attributes](/modeling/model-components/attributes/security-attribute.md).

## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }
    
    [Coalesce, Execute(Roles = "Payroll,HR")]
    public void GiveRaise(int centsPerHour) {
        ...
    }

    ...
}
```

## Properties

<Prop def="public string Roles { get; set; }" />

A comma-separated list of roles which are allowed to execute the method.


<Prop def="public SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthorized;" />

The level of access to allow for the action for the method.

Enum values are:
- `SecurityPermissionLevels.AllowAll` Allow all users to perform the action for the attribute, including users who are not authenticated at all.
- `SecurityPermissionLevels.AllowAuthorized` Allow only users who are members of the roles specified on the attribute to perform the action. If no roles are specified on the attribute, then all authenticated users are allowed (no anonymous access). 
- `SecurityPermissionLevels.DenyAll` Deny the action to all users, regardless of authentication status or authorization level. If `DenyAll` is used, no API endpoint for the action will be generated.


<Prop def="public bool AutoClear { get; set; }" />

If true, the method's arguments will be cleared after a successful invocation on admin pages.

<Prop def="public bool? ValidateAttributes { get; set; }" />

If non-null, overrides the value of [`CoalesceOptions.ValidateAttributesForMethods`](/topics/security.md#validateattributesformethods) when determining whether to perform automatic server-side validation of the method's parameters.

If validation is performed, the method's parameters will be validated by the server and the method invocation prevented if errors are found.
