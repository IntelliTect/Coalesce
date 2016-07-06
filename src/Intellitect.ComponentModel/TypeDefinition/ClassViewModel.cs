using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Intellitect.ComponentModel.DataAnnotations;
using Intellitect.ComponentModel.Utilities;
using Intellitect.ComponentModel.TypeDefinition.Wrappers;
using Microsoft.CodeAnalysis;

namespace Intellitect.ComponentModel.TypeDefinition
{
    public class ClassViewModel
    {
        internal ClassWrapper Wrapper { get; }
        private string _controllerName;
        private string _apiName;
        public bool HasDbSet { get; }
        //  TODO: Get base URL
        protected string baseUrl = "";

        protected ICollection<PropertyViewModel> _Properties;
        protected ICollection<MethodViewModel> _Methods;

        public ClassViewModel(Type type, string controllerName, string apiName, bool hasDbSet)
            : this(controllerName, apiName, hasDbSet)
        {
            Wrapper = new ReflectionClassWrapper(type);
        }

        public ClassViewModel(ITypeSymbol classSymbol, string controllerName, string apiName, bool hasDbSet) : this(controllerName, apiName, hasDbSet)
        {
            Wrapper = new SymbolClassWrapper(classSymbol);
        }

        private ClassViewModel(string controllerName, string apiName, bool hasDbSet)
        {
            if (!string.IsNullOrWhiteSpace(controllerName))
            {
                _controllerName = controllerName.Replace("Controller", "");
            }
            _apiName = apiName;
            HasDbSet = hasDbSet;

        }

        public string Name
        {
            get { return Wrapper.Name; }
        }

        public string Comment { get { return Wrapper.Comment; } }

        public string ControllerName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_controllerName)) return _controllerName;
                return Name;
            }
        }
        public string ApiName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_apiName)) return _apiName;
                return Name;
            }
        }


        /// <summary>
        /// Name of the ViewModelClass
        /// </summary>
        public string ViewModelClassName
        {
            get { return Name; }
        }

        public string Namespace { get { return Wrapper.Namespace; } }

        /// <summary>
        /// Name of an instance of the ViewModelClass
        /// </summary>
        public string ViewModelObjectName
        {
            get { return ViewModelClassName.ToCamelCase(); }
        }
        /// <summary>
        /// Name of the List ViewModelClass
        /// </summary>
        public string ListViewModelClassName
        {
            get { return Name + "List"; }
        }
        /// <summary>
        /// Name of an instance of the List ViewModelClass
        /// </summary>
        public string ListViewModelObjectName
        {
            get { return ListViewModelClassName.ToCamelCase(); }
        }

        /// <summary>
        /// All properties for the object
        /// </summary>
        public ICollection<PropertyViewModel> Properties
        {
            get
            {
                if (_Properties == null)
                {
                    _Properties = new List<PropertyViewModel>();
                    foreach (var pw in Wrapper.Properties)
                    {
                        if (_Properties.Any(f => f.Name == pw.Name))
                        {
                            // This is a duplicate. Keep the one that isn't virtual
                            if (!pw.IsVirtual)
                            {
                                _Properties.Remove(_Properties.First(f => f.Name == pw.Name));
                                _Properties.Add(new PropertyViewModel(pw, this));
                            }
                        }
                        else
                        {
                            _Properties.Add(new PropertyViewModel(pw, this));
                        }
                    }

                }
                return _Properties;
            }
        }

        /// <summary>
        /// All the methods for the Class
        /// </summary>
        public ICollection<MethodViewModel> Methods
        {
            get
            {
                if (_Methods == null)
                {
                    _Methods = new List<MethodViewModel>();
                    foreach (var mw in Wrapper.Methods)
                    {
                        _Methods.Add(new MethodViewModel(mw, this));
                    }
                }

                return _Methods;
            }
        }


        /// <summary>
        /// Returns the property ID field.
        /// </summary>
        public PropertyViewModel PrimaryKey
        {
            get { return Properties.FirstOrDefault(f => f.IsPrimaryKey); }
        }

        /// <summary>
        /// Use the ListText Attribute first, then Name and then ID.
        /// </summary>
        public PropertyViewModel ListTextProperty
        {
            get
            {
                if (Properties.Any(f => f.IsListText))
                {
                    return Properties.First(f => f.IsListText);
                }
                if (Properties.Any(f => f.Name == "Name"))
                {
                    return Properties.First(f => f.Name == "Name");
                }
                return this.PrimaryKey;
            }
        }


        public string ApiUrl
        {
            get { return baseUrl + @"api/" + ApiName; }
        }
        public string ViewUrl
        {
            get { return baseUrl + ControllerName; }
        }

        /// <summary>
        /// Gets a sorted list of the default order by attributes for the class.
        /// </summary>
        public IEnumerable<OrderByInformation> DefaultOrderBy
        {
            get
            {
                var result = new List<OrderByInformation>();
                foreach (var prop in Properties)
                {
                    var orderInfo = prop.DefaultOrderBy;
                    if (orderInfo != null)
                    {
                        result.Add(orderInfo);
                    }
                }
                // Nothing found, order by ListText and then ID.
                if (!result.Any())
                {
                    if (PropertyByName("Name") != null && !PropertyByName("Name").HasNotMapped)
                    {
                        result.Add(
                            new OrderByInformation()
                            {
                                FieldName = "Name",
                                OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending,
                                FieldOrder = 1
                            });
                    }
                    else
                    {
                        result.Add(
                            new OrderByInformation()
                            {
                                FieldName = Properties.First(f => f.IsPrimaryKey).Name,
                                OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending,
                                FieldOrder = 1
                            });
                    }
                }
                return result.OrderBy(f => f.FieldOrder);
            }
        }

        /// <summary>
        /// Gets a list of properties that should be searched for text.
        /// It will first look for any properties that are decorated with the Search attribute.
        /// If it doesn't find those, it will look for a property called Name or {Class}Name.
        /// If none of those are found, it will result in returning the property that is the primary key for the object
        /// </summary>
        public Dictionary<string, PropertyViewModel> SearchProperties
        {
            get
            {
                var searchProperties = Properties.Where(f => f.IsSearch).ToList();
                var result = new Dictionary<string, PropertyViewModel>();
                if (searchProperties.Any())
                {
                    // Process these items to make sure we have things we can search on.
                    foreach (var prop in searchProperties)
                    {
                        if (prop.Type.HasClassViewModel)
                        {
                            // Remove this item and add the child's search items with a prepended property name
                            var childResult = prop.Type.ClassViewModel.Properties.Where(f => f.IsSearch).ToList();
                            foreach (var childProp in childResult)
                            {
                                result.Add($"{prop.Name}.{childProp.Name}", childProp);
                            }
                        }else
                        {
                            result.Add(prop.Name, prop);
                        }
                    }
                }
                else
                {
                    var prop = Properties.FirstOrDefault(
                        f => string.Compare(f.Name, "Name", StringComparison.InvariantCultureIgnoreCase) == 0 && !f.HasNotMapped);
                    if (prop != null)
                    {
                        result.Add(prop.Name, prop);
                    }
                }
                if (!result.Any())
                {
                    var prop = Properties.FirstOrDefault(
                        f => string.Compare(f.Name, $"{f.Parent.Name}Name", StringComparison.InvariantCultureIgnoreCase) == 0 && !f.HasNotMapped);
                    if (prop != null)
                    {
                        result.Add(prop.Name, prop);
                    }
                }
                if (!result.Any())
                {
                    var prop = Properties.FirstOrDefault(f=>f.IsPrimaryKey);
                    if (prop != null)
                    {
                        result.Add(prop.Name, prop);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Returns a property matching the name if it exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyByName(string key)
        {
            return Properties.FirstOrDefault(f => string.Compare(f.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// Returns a method matching the name if it exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public MethodViewModel MethodByName(string key)
        {
            return Methods.FirstOrDefault(f => string.Compare(f.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// Returns a property matching the name if it exists.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyBySelector<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            PropertyInfo propInfo = GetPropertyInfo<T, TProperty>(propertySelector);
            if (propInfo == null) throw new ArgumentException("Could not find property");
            return PropertyByName(propInfo.Name);
        }


        // http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
        private PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);
            MemberExpression member;

            // Check to see if the node type is a Convert type (this is the case with enums)
            if (propertyLambda.Body.NodeType == ExpressionType.Convert)
            {
                member = ((UnaryExpression)propertyLambda.Body).Operand as MemberExpression;
            }
            else
            {
                member = propertyLambda.Body as MemberExpression;
            }
            if (member == null)
            {
                // Handle the case of a nullable.
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }

        /// <summary>
        /// Returns true if this is a complex type.
        /// </summary>
        public bool IsOneToOne
        {
            get
            {
                return PrimaryKey.IsForeignKey;
            }
        }

        /// <summary>
        /// Returns true if this is a complex type.
        /// </summary>
        public bool IsComplexType
        {
            get
            {
                return HasAttribute<ComplexTypeAttribute>();
            }
        }

        /// <summary>
        /// Returns true if the attribute exists.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public bool HasAttribute<TAttribute>() where TAttribute : Attribute
        {
            return Wrapper.HasAttribute<TAttribute>();
        }

        public bool WillCreateViewController
        {
            get
            {
                var value = (Nullable<bool>)Wrapper.GetAttributeValue<CreateControllerAttribute>(nameof(CreateControllerAttribute.WillCreateView));
                return value == null || value.Value;
            }
        }
        public bool WillCreateApiController
        {
            get
            {
                var value = (Nullable<bool>)Wrapper.GetAttributeValue<CreateControllerAttribute>(nameof(CreateControllerAttribute.WillCreateApi));
                return value == null || value.Value;
            }
        }

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return Regex.Replace(Name, "[A-Z]", " $0").Trim();
            }
        }

        public bool HasViewModel
        {
            get
            {
                if (Name == "IdentityRole") return false;
                return true;
            }
        }

        public override string ToString()
        {
            return $"{Name} : {Wrapper}";
        }

        public string TableName
        {
            get
            {
                //TODO: Make this more robust
                return "dbo." + Name;
            }
        }

        public bool OnContext { get; set; }

        public SecurityInfoClass SecurityInfo
        {
            get
            {
                var result = new SecurityInfoClass();

                if (HasAttribute<ReadAttribute>())
                {
                    result.IsRead = true;
                    var allowAnonmous = (bool?)Wrapper.GetAttributeValue<ReadAttribute>(nameof(ReadAttribute.AllowAnonymous));
                    if (allowAnonmous.HasValue) result.AllowAnonymousRead = allowAnonmous.Value;
                    var roles = (string)Wrapper.GetAttributeValue<ReadAttribute>(nameof(ReadAttribute.Roles));
                    result.ReadRoles = roles;
                }

                if (HasAttribute<EditAttribute>())
                {
                    result.IsEdit = true;
                    var allowAnonmous = (bool?)Wrapper.GetAttributeValue<EditAttribute>(nameof(EditAttribute.AllowAnonymous));
                    if (allowAnonmous.HasValue) result.AllowAnonymousEdit = allowAnonmous.Value;
                    var roles = (string)Wrapper.GetAttributeValue<EditAttribute>(nameof(EditAttribute.Roles));
                    result.EditRoles = roles;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns true if any of the properties allow for a save when there is a validation issue. (warnings)
        /// </summary>
        public bool ClientValidationAllowSave
        {
            get
            {
                return Properties.Any(f => f.ClientValidationAllowSave);
            }
        }

        /// <summary>
        /// True if this class can be created.
        /// </summary>
        public bool IsCreateAllowed
        {
            get
            {
                if (Wrapper.HasAttribute<CreateAttribute>())
                {
                    var allowCreate = Wrapper.GetAttributeValue<CreateAttribute>(nameof(CreateAttribute.Allow)) as bool?;
                    return !allowCreate.HasValue || allowCreate.Value;
                }
                return true;
            }
        }

        /// <summary>
        /// True if this class can be created.
        /// </summary>
        public bool IsDeleteAllowed
        {
            get
            {
                if (Wrapper.HasAttribute<DeleteAttribute>())
                {
                    var allowDelete = Wrapper.GetAttributeValue<DeleteAttribute>(nameof(DeleteAttribute.Allow)) as bool?;
                    return !allowDelete.HasValue || allowDelete.Value;
                }
                return true;
            }
        }

        public bool IsEditAllowed
        {
            get
            {
                if (Wrapper.HasAttribute<EditAttribute>())
                {
                    var allowEdit = Wrapper.GetAttributeValue<EditAttribute>(nameof(EditAttribute.Allow)) as bool?;
                    return !allowEdit.HasValue || allowEdit.Value;
                }
                return true;
            }
        }

    }
}