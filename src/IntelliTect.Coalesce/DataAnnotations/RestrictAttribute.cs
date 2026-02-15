using System;

namespace IntelliTect.Coalesce.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public sealed class RestrictAttribute<T> : Attribute
    where T : IPropertyRestriction
{
    // Originally, there was a design of this attribute that had the attribute itself implementing 
    // `IPropertyRestriction`, but this design of the attribute itself implementing the functionality
    // presents with a major problem:

    // We'd like to have a `IPropertyRestriction` instance per mapping operation
    // so that instances can be constructed with dependency injection and also cache
    // state for reuse within the same mapping operation. This doesn't work with 
    // attribute instances which are instantiated by the runtime.

    // So, this attribute only functions as a marker attribute for discovery of T.
}
