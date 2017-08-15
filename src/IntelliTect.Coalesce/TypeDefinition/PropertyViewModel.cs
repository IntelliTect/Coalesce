using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System.Text;
using IntelliTect.Coalesce.Helpers.Search;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class PropertyViewModel
    {
        /// <summary>
        /// .net PropertyInfo class that gives reflected information about the property.
        /// </summary>
        internal PropertyWrapper Wrapper { get; }

        internal PropertyViewModel(PropertyWrapper propertyWrapper, ClassViewModel parent, int classFieldOrder)
        {
            Wrapper = propertyWrapper;
            Parent = parent;
            ClassFieldOrder = classFieldOrder;
        }
        public PropertyViewModel(PropertyInfo propertyInfo, ClassViewModel parent, int classFieldOrder)
        {
            Wrapper = new ReflectionPropertyWrapper(propertyInfo);
            Parent = parent;
            ClassFieldOrder = classFieldOrder;
        }

        public PropertyViewModel(IPropertySymbol propertySymbol, ClassViewModel parent, int classFieldOrder)
        {
            Wrapper = new SymbolPropertyWrapper(propertySymbol);
            Parent = parent;
            ClassFieldOrder = classFieldOrder;
        }

        public static implicit operator PropertyViewModel(System.Linq.Expressions.LambdaExpression e)
        {
            return ReflectionRepository.PropertyBySelector(e);
        }

        public TypeViewModel Type => new TypeViewModel(Wrapper.Type);

        /// <summary>
        /// Order rank of the field in the model.
        /// </summary>
        public int ClassFieldOrder { get; }


        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name => Wrapper.Name;

        /// <summary>
        /// Name of the property sent by Json over the wire. Camel Cased Name
        /// </summary>
        public string JsonName => Wrapper.Name.ToCamelCase();

        public string Comment
        {
            get
            {
                return Regex.Replace(Wrapper.Comment, "\n(\\s+)", "\n        // ");
            }
        }
        /// <summary>
        /// Name of the type
        /// </summary>
        public string TypeName => Wrapper.Type.Name;

        public ClassViewModel Parent { get; }

        /// <summary>
        /// Gets the type name without any collection around it.
        /// </summary>
        public TypeViewModel PureType => new TypeViewModel(Wrapper.Type.PureType);

        public bool PureTypeOnContext => PureType.ClassViewModel?.OnContext ?? false;

        /// <summary>
        /// Gets the name for the API call.
        /// </summary>
        public string Api
        {
            get
            {
                if (Wrapper.Type.IsGeneric && Wrapper.Type.IsCollection)
                {
                    if (IsManytoManyCollection)
                    {
                        return Object.ApiUrl;
                    }
                }
                return Object.ApiUrl;
            }
        }

        public string JsVariable => Name.ToCamelCase();

        private static readonly Regex JsKeywordRegex = new Regex(
                "^(?:do|if|in|for|let|new|try|var|case|else|enum|eval|false|null|this|true|void|with|break|catch|class|const|super|throw|while|yield|delete|export|import|public|return|static|switch|typeof|default|extends|finally|package|private|continue|debugger|function|arguments|interface|protected|implements|instanceof)$");

        /// <summary>
        /// Returns true if the value if JsVariable is a reserved keyword in JavaScript.
        /// </summary>
        public bool JsVariableIsReserved => JsKeywordRegex.IsMatch(JsVariable);

        /// <summary>
        /// Returns the correctly-prefixed version of the value of JsVariable for use in Knockout bindings
        /// </summary>
        public string JsVariableForBinding => JsVariableIsReserved ? $"$data.{JsVariable}" : JsVariable;

        /// <summary>
        /// Name of the Valid Value list object in JS in Pascal case.
        /// </summary>
        public string ValidValueListName => Name + "ValidValues";


        /// <summary>
        /// Text property name for knockout, for things like enums. PureType+'Text'
        /// </summary>
        public string JsTextPropertyName => JsVariable + "Text";

        /// <summary>
        /// Text property name for knockout bindings, for things like enums. PureType+'Text'
        /// </summary>
        public string JsTextPropertyNameForBinding => JsVariableForBinding + "Text";


        /// <summary>
        /// Returns true if the property is class outside the system NameSpace, but is not a string, array, or filedownload
        /// </summary>
        public bool IsPOCO
        {
            get
            {
                return !Wrapper.Type.Namespace.StartsWith("System") &&
                  !Wrapper.Type.IsString &&
                  Wrapper.Type.IsClass &&
                  !Wrapper.Type.IsArray &&
                  !Wrapper.Type.IsCollection &&
                  !IsFileDownload;
            }
        }

        public bool IsStatic => Wrapper.IsStatic;

        /// <summary>
        /// Returns true if this property is a complex type.
        /// </summary>
        public bool IsComplexType => IsPOCO && Object.IsComplexType;


        /// <summary>
        /// True if this property has a ViewModel.
        /// </summary>
        public bool HasViewModel => Object != null && !IsInternalUse;

        /// <summary>
        /// Gets the ClassViewModel associated with the Object
        /// </summary>
        public ClassViewModel Object => Wrapper.Type.PureType.ClassViewModel;

        public bool IsDbSet => Type.Name.Contains("DbSet");


        /// <summary>
        /// Returns true if this property is a collection and has the ManyToMany Attribute 
        /// </summary>
        public bool IsManytoManyCollection => Wrapper.Type.IsCollection && Wrapper.HasAttribute<ManyToManyAttribute>();

        /// <summary>
        /// True if this property has the ClientValidation Attribute
        /// </summary>
        public bool HasClientValidation => Wrapper.HasAttribute<ClientValidationAttribute>();

        /// <summary>
        /// True if the client should save data when there is a ClientValidation error. False is default.
        /// </summary>
        public bool ClientValidationAllowSave
        {
            get
            {
                if (!HasClientValidation) return false;
                var allowSave = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.AllowSave)) as bool?;
                return allowSave.HasValue && allowSave.Value;
            }
        }

        /// <summary>
        /// Gets the Knockout JS text for the validation.
        /// </summary>
        public string ClientValidationKnockoutJs
        {
            get
            {

                var isRequired = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsRequired)) as bool?;
                var minValue = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MinValue)) as double?;
                var maxValue = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MaxValue)) as double?;
                var minLength = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MinLength)) as int?;
                var maxLength = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MaxLength)) as int?;
                var pattern = Wrapper.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.Pattern));
                var step = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.Step)) as double?;
                var isEmail = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsEmail)) as bool?;
                var isPhoneUs = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsPhoneUs)) as bool?;
                var equal = Wrapper.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.Equal));
                var notEqual = Wrapper.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.NotEqual));
                var isDate = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsDate)) as bool?;
                var isDateIso = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsDateIso)) as bool?;
                var isNumber = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsNumber)) as bool?;
                var isDigit = Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsDigit)) as bool?;
                var customName = Wrapper.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.CustomName));
                var customValue = Wrapper.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.CustomValue));
                var errorMessage = Wrapper.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.ErrorMessage));


                var validations = new List<string>();

                if (isRequired.HasValue && isRequired.Value)
                {
                    validations.Add($"required: {KoValidationOptions("true", errorMessage ?? $"{(IdPropertyObjectProperty ?? this).DisplayName} is required.")}");
                }
                else if (IsRequired)
                {
                    string message = null;
                    if (Wrapper.HasAttribute<RequiredAttribute>())
                    {
                        message = Wrapper.GetAttributeObject<RequiredAttribute, string>(nameof(RequiredAttribute.ErrorMessage));
                    }
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        var name = (IdPropertyObjectProperty ?? this).DisplayName;
                        message = $"{name} is required.";
                    }

                    validations.Add($"required: {KoValidationOptions("true", message)}");
                }

                if (Type.IsString)
                {
                    if (Range != null)
                    {
                        var message = Wrapper.GetAttributeObject<RangeAttribute, string>(nameof(RangeAttribute.ErrorMessage));
                        validations.Add($"minLength: {KoValidationOptions(Range.Item1.ToString(), message)}, maxLength: {KoValidationOptions(Range.Item2.ToString(), message)}");
                    }
                    else
                    {
                        if (MinLength.HasValue)
                        {
                            var message = Wrapper.GetAttributeObject<MinLengthAttribute, string>(nameof(MinLengthAttribute.ErrorMessage));
                            validations.Add($"minLength: {KoValidationOptions(MinLength.Value.ToString(), message)}");
                        }
                        else if (minLength.HasValue && minLength.Value != int.MaxValue)
                        {
                            validations.Add($"minLength: {KoValidationOptions(minLength.Value.ToString(), errorMessage)}");
                        }

                        if (MaxLength.HasValue)
                        {
                            var message = Wrapper.GetAttributeObject<MaxLengthAttribute, string>(nameof(MaxLengthAttribute.ErrorMessage));
                            validations.Add($"maxLength: {KoValidationOptions(MaxLength.Value.ToString(), message)}");
                        }
                        else if (maxLength.HasValue && maxLength.Value != int.MinValue)
                        {
                            validations.Add($"maxLength: {KoValidationOptions(maxLength.Value.ToString(), errorMessage)}");
                        }
                    }
                }
                else if (Type.IsNumber)
                {
                    if (Range != null)
                    {
                        var message = Wrapper.GetAttributeObject<RangeAttribute, string>(nameof(RangeAttribute.ErrorMessage));
                        validations.Add($"min: {KoValidationOptions(Range.Item1.ToString(), message)}, max: {KoValidationOptions(Range.Item2.ToString(), message)}");
                    }
                    else
                    {
                        if (minValue.HasValue && minValue.Value != double.MaxValue)
                            validations.Add($"min: {KoValidationOptions(minValue.Value.ToString(), errorMessage)}");
                        if (maxValue.HasValue && maxValue.Value != double.MinValue)
                            validations.Add($"max: {KoValidationOptions(maxValue.Value.ToString(), errorMessage)}");
                    }
                }

                if (pattern != null)
                    validations.Add($"pattern: {KoValidationOptions($"'{pattern}'", errorMessage)}");
                if (step.HasValue && step.Value != 0)
                    validations.Add($"step: {KoValidationOptions($"{step.Value}", errorMessage)}");
                if (isEmail.HasValue && isEmail.Value)
                    validations.Add($"email: {KoValidationOptions("true", errorMessage)}");
                if (isPhoneUs.HasValue && isPhoneUs.Value)
                    validations.Add($"phoneUS: {KoValidationOptions("true", errorMessage)}");
                if (equal != null)
                    validations.Add($"equal: {KoValidationOptions($"{equal}", errorMessage)}");
                if (notEqual != null)
                    validations.Add($"notEqual: {KoValidationOptions($"{notEqual}", errorMessage)}");
                if (isDate.HasValue && isDate.Value)
                    validations.Add($"isDate: {KoValidationOptions("true", errorMessage)}");
                if (isDateIso.HasValue && isDateIso.Value)
                    validations.Add($"isDateISO: {KoValidationOptions("true", errorMessage)}");
                if (isNumber.HasValue && isNumber.Value)
                    validations.Add($"isNumber: {KoValidationOptions("true", errorMessage)}");
                if (isDigit.HasValue && isDigit.Value)
                    validations.Add($"isDigit: {KoValidationOptions("true", errorMessage)}");
                if (!string.IsNullOrWhiteSpace(customName) && !string.IsNullOrWhiteSpace(customValue))
                    validations.Add($"{customName}: {customValue}");

                return string.Join(", ", validations);
            }
        }

        private string AddErrorMessage(string errorMessage)
        {
            string message = null;

            if (!string.IsNullOrWhiteSpace(errorMessage)) message = $", message: \"{errorMessage}\"";

            return message;
        }

        private string KoValidationOptions(string value, string errorMessage)
        {
            string message = AddErrorMessage(errorMessage);
            if (!string.IsNullOrWhiteSpace(message))
            {
                return $"{{params: {value}, message: \"{errorMessage}\"}}";
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Returns the name of the collection to map as a direct many-to-many collection
        /// </summary>
        public string ManyToManyCollectionName => Wrapper.GetAttributeValue<ManyToManyAttribute>(nameof(ManyToManyAttribute.CollectionName)) as string;

        /// <summary>
        /// Property on the other side of the many-to-many relationship.
        /// </summary>
        public PropertyViewModel ManyToManyCollectionProperty
        {
            get
            {
                foreach (var prop in Object.Properties)
                {
                    if (prop.IsPOCO && prop.Object.Name != Parent.Name)
                    {
                        return prop;
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// Returns true if this property has the InternalUse Attribute 
        /// </summary>
        public bool IsInternalUse => Wrapper.IsInternalUse;

        /// <summary>
        /// Returns true if this property has the FileDownload Attribute.
        /// </summary>
        public bool IsFileDownload => Wrapper.HasAttribute<FileDownloadAttribute>();

        /// <summary>
        /// Only available on reflected types, not during Roslyn compile.
        /// </summary>
        public PropertyInfo PropertyInfo => Wrapper.PropertyInfo;

        /// <summary>
        /// True if the property is read only.
        /// </summary>
        public bool IsReadOnly => !CanWrite && CanRead;

        /// <summary>
        /// True if the property can be written.
        /// </summary>
        public bool CanWrite => Wrapper.HasSetter && !IsPrimaryKey && !HasReadOnlyAttribute && !HasReadOnlyApiAttribute && !(SecurityInfo.IsRead && !SecurityInfo.IsEdit);

        /// <summary>
        /// True if the property can be read.
        /// </summary>
        public bool CanRead => Wrapper.HasGetter;


        /// <summary>
        /// True if the property has the DateType(DateOnly) Attribute.
        /// </summary>
        public bool IsDateOnly
        {
            get
            {
                var dateType = Wrapper.GetAttributeValue<DateTypeAttribute, DateTypeAttribute.DateTypes>(nameof(DateTypeAttribute.DateType));
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
        public string DisplayName
        {
            get
            {
                var displayName = Wrapper.GetAttributeValue<DisplayNameAttribute>(nameof(DisplayNameAttribute.DisplayName)) as string;
                if (displayName != null) return displayName;
                displayName = Wrapper.GetAttributeValue<DisplayAttribute>(nameof(DisplayAttribute.Name)) as string;
                if (displayName != null) return displayName;
                else return Regex.Replace(Name, "[A-Z]", " $0").Trim();
            }
        }

        public string DisplayNameLabel(string labelOverride) => labelOverride ?? DisplayName;

        /// <summary>
        /// If true, there is an API controller that is serving this type of data.
        /// </summary>
        public bool HasValidValues => IsManytoManyCollection || ApiController != null;

        /// <summary>
        /// If this is an object, the name of the API controller serving this data. Or null if none.
        /// </summary>
        public string ApiController
        {
            get
            {
                if (Object == null || !Object.OnContext) return null;

                if (IsPOCO && !IsComplexType) return Name;

                return null;
            }
        }

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
            var value = Wrapper.GetAttributeValue<HiddenAttribute, HiddenAttribute.Areas>(nameof(HiddenAttribute.Area));
            if (value != null) return value.Value == area || value.Value == HiddenAttribute.Areas.All;
            return false;
        }

        public string ListGroup => Wrapper.GetAttributeObject<ListGroupAttribute, string>(nameof(ListGroupAttribute.Group));

        public bool HasReadOnlyAttribute => Wrapper.HasAttribute<ReadOnlyAttribute>();

#pragma warning disable CS0618 // Type or member is obsolete
        public bool HasReadOnlyApiAttribute => Wrapper.HasAttribute<ReadOnlyApiAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// True if the property has the Required attribute, or if the value type is not nullable.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                var value = Wrapper.HasAttribute<RequiredAttribute>();
                if (value) return true;

                value = Wrapper.HasAttribute<ClientValidationAttribute>();
                if (value && Wrapper.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsRequired)) as bool? == false) return false;

                // Non-nullable foreign keys and their corresponding objects are implicitly required.
                if (IsForeignKey && !Type.IsNullable) return true;

                if (IsPrimaryKey) return false;  // Because it will be created by the server.
                // TODO: Figure out how to handle situations where we want to hand back an invalid model because the server side is going to figure things out for us.
                //if (IsId && !IsNullable) return true;
                if (Wrapper.Type.IsCollection) return false;
                return false;
            }
        }

        /// <summary>
        /// Returns the MinLength of the property or null if it doesn't exist.
        /// </summary>
        public int? MinLength => Wrapper.GetAttributeValue<MinLengthAttribute, int>(nameof(MinLengthAttribute.Length));
        
        /// <summary>
        /// Returns the MaxLength of the property or null if it doesn't exist.
        /// </summary>
        public int? MaxLength => Wrapper.GetAttributeValue<MaxLengthAttribute, int>(nameof(MaxLengthAttribute.Length));
        
        /// <summary>
        /// Returns the range of valid values or null if they don't exist. (min, max)
        /// </summary>
        public Tuple<object, object> Range
        {
            get
            {
                var min = Wrapper.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Minimum));
                var max = Wrapper.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Maximum));
                if (min != null && max != null) return new Tuple<object, object>(min, max);
                return null;
            }
        }

        /// <summary>
        /// Returns true if this property is marked with the Search attribute.
        /// </summary>
        public bool IsSearchable(string rootModelName)
        {
            if (!Wrapper.HasAttribute<SearchAttribute>()) return false;

            if (rootModelName == null) return true;

            var whitelist = Wrapper.GetAttributeObject<SearchAttribute, string>(a => a.RootWhitelist);
            if (!string.IsNullOrWhiteSpace(whitelist))
            {
                return whitelist.Split(',').Contains(rootModelName);
            }

            var blacklist = Wrapper.GetAttributeObject<SearchAttribute, string>(a => a.RootBlacklist);
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
            Wrapper.GetAttributeValue<SearchAttribute, SearchAttribute.SearchMethods>(a => a.SearchMethod)
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
            Wrapper.GetAttributeValue<SearchAttribute, bool>(a => a.IsSplitOnSpaces) ?? false;

        /// <summary>
        /// Returns the fields to search for this object. This could be just the field itself 
        /// or a number of child fields if this is an object or collection.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        public IEnumerable<SearchableProperty> SearchProperties(string rootModelName, int depth = 0, int maxDepth = 2)
        {
            if (!IsSearchable(rootModelName)) yield break;

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
        public bool IsListText => Wrapper.HasAttribute<ListTextAttribute>();

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
                if (Wrapper.HasAttribute<KeyAttribute>())
                    return true;
                else if (string.Compare(Name, "Id", StringComparison.InvariantCultureIgnoreCase) == 0) return true;
                else if (string.Compare(Name, Parent.Name + "Id", StringComparison.InvariantCultureIgnoreCase) == 0) return true;
                else if (Parent.IsDto && Parent.BaseViewModel != null && string.Compare(Name, Parent.BaseViewModel.PrimaryKey.Name, StringComparison.InvariantCultureIgnoreCase) == 0) return true;
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
                if (Wrapper.HasAttribute<ForeignKeyAttribute>() && !IsPOCO)
                {
                    return true;
                }
                if (this.Name.EndsWith("Id") && !IsPrimaryKey && Parent.PropertyByName(Name.Substring(0, Name.Length - 2)) != null)
                {
                    return true;
                }
                if (Parent.Properties.Any(p => Name == (string)p.Wrapper.GetAttributeValue<ForeignKeyAttribute>(nameof(ForeignKeyAttribute.Name))))
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
                var value = (string)Wrapper.GetAttributeValue<ForeignKeyAttribute>(nameof(ForeignKeyAttribute.Name));
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
                if (IsForeignKey)
                {
                    /// Use the ForeignKey Attribute if it is there.
                    var value = (string)Wrapper.GetAttributeValue<ForeignKeyAttribute>(nameof(ForeignKeyAttribute.Name));
                    if (value != null) return value;

                    // Use the ForeignKey Attribute on the object property if it is there.
                    var prop = Parent.Properties.SingleOrDefault(p => Name == (string)p.Wrapper.GetAttributeValue<ForeignKeyAttribute>(nameof(ForeignKeyAttribute.Name)));
                    if (prop != null) return prop.Name;

                    // Else, by convention remove the Id at the end.
                    return Name.Substring(0, Name.Length - 2);
                }
                else
                {
                    return null;
                }
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
                var value = (int?)Wrapper.GetAttributeValue<DisplayAttribute>(nameof(DisplayAttribute.Order));
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
                var order = (int?)Wrapper.GetAttributeValue<DefaultOrderByAttribute>(nameof(DefaultOrderByAttribute.FieldOrder));
                var direction = Wrapper.GetAttributeValue<DefaultOrderByAttribute, DefaultOrderByAttribute.OrderByDirections>(nameof(DefaultOrderByAttribute.OrderByDirection));
                var fieldName = Wrapper.GetAttributeValue<DefaultOrderByAttribute>(nameof(DefaultOrderByAttribute.FieldName)) as string;
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
        /// Ex: Adult/table?adultId=
        /// </summary>
        public string ListEditorUrl
        {
            get
            {
                if (InverseIdProperty == null) { return null; }
                return string.Format("{0}/Table?{1}=", Object.ControllerName, InverseIdProperty.JsonName);
            }
        }

        /// <summary>
        /// Returns the core URL for the List Editor.
        /// </summary>
        public string ListEditorUrlName => string.Format("{0}ListUrl", JsVariable);

        public bool HasViewModelProperty
        {
            get
            {
                if (IsInternalUse) return false;
                if (PureType.Name == "Image") return false;
                if (PureType.Name == "IdentityRole") return false;
                if (PureType.Name == "IdentityUserRole") return false;
                if (PureType.Name == "IdentityUserClaim") return false;
                if (PureType.Name == "IdentityUserLogin") return false;
                return true;
            }
        }

        public bool HasInverseProperty => Wrapper.HasAttribute<InversePropertyAttribute>();

        /// <summary>
        /// For a many to many collection this is the reference to this object from the contained object.
        /// </summary>
        public PropertyViewModel InverseProperty
        {
            get
            {
                var name = Wrapper.GetAttributeObject<InversePropertyAttribute, string>(nameof(InversePropertyAttribute.Property));
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


        /// <summary>
        /// Name of the database column
        /// </summary>
        //TODO: Make this more robust
        public string ColumnName => Name;

        public override string ToString() => $"{Name} : {TypeName}";

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

        public SecurityInfoProperty SecurityInfo
        {
            get
            {
                var result = new SecurityInfoProperty();

                if (Wrapper.HasAttribute<ReadAttribute>())
                {
                    result.IsRead = true;
                    var roles = (string)Wrapper.GetAttributeValue<ReadAttribute>(nameof(ReadAttribute.Roles));
                    result.ReadRoles = roles;
                }

                if (Wrapper.HasAttribute<EditAttribute>())
                {
                    result.IsEdit = true;
                    var roles = (string)Wrapper.GetAttributeValue<EditAttribute>(nameof(EditAttribute.Roles));
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
        public bool HasNotMapped => Wrapper.HasAttribute<NotMappedAttribute>();

        /// <summary>
        /// If true, this property should be searchable on the URL line. 
        /// </summary>
        public bool IsUrlParameter =>
            !IsComplexType && (!Type.IsClass || Type.IsString) && !Type.IsArray && (!Type.IsGeneric || (Type.IsNullable && Type.PureType.IsNumber));

        /// <summary>
        /// List of words already used in the API for other things.
        /// </summary>
        private static readonly string[] ReservedUrlParameterNames = new[] {
            "fields","include","includes","orderby","orderbydescending","page","pagesize","where","dataSource","case","params","if","this","base"};

        /// <summary>
        /// Name of the field to use in the API. If this is in ReservedUrlParameterNames, then my is added to the name.
        /// </summary>
        public string UrlParameterName => ReservedUrlParameterNames.Contains(Name.ToLower()) ? "my" + Name : Name.ToCamelCase();

        /// <summary>
        /// True if this property has the Includes Attribute
        /// </summary>
        public bool HasDtoIncludes => Wrapper.HasAttribute<DtoIncludesAttribute>();

        /// <summary>
        /// Returns a list of content views from the Includes attribute
        /// </summary>
        public List<string> DtoIncludes
        {
            get
            {
                var includes = (Wrapper.GetAttributeValue<DtoIncludesAttribute>(nameof(DtoIncludesAttribute.ContentViews)) as string).Trim();
                return includes.Split(new char[] { ',' }).ToList().ConvertAll(s => s.Trim());
            }
        }

        /// <summary>
        /// True if this property has the Excludes Attribute
        /// </summary>
        public bool HasDtoExcludes => Wrapper.HasAttribute<DtoExcludesAttribute>();

        /// <summary>
        /// Returns a list of content views from the Excludes attribute
        /// </summary>
        public List<string> DtoExcludes
        {
            get
            {
                var excludes = (Wrapper.GetAttributeValue<DtoExcludesAttribute>(nameof(DtoExcludesAttribute.ContentViews)) as string).Trim();
                return excludes.Split(new char[] { ',' }).ToList().ConvertAll(s => s.Trim());
            }
        }

        private string GetPropertySetterConditional(bool isForEdit)
        {
            var readRoles = (SecurityInfo.IsSecuredProperty && SecurityInfo.ReadRolesList.Count() > 0) ?
                string.Join(" || ", SecurityInfo.ReadRolesList.Select(s => $"is{s}")) : "";
            var editRoles = isForEdit && (SecurityInfo.IsSecuredProperty && SecurityInfo.EditRolesList.Count() > 0) ?
                string.Join(" || ", SecurityInfo.EditRolesList.Select(s => $"is{s}")) : "";
            var includes = HasDtoIncludes ? string.Join(" || ", DtoIncludes.Select(s => $"include{s}")) : "";
            var excludes = HasDtoExcludes ? string.Join(" || ", DtoExcludes.Select(s => $"exclude{s}")) : "";

            var statement = new List<string>();
            if (!string.IsNullOrEmpty(readRoles)) statement.Add($"({readRoles})");
            if (!string.IsNullOrEmpty(editRoles)) statement.Add($"({editRoles})");
            if (!string.IsNullOrEmpty(includes)) statement.Add($"({includes})");
            if (!string.IsNullOrEmpty(excludes)) statement.Add($"!({excludes})");

            return string.Join(" && ", statement);
        }

        public string ObjToDtoPropertySetter(string objectName)
        {
            string setter;
            if (Type.IsCollection)
            {
                if (PureType.HasClassViewModel)
                {
                    // Only check the includes tree for things that are in the database.
                    // Otherwise, this would break IncludesExternal.
                    var sb = new StringBuilder();

                    // Set this as a variable once and then use it below. This prevents multiple-evaluation of computed getter-only properties.
                    sb.AppendLine($"var propVal{Name} = obj.{Name};");

                    sb.Append("            ");
                    sb.Append($"if (propVal{Name} != null");
                    if (PureType.ClassViewModel.HasDbSet)
                    {
                        sb.Append($" && (tree == null || tree[nameof({objectName}.{Name})] != null)");
                    }
                    sb.Append(") {");
                    sb.AppendLine();
                    sb.Append("                ");
                    sb.Append($"{objectName}.{Name} = propVal{Name}");

                    var defaultOrderBy = PureType.ClassViewModel.DefaultOrderByClause()?.EscapeStringLiteralForCSharp();
                    if (defaultOrderBy != null)
                    {
                        sb.Append($".OrderBy(\"{defaultOrderBy}\")");
                    }

                    sb.Append($".Select(f => {PureType.Name}DtoGen.Create(f, user, includes, objects, tree?[nameof({objectName}.{Name})])).ToList();");

                    sb.AppendLine();
                    sb.Append("            ");
                    if (PureType.ClassViewModel.HasDbSet)
                    {
                        sb.Append("}");
                        // If we know for sure that we're loading these things (becuse the IncludeTree said so),
                        // but EF didn't load any, then add a blank collection so the client will delete any that already exist.
                        sb.Append($" else if (propVal{Name} == null && tree?[nameof({objectName}.{Name})] != null)");
                        sb.Append(" {");
                        sb.AppendLine();
                        sb.Append("                ");
                        sb.Append($"{objectName}.{Name} = new {PureType.Name}DtoGen[0];");
                        sb.AppendLine();
                        sb.Append("            ");
                        sb.Append("}");
                    }
                    else
                    {
                        sb.Append("}");
                    }

                    sb.AppendLine();
                    setter = sb.ToString();
                }
                else
                {
                    setter = $@"{objectName}.{Name} = obj.{Name};";
                }

            }
            else if (Type.HasClassViewModel)
            {
                // Only check the includes tree for things that are in the database.
                // Otherwise, this would break IncludesExternal.
                string treeCheck = Type.ClassViewModel.HasDbSet ? $"if (tree == null || tree[nameof({objectName}.{Name})] != null)" : "";
                setter = $@"{treeCheck}
                {objectName}.{Name} = {Type.Name}DtoGen.Create(obj.{Name}, user, includes, objects, tree?[nameof({objectName}.{Name})]);
";
            }
            else
            {
                setter = $"{objectName}.{Name} = obj.{Name};";
            }

            var statement = GetPropertySetterConditional(false);
            if (!string.IsNullOrWhiteSpace(statement))
            {
                return $@"            if ({statement})
            {{
                {setter}
            }}
";
            }
            else
            {
                return $"            {setter}\n";
            }
        }

        public string DtoToObjPropertySetter()
        {
            if (IgnorePropertyInUpdates)
            {
                return "";
            }
            else
            {
                var name = Name;
                if (!Type.IsNullable && Type.CsDefaultValue != "null" && !Type.IsByteArray)
                {
                    if (Type.IsDate) name = $"({name} ?? DateTime.Today)";
                    else name = $"({name} ?? {Type.CsDefaultValue})";
                }
                var setter = $"entity.{Name} = {Type.ExplicitConversionType}{name};";

                var statement = GetPropertySetterConditional(true);
                if (!string.IsNullOrWhiteSpace(statement))
                {
                    return $@"          if ({statement})
            {{
                {setter}
            }}
";
                }
                else
                {
                    return $"\t\t\t{setter}{Environment.NewLine}";
                }
            }
        }

        private bool IgnorePropertyInUpdates
        {
            get
            {
                return 
                    IsReadOnly ||
                    (IsPOCO && !IsComplexType) ||
                    IsManytoManyCollection ||
                    Type.IsCollection ||
                    IsInternalUse;
            }
        }
    }
}