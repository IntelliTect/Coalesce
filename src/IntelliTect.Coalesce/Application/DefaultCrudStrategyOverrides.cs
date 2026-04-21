using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce;

/// <summary>
/// Stores implementation type overrides for default CRUD strategies
/// when the implementation type has a different number of generic parameters
/// than the service type (partially constructed generics).
/// </summary>
public class DefaultCrudStrategyOverrides
{
    private readonly Dictionary<Type, Type> _overrides = new();

    public void Set(Type serviceType, Type implementationType)
    {
        _overrides[serviceType] = implementationType;
    }

    public bool TryGet(Type serviceType, out Type implementationType)
    {
        return _overrides.TryGetValue(serviceType, out implementationType!);
    }

    /// <summary>
    /// Given an open implementation type with different arity than the service type,
    /// constructs the properly closed implementation type using the service type arguments.
    /// </summary>
    public static Type CloseImplementationType(Type openImplType, Type openServiceType, Type[] serviceTypeArgs)
    {
        if (!openImplType.IsGenericTypeDefinition)
        {
            return openImplType;
        }

        var implGenericArgs = openImplType.GetGenericArguments();

        if (implGenericArgs.Length == serviceTypeArgs.Length)
        {
            // Arity matches, simple case
            return openImplType.MakeGenericType(serviceTypeArgs);
        }

        // Find the interface or base class on openImplType that matches openServiceType
        Type? matchingBase = FindMatchingGenericBase(openImplType, openServiceType);

        if (matchingBase == null)
        {
            throw new InvalidOperationException(
                $"Could not find {openServiceType} in the type hierarchy of {openImplType}.");
        }

        var interfaceArgs = matchingBase.GetGenericArguments();
        var implTypeArgs = new Type[implGenericArgs.Length];

        for (int i = 0; i < interfaceArgs.Length; i++)
        {
            if (interfaceArgs[i].IsGenericParameter)
            {
                implTypeArgs[interfaceArgs[i].GenericParameterPosition] = serviceTypeArgs[i];
            }
        }

        // Verify all impl type args were filled
        for (int i = 0; i < implTypeArgs.Length; i++)
        {
            if (implTypeArgs[i] == null)
            {
                throw new InvalidOperationException(
                    $"Could not determine the type argument for generic parameter " +
                    $"'{implGenericArgs[i].Name}' at position {i} of {openImplType}.");
            }
        }

        return openImplType.MakeGenericType(implTypeArgs);
    }

    private static Type? FindMatchingGenericBase(Type type, Type openGenericType)
    {
        // Check interfaces
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == openGenericType)
                return iface;
        }

        // Check base types
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == openGenericType)
                return baseType;
            baseType = baseType.BaseType;
        }

        return null;
    }
}
