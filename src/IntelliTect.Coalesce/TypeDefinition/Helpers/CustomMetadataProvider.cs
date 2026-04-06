using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.TypeDefinition;

/// <summary>
/// A single custom attribute's extracted metadata: its key and name/value pairs.
/// </summary>
public class CustomMetadataItem
{
    public string Key { get; }
    public IReadOnlyDictionary<string, object?> Properties { get; }

    public CustomMetadataItem(string key, IReadOnlyList<KeyValuePair<string, object?>> values)
    {
        Key = key;
        Properties = values.Count > 0
            ? new Dictionary<string, object?>(values.Select(kv => new KeyValuePair<string, object?>(kv.Key, kv.Value)))
            : new Dictionary<string, object?>();
    }
}

/// <summary>
/// Provides extraction of custom attribute metadata from symbols based on
/// assembly-level <see cref="CoalesceMetadataAttribute"/> declarations.
/// Registrations are tracked per-assembly.
/// </summary>
public class CustomMetadataProvider
{
    // Per-assembly: assemblyIdentity -> (key -> attributeType)
    private readonly Dictionary<object, Dictionary<string, TypeViewModel>> _registrationsByAssembly = new();

    public bool HasEntries => _registrationsByAssembly.Values.Any(d => d.Count > 0);

    /// <summary>
    /// Gets the registered attribute type keys for a specific assembly.
    /// </summary>
    public IReadOnlyDictionary<string, TypeViewModel>? GetRegistrations(object assemblyIdentity)
    {
        return _registrationsByAssembly.GetValueOrDefault(assemblyIdentity);
    }

    /// <summary>
    /// Reads <see cref="CoalesceMetadataAttribute"/> declarations from the given assembly
    /// and adds them as registrations keyed by assembly identity.
    /// </summary>
    public void AddAssembly(object assemblyIdentity, IAttributeProvider assembly)
    {
        if (_registrationsByAssembly.ContainsKey(assemblyIdentity)) return;

        var registrations = new Dictionary<string, TypeViewModel>();

        foreach (var attr in assembly.GetAttributes<CoalesceMetadataAttribute>())
        {
            // For non-generic CoalesceMetadataAttribute, the type is in the constructor arg.
            // For generic CoalesceMetadataAttribute<T>, Roslyn doesn't expose the base ctor arg,
            // so fall back to the first type argument of the attribute class.
            if (attr.GetValue(nameof(CoalesceMetadataAttribute.AttributeType)) is not TypeViewModel typeVm)
            {
                if (attr.Type.FirstTypeArgument is not { } typeArg) continue;
                typeVm = typeArg;
            }

            var key = attr.GetValue(a => a.Key);
            if (string.IsNullOrWhiteSpace(key))
            {
                key = typeVm.Name;
                if (key.EndsWith("Attribute", StringComparison.Ordinal))
                {
                    key = key[..^"Attribute".Length];
                }
                key = key.ToCamelCase();
            }

            registrations.TryAdd(key, typeVm);
        }

        _registrationsByAssembly[assemblyIdentity] = registrations;
    }

    /// <summary>
    /// Extracts all matching custom metadata from the given attribute provider,
    /// using registrations for the assembly containing the specified type.
    /// </summary>
    public IEnumerable<CustomMetadataItem> GetCustomMetadata(
        IAttributeProvider provider, TypeViewModel containingType)
    {
        if (!_registrationsByAssembly.TryGetValue(containingType.AssemblyIdentity, out var regs) || regs.Count == 0)
            yield break;

        foreach (var attr in provider.GetAttributes<Attribute>())
        {
            foreach (var (key, attrType) in regs)
            {
                if (attr.Type.Equals(attrType))
                {
                    yield return new CustomMetadataItem(key, attr.GetAllValues());
                }
            }
        }
    }
}
