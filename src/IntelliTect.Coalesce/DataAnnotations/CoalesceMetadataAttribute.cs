using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// <para>
/// When placed on an assembly, specifies an attribute type whose values should be
/// extracted from any Coalesce-generated types, properties, methods, parameters, and enum members,
/// and emitted into the generated TypeScript metadata.
/// </para>
/// <para>
/// The extracted attribute values will be available in the <c>meta</c> property
/// of the corresponding metadata object in <c>metadata.g.ts</c>.
/// </para>
/// </summary>
/// <example>
/// <code>
/// [assembly: CoalesceMetadata&lt;DisplayFormatAttribute&gt;]
/// [assembly: CoalesceMetadata&lt;MyCustomAttribute&gt;("myCustom")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class CoalesceMetadataAttribute : Attribute
{
    /// <summary>
    /// The attribute type whose values should be extracted and emitted into metadata.
    /// </summary>
    public Type AttributeType { get; }

    /// <summary>
    /// The key name to use in the generated metadata.
    /// If not specified, the attribute type name is used with the "Attribute" suffix removed and camelCased.
    /// </summary>
    public string? Key { get; }

    /// <summary>
    /// Initializes a new instance with the specified attribute type.
    /// The key name in generated metadata will be derived from the type name.
    /// </summary>
    /// <param name="attributeType">The attribute type to extract metadata from.</param>
    public CoalesceMetadataAttribute(Type attributeType)
    {
        AttributeType = attributeType ?? throw new ArgumentNullException(nameof(attributeType));
    }

    /// <summary>
    /// Initializes a new instance with the specified attribute type and explicit key name.
    /// </summary>
    /// <param name="attributeType">The attribute type to extract metadata from.</param>
    /// <param name="key">The key name to use in the generated metadata.</param>
    public CoalesceMetadataAttribute(Type attributeType, string key)
    {
        AttributeType = attributeType ?? throw new ArgumentNullException(nameof(attributeType));
        Key = key;
    }
}

/// <inheritdoc cref="CoalesceMetadataAttribute"/>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CoalesceMetadataAttribute<TAttribute> : CoalesceMetadataAttribute
    where TAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance with the specified attribute type.
    /// The key name in generated metadata will be derived from the type name.
    /// </summary>
    public CoalesceMetadataAttribute() : base(typeof(TAttribute)) { }

    /// <summary>
    /// Initializes a new instance with the specified attribute type and explicit key name.
    /// </summary>
    /// <param name="key">The key name to use in the generated metadata.</param>
    public CoalesceMetadataAttribute(string key) : base(typeof(TAttribute), key) { }
}
