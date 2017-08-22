
Controller
==========

Allows for control over the generated MVC Controllers.

Currently only controls over the API controllers are present, but additional properties may be added in the future.
    

Example Usage
-------------

.. code-block:: c#

    [Controller(ApiRouted = false, ApiControllerSuffix = "Gen", ApiActionsProtected = true)]
    public class Person
    {
        public int PersonId { get; set; }
        
        ...
    }



Properties
----------

    :csharp:`public bool ApiRouted { get; set; } = true;`
        Determines whether or not a :csharp:`[Route]` annotation will be placed on the generated API controller. Set to :csharp:`false` to prevent emission of the :csharp:`[Route]` attribute.

        Use cases include:
            -  Defining your routes through IRouteBuilder in Startup.cs instead
            -  Preventing API controllers from being exposed by default.
            -  Routing to your own custom controller that inherits from the generated API controller in order to implement more granular or complex authorization logic.

    :csharp:`public string ApiControllerName { get; set; } = null;`
        If set, will determine the name of the generated API controller.

        Takes precedence over the value of :csharp:`ApiControllerSuffix`.

    :csharp:`public string ApiControllerSuffix { get; set; } = null;`
        If set, will be appended to the default name of the API controller generated for this model.

        Will be overridden by the value of :csharp:`ApiControllerName` if it is set.

    :csharp:`public bool ApiActionsProtected { get; set; } = false;`
        If true, actions on the generated API controller will have an access modifier of :csharp:`protected` instead of :csharp:`public`.

        In order to consume the generated API controller, you must inherit from the generated controller and override each desired generated action method via hiding (i.e. use :csharp:`public new ...`, not :csharp:`public override ...`).

        .. note::

            If you inherit from the generated API controllers and override their methods without setting :csharp:`ApiActionsProtected = true`, all non-overriden actions from the generated controller will still be exposed as normal.