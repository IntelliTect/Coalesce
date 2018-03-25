﻿using System;
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

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class PropertyViewModel : IAttributeProvider
    {
        /// <summary>
        /// Returns whether or not the property may be exposed to the client.
        /// </summary>
        public bool IsClientProperty => !IsInternalUse && HasGetter;

        public TypeViewModel Type { get; protected set; }

        public abstract string Comment { get; }

        public abstract bool HasGetter { get; }
        public abstract bool HasSetter { get; }

        public abstract bool IsVirtual { get; }

        public abstract bool IsStatic { get; }

        /// <summary>
        /// Convenient accessor for the PropertyInfo when in reflection-based contexts.
        /// </summary>
        public virtual PropertyInfo PropertyInfo => throw new InvalidOperationException("PropertyInfo not available in the current context");

        /// <summary>
        /// Returns true if this property has the InternalUse Attribute 
        /// </summary>
        public virtual bool IsInternalUse => HasAttribute<InternalUseAttribute>();

        /// <summary>
        /// Order rank of the field in the model.
        /// </summary>
        public int ClassFieldOrder { get; internal set; }


        /// <summary>
        /// Name of the property
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Name of the property sent by Json over the wire. Camel Cased Name
        /// </summary>
        public string JsonName => Name.ToCamelCase();
        
        public ClassViewModel Parent { get; protected set; }

        /// <summary>
        /// Gets the type name without any collection around it.
        /// </summary>
        public TypeViewModel PureType => Type.PureType;

        public bool PureTypeOnContext => PureType.ClassViewModel?.OnContext ?? false;

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
        public ClassViewModel Object => PureType.ClassViewModel;
        
        /// <summary>
        /// Returns true if this property is a collection and has the ManyToMany Attribute 
        /// </summary>
        public bool IsManytoManyCollection => Type.IsCollection && HasAttribute<ManyToManyAttribute>();

        /// <summary>
        /// True if this property has the ClientValidation Attribute
        /// </summary>
        public bool HasClientValidation => HasAttribute<ClientValidationAttribute>();

        /// <summary>
        /// True if the client should save data when there is a ClientValidation error. False is default.
        /// </summary>
        public bool ClientValidationAllowSave
        {
            get
            {
                if (!HasClientValidation) return false;
                var allowSave = this.GetAttributeValue<ClientValidationAttribute, bool>(a => a.AllowSave);
                return allowSave.HasValue && allowSave.Value;
            }
        }

        /// <summary>
        /// Returns the name of the collection to map as a direct many-to-many collection
        /// </summary>
        public string ManyToManyCollectionName => this.GetAttributeValue<ManyToManyAttribute>(a => a.CollectionName);

        /// <summary>
        /// Property on the other side of the many-to-many relationship.
        /// </summary>
        public PropertyViewModel ManyToManyCollectionProperty =>
            Object.Properties.FirstOrDefault(prop => prop.IsPOCO && prop.Object.Name != Parent.Name);

        /// <summary>
        /// True if the property is read only.
        /// </summary>
        public bool IsReadOnly => !IsClientWritable && HasGetter;

        /// <summary>
        /// True if the property can be sent from the client to the server.
        /// This includes normal writable properties, as well as primary keys.
        /// </summary>
        public bool IsClientSerializable => (IsClientWritable || IsPrimaryKey) && HasSetter && !IsPOCO && !Type.IsCollection;

        /// <summary>
        /// True if the property can be written.
        /// </summary>
        public bool IsClientWritable => 
            !IsInternalUse 
            && HasSetter 
            // Exclude object properties with setters that aren't DB mapped - 
            // these are probably Owned Types, which we don't currently support editing.
            && (!IsPOCO || Object.HasDbSet) 

            && !IsPrimaryKey 
            && !HasReadOnlyAttribute
            && !HasReadOnlyApiAttribute 
            && !(SecurityInfo.IsRead && !SecurityInfo.IsEdit);
        
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

        public string DateFormat => IsDateOnly ? "M/D/YYYY" : "M/D/YYYY h:mm a";

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName =>
            this.GetAttributeValue<DisplayNameAttribute>(a => a.DisplayName) ??
            this.GetAttributeValue<DisplayAttribute>(a => a.Name) ??
            Regex.Replace(Name, "[A-Z]", " $0").Trim();

        /// <summary>
        /// If true, there is an API controller that is serving this type of data.
        /// </summary>
        public bool HasValidValues => IsManytoManyCollection || ((Object?.OnContext) ?? false && IsPOCO);

        /// <summary>
        /// For the specified area, returns true if the property has a hidden attribute.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool IsHidden(HiddenAttribute.Areas area)
        {
            if (IsId) return true;
            if (IsInternalUse) return true;
            // Check the attribute
            var value = this.GetAttributeValue<HiddenAttribute, HiddenAttribute.Areas>(a => a.Area);
            if (value != null) return value.Value == area || value.Value == HiddenAttribute.Areas.All;
            return false;
        }

        public bool HasReadOnlyAttribute => this.HasAttribute<ReadOnlyAttribute>();

#pragma warning disable CS0618 // Type or member is obsolete
        public bool HasReadOnlyApiAttribute => this.HasAttribute<ReadOnlyApiAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// True if the property has the Required attribute, or if the value type is not nullable.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                if (this.HasAttribute<RequiredAttribute>()) return true;

                // Explicit == false is intentional - if the parameter is missing, GetAttributeValue returns null.
                if (this.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsRequired) == false) return false;

                // Non-nullable foreign keys and their corresponding objects are implicitly required.
                if (IsForeignKey && !Type.IsNullable) return true;

                if (IsPrimaryKey) return false;  // Because it will be created by the server.
                // TODO: Figure out how to handle situations where we want to hand back an invalid model because the server side is going to figure things out for us.
                //if (IsId && !IsNullable) return true;
                if (Type.IsCollection) return false;
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
        public Tuple<object, object> Range
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
        public bool IsSearchable(string rootModelName)
        {
            if (!HasAttribute<SearchAttribute>()) return false;

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
        public string SearchMethodCall => SearchMethod == SearchAttribute.SearchMethods.Contains
            ? @"ToLower().IndexOf((""{0}"").ToLower()) >= 0"
            : @"ToLower().StartsWith((""{0}"").ToLower())";

        /// <summary>
        /// True if the search term should be split on spaces and evaluated individually with or.
        /// </summary>
        public bool SearchIsSplitOnSpaces =>
            this.GetAttributeValue<SearchAttribute, bool>(a => a.IsSplitOnSpaces) ?? false;

        /// <summary>
        /// Returns the fields to search for this object. This could be just the field itself 
        /// or a number of child fields if this is an object or collection.
        /// </summary>
        public IEnumerable<SearchableProperty> SearchProperties(string rootModelName, int depth = 0, int maxDepth = 2, bool force = false)
        {
            if (!force && !IsSearchable(rootModelName)) yield break;

            if (this.PureType.HasClassViewModel)
            {
                // If we will exceed the depth don't try to query on an object.
                if (depth < maxDepth)
                {
                    // Remove this item and add the child's search items with a prepended property name
                    var childProperties = this.Type.PureType.ClassViewModel.SearchProperties(rootModelName, depth + 1, maxDepth);

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
                if (this.HasAttribute<KeyAttribute>())
                    return true;
                else if (string.Equals(Name, "Id", StringComparison.InvariantCultureIgnoreCase))
                    return true;
                else if (string.Equals(Name, Parent.Name + "Id", StringComparison.InvariantCultureIgnoreCase))
                    return true;
                else if (Parent.IsDto && Parent.BaseViewModel != null && string.Equals(Name, Parent.BaseViewModel.PrimaryKey.Name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
                return false;

            }
        }

        /// <summary>
        /// Returns true if this property's name ends with Id.
        /// </summary>
        public bool IsForeignKey
        {
            // TODO: should this be the same as IsPrimaryKey
            get
            {
                if (IsPOCO) return false;
                if (this.HasAttribute<ForeignKeyAttribute>() && !IsPOCO)
                {
                    return true;
                }
                if (this.Name.EndsWith("Id") && !IsPrimaryKey && Parent.PropertyByName(Name.Substring(0, Name.Length - 2)) != null)
                {
                    return true;
                }
                if (Parent.Properties.Any(p => Name == p.GetAttributeValue<ForeignKeyAttribute>(a => a.Name)))
                {
                    // You can also put the attribute on the POCO property and refer to the key property. This detects that.
                    return true;
                }

                return false;
            }
        }



        /// <summary>
        /// If this is an object, returns the name of the property that holds the ID. 
        /// </summary>
        public string ObjectIdPropertyName
        {
            get
            {
                // Use the foreign key attribute
                var value = this.GetAttributeValue<ForeignKeyAttribute>(a => a.Name);
                if (value != null) return value;
                // See if this is a one-to-one using the parent's key
                // Look up the other object and check the key
                var vm = PureType.ClassViewModel;
                if (vm != null)
                {
                    if (vm.IsOneToOne)
                    {
                        return Parent.PrimaryKey.Name;
                    }
                }
                // Look on the Object for the key in case of a commonly keyed one-to-one
                return PureType.Name + "Id";
            }
        }


        /// <summary>
        /// If this is an object, returns the property that holds the ID. 
        /// </summary>
        public PropertyViewModel ObjectIdProperty => Parent.PropertyByName(ObjectIdPropertyName);


        public string IdFieldCollection => PureType + (IsManytoManyCollection ? "Ids" : "Id");

        /// <summary>
        /// Gets the name of the property that this ID property points to.
        /// </summary>
        private string IdPropertyObjectPropertyName
        {
            get
            {
                if (!IsForeignKey) return null;

                // Use the ForeignKey Attribute if it is there.
                var value = this.GetAttributeValue<ForeignKeyAttribute>(a => a.Name);
                if (value != null) return value;

                // Use the ForeignKey Attribute on the object property if it is there.
                var prop = Parent.Properties.SingleOrDefault(p => Name == p.GetAttributeValue<ForeignKeyAttribute>(a => a.Name));
                if (prop != null) return prop.Name;

                // Else, by convention remove the Id at the end.
                return Name.Substring(0, Name.Length - 2);
            }
        }

        /// <summary>
        /// Gets the property that is the object reference for this ID property.
        /// </summary>
        public PropertyViewModel IdPropertyObjectProperty
        {
            get
            {
                if (string.IsNullOrEmpty(IdPropertyObjectPropertyName))
                {
                    return null;
                }
                else
                {
                    return Parent.PropertyByName(IdPropertyObjectPropertyName);
                }
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


        public OrderByInformation DefaultOrderBy
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
                        string defaultValue = Type.ClassViewModel?.PropertyByName(fieldName).Type.CsDefaultValue ?? "\"\"";

                        name = $"({name} == null ? {defaultValue}: {name}.{fieldName})";
                    }
                    return new OrderByInformation() { FieldName = name, FieldOrder = order.Value, OrderByDirection = direction.Value };
                }
                return null;
            }
        }


        /// <summary>
        /// Returns the URL for the List Editor with the ???Id= query string.
        /// Ex: Adult/table?filter.adultId=
        /// </summary>
        public string ListEditorUrl
        {
            get
            {
                if (InverseIdProperty == null) { return null; }
                return string.Format("{0}/Table?filter.{1}=", Object.ControllerName, InverseIdProperty.JsonName);
            }
        }

        /// <summary>
        /// Returns the core URL for the List Editor.
        /// </summary>
        public string ListEditorUrlName => string.Format("{0}ListUrl", JsVariable);

        public bool HasInverseProperty => HasAttribute<InversePropertyAttribute>();

        /// <summary>
        /// For a collection, this is the reference to this object from the contained object.
        /// </summary>
        public PropertyViewModel InverseProperty
        {
            get
            {
                var name = this.GetAttributeValue<InversePropertyAttribute>(a => a.Property);
                if (name != null)
                {
                    var inverseProperty = Object.PropertyByName(name);
                    return inverseProperty;
                }
                else if (Object != null)
                {
                    return Object.PropertyByName(Parent.Name);
                }
                return null;
            }
        }

        /// <summary>
        /// For a many to many collection this is the ID reference to this object from the contained object.
        /// </summary>
        public PropertyViewModel InverseIdProperty
        {
            get
            {
                var inverseProperty = InverseProperty;
                if (inverseProperty != null)
                {
                    return inverseProperty.ObjectIdProperty;
                }
                return null;
            }
        }

        public override string ToString() => $"{Name} : {Type.FullyQualifiedName}";

        public string SecurityToString()
        {

            return $"Read: {SecurityReadToString()}  Edit: {SecurityEditToString()}";
        }

        public string SecurityEditToString()
        {
            if (IsInternalUse) return "None";
            if (SecurityInfo.IsSecuredProperty)
            {
                if (!SecurityInfo.IsEdit) return "None";
                return $"Roles: {SecurityInfo.EditRolesExternal}";
            }
            return "Allow";
        }

        public string SecurityReadToString()
        {
            if (IsInternalUse) return "None";
            if (SecurityInfo.IsSecuredProperty) return $"Roles: {SecurityInfo.ReadRolesExternal}";
            return "Allow";
        }

        public PropertySecurityInfo SecurityInfo
        {
            get
            {
                var result = new PropertySecurityInfo();

                if (HasAttribute<ReadAttribute>())
                {
                    result.IsRead = true;
                    var roles = this.GetAttributeValue<ReadAttribute>(a => a.Roles);
                    result.ReadRoles = roles;
                }

                if (HasAttribute<EditAttribute>())
                {
                    result.IsEdit = true;
                    var roles = this.GetAttributeValue<EditAttribute>(a => a.Roles);
                    result.EditRoles = roles;
                }

                return result;
            }
        }


        /// <summary>
        /// Object is not in the database, but hyndrated via other means.
        /// </summary>
        public bool IsExternal => IsPOCO && ObjectIdProperty != null && HasNotMapped;


        /// <summary>
        /// Has the NotMapped attribute.
        /// </summary>
        public bool HasNotMapped => HasAttribute<NotMappedAttribute>();

        /// <summary>
        /// If true, this property should be filterable on the URL line via "filter.{UrlParameterName}. 
        /// </summary>
        public bool IsUrlFilterParameter => 
            IsClientProperty && !HasNotMapped && (Type.IsPrimitive || Type.IsDate);

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

        public abstract object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;
    }
}