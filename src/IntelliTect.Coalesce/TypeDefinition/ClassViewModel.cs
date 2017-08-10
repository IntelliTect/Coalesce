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
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ClassViewModel
    {
        internal ClassWrapper Wrapper { get; }

        /// <summary>
        /// Has a DbSet property in the Context.
        /// </summary>
        public bool HasDbSet { get; internal set; }
        
        protected ICollection<PropertyViewModel> _Properties;
        protected ICollection<MethodViewModel> _Methods;

        public ClassViewModel(TypeViewModel type)
        {
            if (type.Wrapper is Wrappers.ReflectionTypeWrapper)
            {
                Wrapper = new ReflectionClassWrapper(((ReflectionTypeWrapper)(type.Wrapper)).Info);
            }
            else
            {
                Wrapper = new SymbolClassWrapper((INamedTypeSymbol)(((SymbolTypeWrapper)(type.Wrapper)).Symbol));
            }
        }

        public ClassViewModel(Type type)
        {
            Wrapper = new ReflectionClassWrapper(type);
        }

        public ClassViewModel(ITypeSymbol classSymbol)
        {
            Wrapper = new SymbolClassWrapper(classSymbol);
        }

        public string Name => Wrapper.Name;

        public string FullName => Wrapper.Namespace + "." + Wrapper.Name;

        public string Comment => Wrapper.Comment;

        public string ControllerName => Name;

        public string ApiControllerClassName
        {
            get
            {
                var overrideName = Wrapper.GetAttributeObject<ControllerAttribute, string>(nameof(ControllerAttribute.ApiControllerName));
                if (!string.IsNullOrWhiteSpace(overrideName)) return overrideName;

                var suffix = Wrapper.GetAttributeObject<ControllerAttribute, string>(nameof(ControllerAttribute.ApiControllerSuffix));
                if (!string.IsNullOrWhiteSpace(suffix)) return $"{ControllerName}Controller{suffix}";

                return $"{ControllerName}Controller";
            }
        }

        public string ApiActionAccessModifier =>
            (Wrapper.GetAttributeValue<ControllerAttribute, bool>(nameof(ControllerAttribute.ApiActionsProtected)) ?? false) ? "protected" : "public";

        public string ApiName => Name;

        public string DtoName => IsDto ? Name : $"{Name}DtoGen";

        public ClassViewModel BaseViewModel => IsDto ? DtoBaseViewModel : this;

        /// <summary>
        /// Returns true if this is a DTO that uses another underlying type specifed in DtoBaseViewModel.
        /// </summary>
        public bool IsDto => Wrapper.IsDto;

        /// <summary>
        /// The ClassViewModel this DTO is based on.
        /// </summary>
        public ClassViewModel DtoBaseViewModel { get { return Wrapper.DtoBaseType; } }


        /// <summary>
        /// Name of the ViewModelClass
        /// </summary>
        public string ViewModelClassName => Name;

        public string ViewModelGeneratedClassName
        {
            get
            {
                if (!HasTypeScriptPartial)
                    return ViewModelClassName;

                var name = Wrapper.GetAttributeValue<TypeScriptPartialAttribute>(nameof(TypeScriptPartialAttribute.BaseClassName)) as string;

                if (string.IsNullOrEmpty(name)) return $"{ViewModelClassName}Partial";

                return name;
            }
        }

        public bool ApiRouted => Wrapper.GetAttributeValue<ControllerAttribute, bool>(nameof(ControllerAttribute.ApiRouted)) ?? true;


        public string Namespace => Wrapper.Namespace;

        /// <summary>
        /// Name of an instance of the ViewModelClass
        /// </summary>
        public string ViewModelObjectName => ViewModelClassName.ToCamelCase();

        /// <summary>
        /// Name of the List ViewModelClass
        /// </summary>
        public string ListViewModelClassName => Name + "List";

        /// <summary>
        /// Name of an instance of the List ViewModelClass
        /// </summary>
        public string ListViewModelObjectName => ListViewModelClassName.ToCamelCase();

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
                    int count = 1;
                    foreach (var pw in Wrapper.Properties)
                    {
                        if (_Properties.Any(f => f.Name == pw.Name))
                        {
                            // This is a duplicate. Keep the one that isn't virtual
                            if (!pw.IsVirtual)
                            {
                                _Properties.Remove(_Properties.First(f => f.Name == pw.Name));
                                var prop = new PropertyViewModel(pw, this, count);
                                _Properties.Add(prop);
                            }
                        }
                        else
                        {
                            var prop = new PropertyViewModel(pw, this, count);
                            _Properties.Add(prop);
                        }
                        count++;
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
                    int count = 1;
                    foreach (var mw in Wrapper.Methods)
                    {
                        if (!IsDto || (mw.Name != "Update" && mw.Name != "CreateInstance"))
                        {
                            _Methods.Add(new MethodViewModel(mw, this, count));
                            count++;
                        }
                    }
                }

                return _Methods;
            }
        }


        /// <summary>
        /// Returns the property ID field.
        /// </summary>
        public PropertyViewModel PrimaryKey => Properties.FirstOrDefault(f => f.IsPrimaryKey);

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


        public string ApiUrl => ApiName;

        public string ViewUrl => ControllerName;

        public string DefaultOrderByClause(string prependText = "")
        {
            var defaultOrderBy = DefaultOrderBy.ToList();
            if (defaultOrderBy.Any())
            {
                var orderByClauseList = new List<string>();
                foreach (var orderInfo in defaultOrderBy)
                {
                    if (orderInfo.OrderByDirection == DefaultOrderByAttribute.OrderByDirections.Ascending)
                    {
                        orderByClauseList.Add($"{prependText}{orderInfo.FieldName} ASC");
                    }
                    else
                    {
                        orderByClauseList.Add($"{prependText}{orderInfo.FieldName} DESC");
                    }
                }
                return string.Join(",", orderByClauseList);
            }
            return null;

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
                    else if (Properties.Any(f => f.IsPrimaryKey))
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
        public Dictionary<string, PropertyViewModel> SearchProperties(string rootModelName = null, int depth = 0)
        {
            // Only go down three levels.
            if (depth == 3) return new Dictionary<string, PropertyViewModel>();

            var searchProperties = Properties.Where(f => f.IsSearchable(rootModelName)).ToList();
            var result = new Dictionary<string, PropertyViewModel>();
            if (searchProperties.Any())
            {
                // Process these items to make sure we have things we can search on.
                foreach (var prop in searchProperties)
                {
                    // Get all the child items
                    foreach (var kvp in prop.SearchTerms(rootModelName, depth))
                    {
                        result.Add(kvp.Key, kvp.Value);
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
                var prop = Properties.FirstOrDefault(f => f.IsPrimaryKey);
                if (prop != null)
                {
                    result.Add(prop.Name, prop);
                }
            }
            return result;
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
            PropertyInfo propInfo = GetPropertyInfo(propertySelector);
            if (propInfo == null) throw new ArgumentException("Could not find property");
            return PropertyByName(propInfo.Name);
        }


        // http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
        internal PropertyInfo GetPropertyInfo(LambdaExpression propertyLambda)
        {
            return propertyLambda.GetExpressedProperty(Type);
        }

        public bool IsOneToOne => PrimaryKey?.IsForeignKey ?? false;

        /// <summary>
        /// Returns true if this is a complex type.
        /// </summary>
        public bool IsComplexType => HasAttribute<ComplexTypeAttribute>();

        /// <summary>
        /// Returns true if this class has a partial typescript file.
        /// </summary>
        public bool HasTypeScriptPartial => HasAttribute<TypeScriptPartialAttribute>();


        /// <summary>
        /// Returns true if the attribute exists.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public bool HasAttribute<TAttribute>() where TAttribute : Attribute => Wrapper.HasAttribute<TAttribute>();

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
        public string DisplayName => Regex.Replace(Name, "[A-Z]", " $0").Trim();

        public bool HasViewModel => Name != "IdentityRole";

        public override string ToString() => $"{Name} : {Wrapper}";

        public string TableName
        {
            get
            {
                string tableName = (string)Wrapper.GetAttributeValue<TableAttribute>(nameof(TableAttribute.Name)) ?? ContextPropertyName;

                if (tableName != null)
                    return "dbo." + tableName;

                return "dbo." + Name;
            }
        }

        public string ContextPropertyName { get; set; }
        public bool OnContext { get; set; }

        private SecurityInfoClass _securityInfo;
        public SecurityInfoClass SecurityInfo
        {
            get
            {
                if (_securityInfo == null)
                {
                    _securityInfo = new SecurityInfoClass(
                        new SecurityInfoPermission(Wrapper.GetSecurityAttribute<ReadAttribute>()),
                        new SecurityInfoPermission(Wrapper.GetSecurityAttribute<EditAttribute>()),
                        new SecurityInfoPermission(Wrapper.GetSecurityAttribute<DeleteAttribute>()),
                        new SecurityInfoPermission(Wrapper.GetSecurityAttribute<CreateAttribute>())
                    );

                    //if (HasAttribute<ReadAttribute>())
                    //{
                    //    _securityInfo.IsRead = true;
                    //    var allowAnonmous = (bool?)Wrapper.GetAttributeValue<ReadAttribute>(nameof(ReadAttribute.AllowAnonymous));
                    //    if (allowAnonmous.HasValue) _securityInfo.AllowAnonymousRead = allowAnonmous.Value;
                    //    var roles = (string)Wrapper.GetAttributeValue<ReadAttribute>(nameof(ReadAttribute.Roles));
                    //    _securityInfo.ReadRoles = roles;
                    //}

                    //if (HasAttribute<EditAttribute>())
                    //{
                    //    _securityInfo.IsEdit = true;
                    //    var allowAnonmous = (bool?)Wrapper.GetAttributeValue<EditAttribute>(nameof(EditAttribute.AllowAnonymous));
                    //    if (allowAnonmous.HasValue) _securityInfo.AllowAnonymousEdit = allowAnonmous.Value;
                    //    var roles = (string)Wrapper.GetAttributeValue<EditAttribute>(nameof(EditAttribute.Roles));
                    //    _securityInfo.EditRoles = roles;
                    //}
                }

                return _securityInfo;
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

        public string DtoIncludesAsCS()
        {
            var includeList = Properties
                                .Where(p => p.HasDtoIncludes)
                                .SelectMany(p => p.DtoIncludes)
                                .Distinct()
                                .Select(include => $"bool include{include} = includes == \"{include}\";")
                                .ToList();

            return string.Join($"{Environment.NewLine}\t\t\t", includeList);
        }

        public string DtoExcludesAsCS()
        {
            var excludeList = Properties
                                .Where(p => p.HasDtoExcludes)
                                .SelectMany(p => p.DtoExcludes)
                                .Distinct()
                                .Select(exclude => $"bool exclude{exclude} = includes == \"{exclude}\";")
                                .ToList();
            return string.Join($"{Environment.NewLine}\t\t\t", excludeList);
        }

        public string PropertyRolesAsCS()
        {
            var allPropertyRoles = Properties.Aggregate(new List<string>(), (p, c) => p.Union(c.SecurityInfo.EditRolesList.Union(c.SecurityInfo.ReadRolesList)).ToList());

            var output = allPropertyRoles.Select(role => $"bool is{role} = false;").ToList();
            output.Add("if (user != null)");
            output.Add("{");
            output.AddRange(allPropertyRoles.Select(role => $"\tis{role} = user.IsInRole(\"{role}\");"));
            output.Add("}");

            return string.Join($"{Environment.NewLine}\t\t\t", output);
        }

        public Type Type
        {
            get
            {
                return Wrapper.Info;
            }
        }
    }
}