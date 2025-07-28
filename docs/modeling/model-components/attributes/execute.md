
# [Execute]

`IntelliTect.Coalesce.DataAnnotations.ExecuteAttribute`

Controls various aspects of [Custom Methods](/modeling/model-components/methods.md), including role-based permissions, HTTP behavior, and more.

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

If true, the method's arguments will be cleared after a successful invocation on [admin pages](/stacks/vue/admin-pages.md).

<Prop def="public bool? ValidateAttributes { get; set; }" />

If non-null, overrides the value of [`CoalesceOptions.ValidateAttributesForMethods`](/topics/security.md#attribute-validation) when determining whether to perform automatic server-side validation of the method's parameters.

If validation is performed, the method's parameters will be validated by the server and the method invocation prevented if errors are found.

<Prop def="public HttpMethod HttpMethod { get; set; } = HttpMethod.Post;" />

The HTTP method to use on the generated API Controller.

Enum values are:
- `HttpMethod.Post` Use the POST method.
- `HttpMethod.Get` Use the GET method.
- `HttpMethod.Put` Use the PUT method.
- `HttpMethod.Delete` Use the DELETE method.
- `HttpMethod.Patch` Use the PATCH method.

<Prop def="public string? VaryByProperty { get; set; }" />

For HTTP GET model instance methods, if `VaryByProperty` is set to the name of a property on the parent model class, [ETag headers](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag) based on the value of this property will be used to implement caching. If the client provides a matching `If-None-Match` Header with the request, the method will not be invoked and HTTP Status `304 Not Modified`` will be returned.

Additionally, if the `VaryByProperty` is set to a client-exposed [property](/modeling/model-components/properties.md), the value of the property will be included in the query string when performing API calls to invoke the method. If the query string value matches the current value on the model, a long-term `Cache-Control` header will be set on the response, allowing the client to avoid making future invocations to the same method while the value of the `VaryByProperty` remains the same.

<Prop def="public Type? DataSource { get; set; }" />

Specifies that the targeted model instance method should load the instance it is called on from the specified data source when invoked from an API endpoint. If not defined, the model's default data source is used.