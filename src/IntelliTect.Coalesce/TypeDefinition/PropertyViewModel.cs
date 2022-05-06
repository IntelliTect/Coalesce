using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System.Text;
using IntelliTect.Coalesce.Helpers.Search;
using System.Linq.Expressions;
using IntelliTect.Coalesce.TypeDefinition.Enums;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class PropertyViewModel : IAttributeProvider, IValueViewModel
    {
        private const string ConventionalIdSuffix = "Id";

        private protected PropertyViewModel(ClassViewModel effectiveParent, ClassViewModel declaringParent, TypeViewModel type)
        {
            Parent = declaringParent;
            EffectiveParent = effectiveParent;
            Type = type;
        }

        /// <summary>
        /// Name of the property
        /// </summary>
        public abstract string Name { get; }

        public TypeViewModel Type { get; protected set; }

        public abstract string Comment { get; }

        public abstract bool HasGetter { get; }

        public abstract bool HasSetter { get; }

        public abstract bool HasPublicSetter { get; }

        public abstract bool IsVirtual { get; }

        public abstract bool IsStatic { get; }

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
        public virtual bool IsInternalUse => HasAttribute<InternalUseAttribute>();

        /// <summary>
        /// Returns whether or not the property may be exposed to the client.
        /// </summary>
        public bool IsClientProperty => !IsInternalUse && HasGetter && !Type.IsInternalUse;

        public TypeViewModel PureType => Type.PureType;
        
        public bool PureTypeOnContext => Object?.IsDbMappedType ?? false;


        public string JsVariable => Name.ToCamelCase();

        public static readonly Regex JsKeywordRegex = new Regex(
            "^(?:do|if|in|for|let|new|try|var|case|else|enum|eval|false|null|this|true" +
            "|void|with|break|catch|class|const|super|throw|while|yield|delete|export|import" +
            "|public|return|static|switch|typeof|default|extends|finally|package|private" +
            "|continue|debugger|function|arguments|interface|protected|implements|instanceof)$");

        /// <summary>
        /// Returns true if the value if JsVariable is a reserved keyword in JavaScript.
        /// </summary>
        public bool JsVariableIsReserved => JsKeywordRegex.IsMatch(JsVariable);

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
        public bool IsManytoManyCollection => Type.IsCollection && HasAttribute<ManyToManyAttribute>();

        /// <summary>
        /// True if the client should save data when there is a ClientValidation error. False is default.
        /// </summary>
        public bool ClientValidationAllowSave =>
            this.GetAttributeValue<ClientValidationAttribute, bool>(a => a.AllowSave) ?? false;

        /// <summary>
        /// Returns the name of the collection to map as a direct many-to-many collection
        /// </summary>
        public string? ManyToManyCollectionName => this.GetAttributeValue<ManyToManyAttribute>(a => a.CollectionName);

        /// <summary>
        /// Property on the other side of the many-to-many relationship.
        /// </summary>
        public PropertyViewModel? ManyToManyCollectionProperty =>
            Object?.ClientProperties.FirstOrDefault(prop => prop.IsPOCO && prop.Object?.Equals(Parent) != true);

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
        public bool IsClientSerializable => SecurityInfo.Edit.IsAllowed();

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
            { Role: PropertyRole.CollectionNavigation } => InverseProperty!.IsClientWritable,
            { IsAutoGeneratedPrimaryKey: true } => false,
            _ => IsClientSerializable,
        };

        /// <summary>
        /// True if the property has the DateType(DateOnly) Attribute.
        /// </summary>
        public bool IsDateOnly
        {
            get
            {
                var dateType = this.GetAttributeValue<DateTypeAttribute, DateTypeAttribute.DateTypes>(a => a.DateType);
                if (dateType != null)
                {
                    return dateType.Value == DateTypeAttribute.DateTypes.DateOnly;
                }
                return false;
            }
        }


        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName =>
            this.GetAttributeValue<DisplayNameAttribute>(a => a.DisplayName) ??
            this.GetAttributeValue<DisplayAttribute>(a => a.Name) ??
            Name.ToProperCase();

        /// <summary>
        /// Returns the description from the DisplayAttribute, if present.
        /// </summary>
        public string? Description => this.GetAttributeValue<DisplayAttribute>(a => a.Description);

        /// <summary>
        /// If true, there is an API controller that is serving this type of data.
        /// </summary>
        public bool HasValidValues => IsManytoManyCollection || ((Object?.IsDbMappedType ?? false) && IsPOCO);

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

                if (IsForeignKey || (IsPrimaryKey && DatabaseGenerated == DatabaseGeneratedOption.Identity))
                {
                    return HiddenAttribute.Areas.All;
                }

                return HiddenAttribute.Areas.None;
            }
        }

        /// <summary>
        /// True if the property has the Required attribute, or if the value type is not nullable.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                if (this.HasAttribute<RequiredAttribute>()) return true;

                // Explicit == false is intentional - if the parameter is missing, GetAttributeValue returns null.
                if (this.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsRequired) == false)
                {
                    return false;
                }

                // Non-nullable foreign keys and their corresponding objects are implicitly required.
                if (IsForeignKey && !Type.IsNullable)
                {
                    return true;
                }

                if (IsPrimaryKey)
                {
                    // Primary keys are required if they aren't auto-generated,
                    // and never required if they are.
                    return !IsAutoGeneratedPrimaryKey;
                }

                // TODO: Figure out how to handle situations where we want to hand back an invalid model because the server side is going to figure things out for us.
                //if (IsId && !IsNullable) return true;
                if (Type.IsCollection)
                {
                    return false;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns the MinLength of the property or null if it doesn't exist.
        /// </summary>
        public int? MinLength => this.GetAttributeValue<MinLengthAttribute, int>(a => a.Length);
        
        /// <summary>
        /// Returns the MaxLength of the property or null if it doesn't exist.
        /// </summary>
        public int? MaxLength => this.GetAttributeValue<MaxLengthAttribute, int>(a => a.Length);
        
        /// <summary>
        /// Returns the range of valid values or null if they don't exist. (min, max)
        /// </summary>
        public Tuple<object, object>? Range
        {
            get
            {
                var min = this.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Minimum));
                var max = this.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Maximum));
                if (min != null && max != null) return new Tuple<object, object>(min, max);
                return null;
            }
        }

        /// <summary>
        /// Returns true if this property is marked with the Search attribute.
        /// </summary>
        public bool IsSearchable(ClassViewModel? rootModel)
        {
            if (!HasAttribute<SearchAttribute>()) return false;

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
        /// Returns the Linq.Dynamic-interpretable string representation of the method invocation that will perform search on the property.
        /// </summary>
        public string SearchMethodCall => SearchMethod switch {
            SearchAttribute.SearchMethods.Contains => @"ToLower().IndexOf((""{0}"").ToLower()) >= 0",
            SearchAttribute.SearchMethods.BeginsWith => @"ToLower().StartsWith((""{0}"").ToLower())",
            SearchAttribute.SearchMethods.Equals => @"ToLower().Equals((""{0}"").ToLower())",
            SearchAttribute.SearchMethods.EqualsNatural => @"Equals(""{0}"")",
            _ => throw new InvalidOperationException()
        };

        /// <summary>
        /// True if the search term should be split on spaces and evaluated individually with or.
        /// </summary>
        public bool SearchIsSplitOnSpaces =>
            this.GetAttributeValue<SearchAttribute, bool>(a => a.IsSplitOnSpaces) ?? false;

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
                if (!EffectiveParent.IsDbMappedType && !Parent.IsDto && !Parent.IsStandaloneEntity)
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

        /// <summary>
        /// Returns true if this property is a foreign key. Guarantees that <see cref="ReferenceNavigationProperty"/> is not null.
        /// </summary>
        public bool IsForeignKey => ReferenceNavigationProperty != null;


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
                    return DatabaseGeneratedOption.Identity;
                }

                return DatabaseGeneratedOption.None;
            }
        }

        /// <summary>
        /// If this is a reference navigation property, returns the property that holds the foreign key. 
        /// </summary>
        public PropertyViewModel? ForeignKeyProperty
        {
            get
            {
                // ForeignKeyProperty only has meaning on reference navigation props.
                // If this prop isn't a POCO, that definitely isn't true.
                if (!Type.IsPOCO) return null;

                // Types/props that aren't DB mapped don't have properties that have relational meaning.
                // EffectiveParent used here to correctly handle properties on base classes -
                // we need to know that the class that is ultimately used is DB mapped.
                if (!IsDbMapped || !EffectiveParent.IsDbMappedType) return null;

                var name =
                    // Use the foreign key attribute
                    this.GetAttributeValue<ForeignKeyAttribute>(a => a.Name)

                    // Use the ForeignKey Attribute on the key property if it is there.
                    ?? EffectiveParent.Properties.SingleOrDefault(p => Name == p.GetAttributeValue<ForeignKeyAttribute>(a => a.Name))?.Name

                    // See if this is a one-to-one using the parent's key
                    // Look up the other object and check the key
                    ?? (Object?.IsOneToOne ?? false ? EffectiveParent.PrimaryKey?.Name : null)

                    // Look for a property that follows convention.
                    ?? Name + ConventionalIdSuffix;

                var prop = EffectiveParent.PropertyByName(name);
                if (prop == null || !prop.Type.IsValidKeyType || !prop.IsDbMapped)
                {
                    return null;
                }

                return prop;
            }
        }

        /// <summary>
        /// Gets the property that is the object reference for this ID property.
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
                    ?? EffectiveParent.Properties.SingleOrDefault(p => Name == p.GetAttributeValue<ForeignKeyAttribute>(a => a.Name))?.Name

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
                                Properties = { this, childField },
                                FieldOrder = order.Value,
                                OrderByDirection = direction.Value
                            };
                        }
                    }

                    return new OrderByInformation()
                    {
                        Properties = { this },
                        FieldOrder = order.Value,
                        OrderByDirection = direction.Value
                    };
                }
                return null;
            }
        }

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
        public PropertyViewModel? InverseProperty
        {
            get
            {
                if (Object == null)
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
                    );
                }

                return null;
            }
        }

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

        /// <summary>
        /// Has the NotMapped attribute.
        /// </summary>
        public bool HasNotMapped => HasAttribute<NotMappedAttribute>();

        public bool IsDbMapped => !HasNotMapped && HasSetter && (Object?.IsDbMappedType ?? true);

        /// <summary>
        /// If true, this property should be filterable on the URL line via "filter.{UrlParameterName}. 
        /// </summary>
        public bool IsUrlFilterParameter =>
            IsClientProperty && !HasNotMapped && (Type.IsPrimitive || Type.IsDate || Type.IsValidKeyType);


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
                if (obj != null && obj.IsDbMappedType)
                {
                    if (Type.IsCollection && obj.PrimaryKey != null && InverseProperty != null)
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

        public abstract object? GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;
    }
}