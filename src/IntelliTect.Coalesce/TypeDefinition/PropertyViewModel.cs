using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers.Search;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace IntelliTect.Coalesce.TypeDefinition;

public abstract class PropertyViewModel : ValueViewModel
{
    private const string ConventionalIdSuffix = "Id";

    private protected PropertyViewModel(ClassViewModel effectiveParent, ClassViewModel declaringParent, TypeViewModel type) : base(type)
    {
        Parent = declaringParent;
        EffectiveParent = effectiveParent;
    }

    public abstract string? Comment { get; }

    public abstract bool HasGetter { get; }

    public abstract bool HasSetter { get; }

    public abstract bool HasPublicSetter { get; }

    public abstract bool IsVirtual { get; }

    public abstract bool IsStatic { get; }

    /// <summary>
    /// True if the property has an `init` accessor instead of a `set` accessor.
    /// </summary>
    public abstract bool IsInitOnly { get; }

    /// <summary>
    /// True if new values of the property are only accepted as input 
    /// during a Create /save call, and not during an Update /save call.
    /// </summary>
    public bool IsCreateOnly =>
        // User-determined PKs,
        (Role == PropertyRole.PrimaryKey && DatabaseGenerated == DatabaseGeneratedOption.None) ||
        // And init-only properties
        (IsInitOnly && EffectiveParent.IsDbMappedType);


    /// <summary>
    /// True if the property has the `required` C# language keyword, introduced in C# 11.
    /// </summary>
    public abstract bool HasRequiredKeyword { get; }

    /// <summary>
    /// The reference type nullability state for reads of this property.
    /// </summary>
    public NullabilityState ReadNullability { get; set; }

    /// <summary>
    /// The reference type nullability state for writes to this property.
    /// </summary>
    public NullabilityState WriteNullability { get; set; }

    /// <summary>
    /// Convenient accessor for the PropertyInfo when in reflection-based contexts.
    /// </summary>
    public virtual PropertyInfo PropertyInfo => throw new InvalidOperationException("PropertyInfo not available in the current context");

    /// <summary>
    /// Order rank of the field in the model.
    /// </summary>
    public int ClassFieldOrder { get; internal set; }

    /// <summary>
    /// The class that the property was declared on. 
    /// For the class that is the context in which the property was requested,
    /// use <see cref="EffectiveParent"/>
    /// </summary>
    public ClassViewModel Parent { get; protected set; }

    /// <summary>
    /// The class that is the context in which the property was requested.
    /// Not nessecarily the class that the property is declared on. For that, use <see cref="Parent"/>
    /// </summary>
    public ClassViewModel EffectiveParent { get; protected set; }

    /// <summary>
    /// Name of the property sent by Json over the wire. Camel Cased Name
    /// </summary>
    public string JsonName => Name.ToCamelCase();

    /// <summary>
    /// Returns true if this property has the InternalUse Attribute 
    /// </summary>
    public virtual bool IsInternalUse => this.HasAttribute<InternalUseAttribute>();

    /// <summary>
    /// Returns whether or not the property may be exposed to the client.
    /// </summary>
    public bool IsClientProperty => !IsInternalUse && HasGetter && !Type.IsInternalUse && !IsStatic;

    public bool PureTypeOnContext => Object?.IsDbMappedType ?? false;

    /// <summary>
    /// Text property name for things like enums. PureType+'Text'
    /// </summary>
    public string JsTextPropertyName => JsVariable + "Text";

    /// <summary>
    /// Returns true if the property is class outside the system Namespace, but is not a string or array
    /// </summary>
    public bool IsPOCO => Type.IsPOCO;

    /// <summary>
    /// Gets the ClassViewModel associated with the Object
    /// </summary>
    public ClassViewModel? Object => PureType.ClassViewModel;

    /// <summary>
    /// Returns true if this property is a collection and has the ManyToMany Attribute 
    /// </summary>
    [MemberNotNullWhen(true, nameof(ManyToManyFarNavigationProperty))]
    public bool IsManyToManyCollection => Type.IsCollection && this.HasAttribute<ManyToManyAttribute>();

    /// <summary>
    /// Returns the name of the collection to map as a direct many-to-many collection
    /// </summary>
    public string? ManyToManyCollectionName => this.GetAttributeValue<ManyToManyAttribute>(a => a.CollectionName);

    /// <summary>
    /// Property on the other side of the many-to-many relationship.
    /// </summary>
    [Obsolete("Use better-named property `ManyToManyFarNavigationProperty`.")]
    public PropertyViewModel? ManyToManyCollectionProperty => ManyToManyFarNavigationProperty;

    /// <summary>
    /// Property on the far side of the many-to-many relationship.
    /// </summary>
    public PropertyViewModel? ManyToManyFarNavigationProperty
    {
        get
        {
            if (!IsManyToManyCollection || Object is null) return null;

            string? propName = this.GetAttributeValue<ManyToManyAttribute>(a => a.FarNavigationProperty);
            if (propName != null && propName == ManyToManyNearNavigationProperty?.Name)
            {
                throw new CoalesceModelException(
                    "ManyToManyAttribute.FarNavigationProperty is referencing the near side of " +
                    "the many-to-many relationship, which is not allowed. " +
                    "To configure the near side of the many-to-many relationship, use [InverseProperty].");
            }

            var candidates = Object.ClientProperties.Where(p =>
                p.Role == PropertyRole.ReferenceNavigation &&
                propName is null
                    ? (
                        !p.Equals(ManyToManyNearNavigationProperty) &&
                        // Only writable nav/fk props are a reasonable guess,
                        // since otherwise it would not be possible to actually use it on the client.
                        p.IsClientWritable
                    )
                    : propName == p.Name
                ).ToList();

            if (candidates.Count > 1)
            {
                throw new CoalesceModelException(
                    "Found more than one candidate far navigation property for [ManyToMany] collection. To fix this, set ManyToManyAttribute.FarNavigationProperty to the name of one of these candidate properties: " +
                    string.Concat(candidates.Select(p => $"\n    {Object}.{p.Name}")));
            }

            return candidates.FirstOrDefault();
        }
    }



    /// <summary>
    /// Property on the near side of the many-to-many relationship.
    /// </summary>
    public PropertyViewModel? ManyToManyNearNavigationProperty => InverseProperty;

    /// <summary>
    /// True if the property is read only.
    /// <para/>
    /// Essentially the inverse of <see cref="IsClientWritable"/>.
    /// </summary>
    public bool IsReadOnly => !IsClientWritable && HasGetter;

    /// <summary>
    /// True if the property can be sent from the client to the server
    /// using the standard, generated DTOs.
    /// <para/>
    /// This includes normal, writable value properties, as well as primary keys.
    /// </summary>
    public bool IsClientSerializable => SecurityInfo.Init.IsAllowed() || SecurityInfo.Edit.IsAllowed();

    /// <summary>
    /// True if the value of the property can be modified by the client in a persistable way.
    /// <para/>
    /// This effectively means that when a change is made to the property
    /// on the client, a `Save` call should be made to persist that change to the database.
    /// <para/>
    /// This does not mean that the property's value will be serialized to a DTO,
    /// but instead that a change to the property may change the serialized object.
    /// For example, modification of reference navigation properties will change a foreign key value,
    /// and modification of collection navigation properties may delete or create a new object.
    /// </summary>
    public bool IsClientWritable => this switch
    {
        { Role: PropertyRole.ReferenceNavigation } => ForeignKeyProperty!.IsClientWritable,
        { Role: PropertyRole.CollectionNavigation } => InverseProperty?.IsClientWritable == true,
        { IsAutoGeneratedPrimaryKey: true } => false,
        _ => IsClientSerializable,
    };

    /// <summary>
    /// True if the property has the DateType(DateOnly) Attribute.
    /// </summary>
    [Obsolete("Documentation comment does not match behavior. Use `DateType` directly.")]
    public bool IsDateOnly => DateType == DateTypeAttribute.DateTypes.DateOnly;

    /// <summary>
    /// Returns the default value specified by <see cref="DefaultValueAttribute"/>, if present.
    /// </summary>
    public LiteralViewModel? DefaultValue => this.GetAttributeValue<DefaultValueAttribute>(nameof(DefaultValueAttribute.Value)) is { } defaultValue
        ? new(Type, defaultValue)
        : null;

    /// <summary>
    /// If true, there is an API controller that is serving this type of data.
    /// </summary>
    public bool HasValidValues => IsManyToManyCollection || ((Object?.IsDbMappedType ?? false) && IsPOCO);

    /// <summary>
    /// For the specified area, returns true if the property has a hidden attribute.
    /// </summary>
    public bool IsHidden(HiddenAttribute.Areas area) => HiddenAreas.HasFlag(area);

    public HiddenAttribute.Areas HiddenAreas
    {
        get
        {
            if (IsInternalUse)
            {
                throw new InvalidOperationException("Cannot evaluate the hidden state of an InternalUse prop.");
            }

            if (this.GetAttributeValue<HiddenAttribute, HiddenAttribute.Areas>(a => a.Area) is HiddenAttribute.Areas value)
            {
                // Take the attribute value first to allow for overrides of the default behavior below
                return value;
            }

            if (IsForeignKey && ReferenceNavigationProperty != null)
            {
                // If the prop is a FK that has a reference navigation,
                // hide the FK so that the reference navigation will be all that's shown.
                return HiddenAttribute.Areas.All;
            }

            if (IsPrimaryKey && DatabaseGenerated == DatabaseGeneratedOption.Identity)
            {
                return HiddenAttribute.Areas.All;
            }

            return HiddenAttribute.Areas.None;
        }
    }

    /// <summary>
    /// True if the property should generate a "required" validation rule.
    /// </summary>
    public override bool IsRequired
    {
        get
        {
            if (base.IsRequired) return true;

            if (IsPrimaryKey)
            {
                if (IsAutoGeneratedPrimaryKey)
                {
                    // Db-generated PKs are created by the DB and therefore cannot be required from a user,
                    // regardless of their `init` keyword or C# reference nullability (e.g. a non-nullable
                    // string PK that is initialized with `null!`)
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (!IsClientSerializable)
            {
                // Properties that the client never sends to the server can't be required.
                // This avoids creating bogus `required` rules for things like non-nullable navigation properties.
                return false;
            }

            if (HasRequiredKeyword) return true;

            // Non-nullable foreign keys and their corresponding objects are implicitly required.
            if (IsForeignKey && !Type.IsReferenceOrNullableValue)
            {
                return true;
            }

            // If C# reference nullable types is on and this type is not nullable, then it is implicitly required.
            if (Type.IsReferenceType && WriteNullability == NullabilityState.NotNull)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Returns true if this property is marked with the Search attribute.
    /// </summary>
    public bool IsSearchable(ClassViewModel? rootModel)
    {
        if (!this.HasAttribute<SearchAttribute>()) return false;

        var rootModelName = rootModel?.Name;
        if (rootModelName == null) return true;

        var whitelist = this.GetAttributeValue<SearchAttribute>(a => a.RootWhitelist);
        if (!string.IsNullOrWhiteSpace(whitelist))
        {
            return whitelist.Split(',').Contains(rootModelName);
        }

        var blacklist = this.GetAttributeValue<SearchAttribute>(a => a.RootBlacklist);
        if (!string.IsNullOrWhiteSpace(blacklist))
        {
            return !blacklist.Split(',').Contains(rootModelName);
        }

        return true;
    }

    /// <summary>
    /// How to search for the string when this is a search property.
    /// </summary>
    public SearchAttribute.SearchMethods SearchMethod =>
        this.GetAttributeValue<SearchAttribute, SearchAttribute.SearchMethods>(a => a.SearchMethod)
        ?? SearchAttribute.SearchMethods.BeginsWith;

    /// <summary>
    /// True if the search term should be split on spaces and evaluated individually with or.
    /// </summary>
    public bool SearchIsSplitOnSpaces =>
        // If no attribute (i.e. this is an implicit search on Name),
        // don't split on spaces because that produces weird behavior.
        // See test Search_SingleDefaultString_SearchesCorrectly that breaks without doing this.
        this.GetAttribute<SearchAttribute>() is not { } searchAttr ? false :
        searchAttr.GetValue(a => a.IsSplitOnSpaces) ?? true;

    /// <summary>
    /// Returns the fields to search for this object. This could be just the field itself 
    /// or a number of child fields if this is an object or collection.
    /// </summary>
    public IEnumerable<SearchableProperty> SearchProperties(ClassViewModel? rootModel, int depth = 0, int maxDepth = 2, bool force = false)
    {
        if (!force && !IsSearchable(rootModel)) yield break;

        if (this.Object != null)
        {
            // If we will exceed the depth don't try to query on an object.
            if (depth < maxDepth)
            {
                // Remove this item and add the child's search items with a prepended property name
                var childProperties = this.Object.SearchProperties(rootModel, depth + 1, maxDepth);

                if (this.Type.IsCollection)
                {
                    yield return new SearchableCollectionProperty(this, childProperties);
                }
                else
                {
                    yield return new SearchableObjectProperty(this, childProperties);
                }
            }
        }
        else
        {
            yield return new SearchableValueProperty(this);
        }
    }


    /// <summary>
    /// Returns true if this property is the field to be used for list text and marked with the ListText Attribute.
    /// </summary>
    public bool IsListText => this.HasAttribute<ListTextAttribute>();

    /// <summary>
    /// Returns true if this property is a primary or foreign key.
    /// </summary>
    public bool IsId => IsPrimaryKey || IsForeignKey;

    /// <summary>
    /// Returns true if this is the primary key for this object.
    /// </summary>
    public bool IsPrimaryKey
    {
        get
        {
            // EffectiveParent used here because primary keys may be declared on a base class (as they are with AspNetCore.Identity).
            if (!EffectiveParent.IsDbMappedType && !Parent.IsCustomDto && !Parent.IsStandaloneEntity)
                return false;
            if (this.HasAttribute<KeyAttribute>())
                return true;
            else if (string.Equals(Name, ConventionalIdSuffix, StringComparison.OrdinalIgnoreCase))
                return true;
            else if (string.Equals(Name, Parent.Name + ConventionalIdSuffix, StringComparison.OrdinalIgnoreCase))
                return true;
            else if (string.Equals(Name, Parent.DtoBaseViewModel?.PrimaryKey?.Name, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;

        }
    }

    public bool IsAutoGeneratedPrimaryKey => IsPrimaryKey && DatabaseGenerated == DatabaseGeneratedOption.Identity;


    private LazyValue<bool> _IsForeignKey;
    /// <summary>
    /// Returns true if this property is a foreign key.
    /// Guarantees that <see cref="ForeignKeyPrincipalType"/> is not null.
    /// </summary>
    public bool IsForeignKey => _IsForeignKey.GetValue(() =>
        ForeignKeyPrincipalType != null ||
        EffectiveParent.ClientProperties.Any(p => p != this && p.ForeignKeyProperty == this)
    );


    public DatabaseGeneratedOption DatabaseGenerated
    {
        get
        {
            var value = this.GetAttributeValue<DatabaseGeneratedAttribute, DatabaseGeneratedOption>(a => a.DatabaseGeneratedOption);
            if (value.HasValue)
            {
                return value.Value;
            }

            if (IsPrimaryKey)
            {
                if (Type.IsEnum) return DatabaseGeneratedOption.None;

                // If the PK is also an FK, it can't be database generated.
                // This happens for shared-key one-to-one relationships.
                if (this.HasAttribute<ForeignKeyAttribute>()) return DatabaseGeneratedOption.None;

                return DatabaseGeneratedOption.Identity;
            }

            return DatabaseGeneratedOption.None;
        }
    }


    private LazyValue<PropertyViewModel?> _ForeignKeyProperty;
    /// <summary>
    /// If this is a navigation property, returns the property that holds the foreign key. 
    /// </summary>
    public PropertyViewModel? ForeignKeyProperty => _ForeignKeyProperty.GetValue(() =>
    {
        // Types/props that aren't DB mapped don't have properties that have relational meaning.
        // EffectiveParent used here to correctly handle properties on base classes -
        // we need to know that the class that is ultimately used is DB mapped.
        if (!IsDbMapped || !EffectiveParent.IsDbMappedType) return null;

        PropertyViewModel? prop = null;
        if (Type.IsCollection)
        {
            // `this` may be a collection navigation prop

            if (InverseProperty?.ForeignKeyProperty is { } fk) return fk;

            // Handle [ForeignKeyAttribute] on collection navigations
            // for relationships that lack a reference navigation property.
            var name = this.GetAttributeValue<ForeignKeyAttribute>(a => a.Name);
            if (name is not null)
            {
                prop = PureType.ClassViewModel?.PropertyByName(name);
            }
        }
        else if (Type.IsPOCO)
        {
            // `this` may be a reference navigation prop

            var name =
                // Use the foreign key attribute
                this.GetAttributeValue<ForeignKeyAttribute>(a => a.Name)

                // Use the ForeignKey Attribute on the key property if it is there.
                ?? EffectiveParent.Properties.SingleOrDefault(p => Name == p.GetAttributeValue<ForeignKeyAttribute>(a => a.Name))?.Name

                // Look for a property that follows convention.
                ?? Name + ConventionalIdSuffix;

            prop = EffectiveParent.PropertyByName(name);

            if (prop == null)
            {
                // If the other side of the relationship is also a reference navigation,
                // this is a shared-key 1-to-1. If there's otherwise no FK specified, assume that the PK
                // of this type is the FK we're looking for here.
                // See test models "OneToOneParent"/"SharedKeyChild2" for example.
                if (this.HasAttribute<InversePropertyAttribute>())
                {
                    // Only look at `InverseProperty` if explicitly annotated.
                    // Otherwise, this will cause infinite recursion.
                    if (
                        InverseProperty?.Role == PropertyRole.ReferenceNavigation &&
                        // The reference navigation on the other side points at this prop's owner type
                        InverseProperty.Object == this.EffectiveParent &&
                        // The FK on the other side must reference this prop's type's PK.
                        // This is what allows us to think of our own PK as also being a FK
                        // into the other type (even if that's the exact opposite of the relationship in practice).
                        // This is admittedly a bit of a hack.
                        InverseProperty.ForeignKeyProperty?.IsPrimaryKey == true
                    )
                    {
                        return EffectiveParent.PrimaryKey;
                    }
                }
                else
                {
                    // Handle the similar scenario as above, but when InversePropertyAttribute
                    // is not present. See "OneToOneParent"/"SharedKeyChild1".
                    if (Object!.ClientProperties.Any(p =>
                        p.ForeignKeyProperty == Object.PrimaryKey &&
                        p.Type == this.EffectiveParent.Type
                    ))
                    {
                        return EffectiveParent.PrimaryKey;
                    }
                }
            }
        }

        if (prop == null || !prop.Type.IsValidKeyType || !prop.IsDbMapped)
        {
            return null;
        }

        return prop;
    });


    /// <summary>
    /// If this is a foreign key property, returns the property that holds the reference navigation.
    /// </summary>
    public PropertyViewModel? ReferenceNavigationProperty
    {
        get
        {
            // ReferenceNavigationProperty only has meaning on foreign key props.
            // Eliminate out anything that can't be a key right away.
            if (!Type.IsValidKeyType) return null;

            // Types/props that aren't DB mapped don't have properties that have relational meaning.
            // EffectiveParent used here to correctly handle properties on base classes -
            // we need to know that the class that is ultimately used is DB mapped.
            if (!IsDbMapped || !EffectiveParent.IsDbMappedType) return null;

            var name =
                // Use the ForeignKey Attribute if it is there.
                this.GetAttributeValue<ForeignKeyAttribute>(a => a.Name)

                // Use the ForeignKey Attribute on the object property if it is there.
                ?? EffectiveParent.Properties.FirstOrDefault(p => Name == p.GetAttributeValue<ForeignKeyAttribute>(a => a.Name))?.Name

                // Else, by convention remove the Id at the end.
                ?? (Name.EndsWith(ConventionalIdSuffix) ? Name.Substring(0, Name.Length - ConventionalIdSuffix.Length) : null);

            var prop = EffectiveParent.PropertyByName(name);
            if (prop == null || !prop.IsPOCO || !prop.IsDbMapped)
            {
                return null;
            }

            return prop;
        }
    }

    /// <summary>
    /// If this is a foreign key property, returns the type of the principal entity.
    /// </summary>
    public ClassViewModel? ForeignKeyPrincipalType
    {
        get
        {
            // Eliminate out anything that can't be a key right away.
            if (!Type.IsValidKeyType) return null;

            // Types/props that aren't DB mapped don't have properties that have relational meaning.
            // EffectiveParent used here to correctly handle properties on base classes -
            // we need to know that the class that is ultimately used is DB mapped.
            if (!IsDbMapped || !EffectiveParent.IsDbMappedType) return null;

            if (ReferenceNavigationProperty?.Object is { } navPropType) return navPropType;

            // Support foreign keys without a reference navigation property.
            // These are configured by putting [ForeignKeyAttribute] on the
            // collection navigation on the other side of the relationship.
            return Parent.Usages
                .OfType<PropertyViewModel>()
                .FirstOrDefault(p =>
                    p.Type.IsCollection &&
                    p.GetAttributeValue<ForeignKeyAttribute>(a => a.Name) == this.Name &&
                    p.EffectiveParent.IsDbMappedType
                )
                ?.EffectiveParent;
        }
    }


    public string EditorOrder
    {
        get
        {
            int order = 10000;
            var value = this.GetAttributeValue<DisplayAttribute, int>(a => a.Order);
            if (value != null) order = value.Value;
            // Format them to be sorted.
            return string.Format($"{order:D7}:{ClassFieldOrder:D3}");
            //return string.Format("{0:D7}:{1}", order, Name);
        }
    }


    public OrderByInformation? DefaultOrderBy
    {
        get
        {
            var order = this.GetAttributeValue<DefaultOrderByAttribute, int>(a => a.FieldOrder);
            var direction = this.GetAttributeValue<DefaultOrderByAttribute, DefaultOrderByAttribute.OrderByDirections>(nameof(DefaultOrderByAttribute.OrderByDirection));
            var fieldName = this.GetAttributeValue<DefaultOrderByAttribute>(a => a.FieldName);
            var suppress = this.GetAttributeValue<DefaultOrderByAttribute, bool>(a => a.Suppress);

            // If Suppress is true, this property should not be used for ordering at all
            if (suppress == true)
            {
                return null;
            }

            if (order != null && direction != null)
            {
                var name = Name;
                if (fieldName != null)
                {
                    // TODO: What if fieldName is a dotted, chained property expression?
                    // TODO: What if fieldName refers to a nested POCO?
                    var childField = Type.ClassViewModel?.PropertyByName(fieldName);
                    if (childField != null)
                    {
                        return new OrderByInformation()
                        {
                            Properties = [this, childField],
                            FieldOrder = order.Value,
                            OrderByDirection = direction.Value
                        };
                    }
                }

                return new OrderByInformation()
                {
                    Properties = [this],
                    FieldOrder = order.Value,
                    OrderByDirection = direction.Value
                };
            }
            return null;
        }
    }


    private LazyValue<PropertyViewModel?> _InverseProperty;
    /// <summary>
    /// <para>
    /// If this property is a collection navigation property (the "many"), 
    /// returns the reference navigation property on the collected type that represents the 
    /// "one" end of the one-to-many relationship.
    /// </para>
    /// <para>
    /// If this property is a reference navigation property (the "one"),
    /// returns the collection navigation property on the referenced type that represents the
    /// "many" end of the one-to-many relationship.
    /// </para>
    /// </summary>
    /// 
    /// <summary>
    /// If this is a navigation property, returns the property that holds the foreign key. 
    /// </summary>
    public PropertyViewModel? InverseProperty => _InverseProperty.GetValue(() =>
    {
        if (Object == null || HasNotMapped)
        {
            return null;
        }

        var name = this.GetAttributeValue<InversePropertyAttribute>(a => a.Property);
        if (name != null)
        {
            return Object.PropertyByName(name);
        }

        if (Type.IsCollection)
        {
            // If this prop is a collection, look for a property on the collected type
            // whose name is the same as the type name of this prop's owner.
            // This serves to pick up the standard convention of props like
            // public Widget Widget { get; set; }
            return Object.ClientProperties.FirstOrDefault(p =>
                p.Type == this.Parent.Type
                && p.Name == Parent.Name
                && p.Role == PropertyRole.ReferenceNavigation
            );
        }
        else if (Role == PropertyRole.ReferenceNavigation)
        {
            // Try to find the inverse by looking for a collection on the
            // referenced type of this property whose inverse is this property.
            return Object.ClientProperties.FirstOrDefault(p =>
                p.Role == PropertyRole.CollectionNavigation
                && p.InverseProperty == this
            )
            // Or, try to find 1-to-1 reference navigations
            ?? Object.ClientProperties.FirstOrDefault(p =>
                p.Role == PropertyRole.ReferenceNavigation &&
                p.Type == EffectiveParent.Type &&
                p.ForeignKeyProperty?.IsPrimaryKey == true

            );
        }

        return null;
    });


    /// <summary>
    /// For a collection navigation property, this is the ID reference to this object from the contained object.
    /// </summary>
    public PropertyViewModel? InverseIdProperty
    {
        get
        {
            var inverseProperty = InverseProperty;
            if (inverseProperty != null)
            {
                return inverseProperty.ForeignKeyProperty;
            }
            return null;
        }
    }

    public override string ToString() => $"{Name} ({Type.FullyQualifiedName})";

    private PropertySecurityInfo? _securityInfo;
    public PropertySecurityInfo SecurityInfo => _securityInfo ??= new PropertySecurityInfo(this);

    public bool CanAutoInclude
    {
        get
        {
            if (Role is not PropertyRole.ReferenceNavigation and not PropertyRole.CollectionNavigation)
            {
                return false;
            }

            if (PureType.GetAttributeValue<ReadAttribute, bool>(a => a.NoAutoInclude) == true)
            {
                return false;
            }

            if (this.GetAttributeValue<ReadAttribute, bool>(a => a.NoAutoInclude) == true)
            {
                return false;
            }

            if (PureType.Assembly.GetAttributeValue<CoalesceConfigurationAttribute, bool>(a => a.NoAutoInclude) == true)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Has the NotMapped attribute.
    /// </summary>
    public bool HasNotMapped => this.HasAttribute<NotMappedAttribute>();

    public bool IsDbMapped =>
        !HasNotMapped &&
        // Collection navigation properties are allowed to be getter-only
        (HasSetter || (Type.IsCollection && PureType.IsPOCO)) &&
        (Object?.IsDbMappedType ?? true);

    /// <summary>
    /// If true, this property should be filterable on the URL line via "filter.{UrlParameterName}. 
    /// </summary>
    public bool IsUrlFilterParameter =>
        IsClientProperty && !HasNotMapped && (
            Type.IsPrimitive ||
            Type.IsValidKeyType ||
            (Type.IsCollection && PureType is { } pure && (
                pure.IsPrimitive ||
                pure.IsValidKeyType
            )));


    /// <summary>
    /// Returns a list of content views from the Includes attribute
    /// </summary>
    public IEnumerable<string> DtoIncludes =>
        (this.GetAttributeValue<DtoIncludesAttribute>(a => a.ContentViews) ?? "")
        .Trim()
        .Split(',')
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s));

    /// <summary>
    /// Returns a list of content views from the Excludes attribute
    /// </summary>
    public IEnumerable<string> DtoExcludes =>
        (this.GetAttributeValue<DtoExcludesAttribute>(a => a.ContentViews) ?? "")
        .Trim()
        .Split(',')
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s));

    /// <summary>
    /// Returns the role the property plays in a relational model.
    /// </summary>
    public PropertyRole Role
    {
        get
        {
            if (IsPrimaryKey)
            {
                return PropertyRole.PrimaryKey;
            }
            else if (IsForeignKey)
            {
                return PropertyRole.ForeignKey;
            }

            var obj = Object;
            if (IsDbMapped && EffectiveParent.IsDbMappedType)
            {
                if (Type.IsCollection &&
                    (InverseProperty != null || this.HasAttribute<ForeignKeyAttribute>())
                )
                {
                    return PropertyRole.CollectionNavigation;
                }
                else if (ForeignKeyProperty != null)
                {
                    return PropertyRole.ReferenceNavigation;
                }
            }

            return PropertyRole.Value;
        }
    }

    internal void ClearEntityUsageCache()
    {
        _ForeignKeyProperty.Reset();
        _InverseProperty.Reset();
        _IsForeignKey.Reset();
    }
}
