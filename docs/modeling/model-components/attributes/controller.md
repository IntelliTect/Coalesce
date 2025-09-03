---
deprecated: true
---

# [Controller]

`IntelliTect.Coalesce.DataAnnotations.ControllerAttribute`

Allows for control over the generated MVC Controllers.

Currently only controls over the API controllers are present, but additional properties may be added in the future.
    
This attribute may be placed on any type from which an API controller is generated, including [Entity Models](/modeling/model-types/entities.md), [Custom DTOs](/modeling/model-types/dtos.md), and [Services](/modeling/model-types/services.md).

## Example Usage

``` c#
[Controller(ApiRouted = false, ApiControllerSuffix = "Gen", ApiActionsProtected = true)]
public class Person
{
    public int PersonId { get; set; }
    
    ...
}
```

## Properties

<Prop def="public bool ApiRouted { get; set; } = true;" />

Determines whether or not a `[Route]` annotation will be placed on the generated API controller. Set to `false` to prevent emission of the `[Route]` attribute.

Use cases include:
- Defining your routes through IRouteBuilder in Startup.cs instead
- Preventing API controllers from being exposed by default
- Routing to your own custom controller that inherits from the generated API controller to implement more granular or complex authorization logic

<Prop def="public string ApiControllerName { get; set; } = null;" />

If set, will determine the name of the generated API controller.

Takes precedence over the value of `ApiControllerSuffix`.

<Prop def="public string ApiControllerSuffix { get; set; } = null;" />

If set, will be appended to the default name of the API controller generated for this model.

Will be overridden by the value of `ApiControllerName` if it is set.

<Prop def="public bool ApiActionsProtected { get; set; } = false;" />

If true, actions on the generated API controller will have an access modifier of `protected` instead of `public`.

In order to consume the generated API controller, you must inherit from the generated controller and override each desired generated action method via hiding (i.e. use `public new ...`, not `public override ...`).

::: tip Note
If you inherit from the generated API controllers and override their methods without setting `ApiActionsProtected = true`, all non-overridden actions from the generated controller will still be exposed as normal.
:::
