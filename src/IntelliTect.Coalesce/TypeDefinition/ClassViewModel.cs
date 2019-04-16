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
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Helpers.Search;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class ClassViewModel : IAttributeProvider
    {
        protected IReadOnlyCollection<PropertyViewModel> _Properties;
        protected IReadOnlyCollection<MethodViewModel> _Methods;

        public abstract string Name { get; }
        public abstract string Comment { get; }
        public TypeViewModel Type { get; protected set; }

        public string FullyQualifiedName => Type.FullyQualifiedName;

        /// <summary>
        /// Returns the name of the type to be used by the client, or in other cases
        /// where the type name should be overridable using the [Coalesce] attribute.
        /// This includes places where there could be name conflicts that should be resolvable
        /// by allowing the developer to override the type name for generation.
        /// </summary>
        public string ClientTypeName =>
            // Check for an override first
            this.GetAttributeValue<CoalesceAttribute>(a => a.ClientTypeName) ??
            // If no override, check for an interface service, and trim the conventional 'I' if found.
            (IsService && Type.IsInterface && Name[0] == 'I' && char.IsUpper(Name[1]) ? Name.Substring(1) : null) ??
            // Nothing special - just use the name.
            Name;

        public string ControllerName => ClientTypeName;

        public string ApiControllerClassName
        {
            get
            {
                var overrideName = this.GetAttributeValue<ControllerAttribute>(a => a.ApiControllerName);
                if (!string.IsNullOrWhiteSpace(overrideName)) return overrideName;

                var suffix = this.GetAttributeValue<ControllerAttribute>(a => a.ApiControllerSuffix) ?? "";
                return $"{ControllerName}Controller{suffix}";
            }
        }

        public string ApiRouteControllerPart => ControllerName;

        public string ViewControllerClassName => $"{ControllerName}Controller";

        public string ApiActionAccessModifier =>
            this.GetAttributeValue<ControllerAttribute, bool>(a => a.ApiActionsProtected) ?? false
            ? "protected"
            : "public";

        public string DtoName => IsDto ? FullyQualifiedName : $"{Name}DtoGen";

        public ClassViewModel BaseViewModel => IsDto ? DtoBaseViewModel : this;

        /// <summary>
        /// If this class implements IClassDto, return true.
        /// </summary>
        public bool IsDto => Type.IsA(typeof(IClassDto<>));

        /// <summary>
        /// If this class implements IClassDto, return the ClassViewModel for the type that this DTO is based upon.
        /// </summary>
        public ClassViewModel DtoBaseViewModel => IsDto
            ? Type.GenericArgumentsFor(typeof(IClassDto<>)).First().ClassViewModel
            : null;

        /// <summary>
        /// Name of the ViewModelClass
        /// </summary>
        public string ViewModelClassName => ClientTypeName;

        public string ViewModelGeneratedClassName
        {
            get
            {
                if (!HasTypeScriptPartial)
                    return ViewModelClassName;

                var name = this.GetAttributeValue<TypeScriptPartialAttribute>(a => a.BaseClassName);

                if (string.IsNullOrEmpty(name)) return $"{ViewModelClassName}Partial";

                return name;
            }
        }

        public bool ApiRouted => this.GetAttributeValue<ControllerAttribute, bool>(a => a.ApiRouted) ?? true;

        /// <summary>
        /// Name of the List ViewModelClass
        /// </summary>
        public string ListViewModelClassName => ClientTypeName + "List";

        public bool IsService => HasAttribute<CoalesceAttribute>() && HasAttribute<ServiceAttribute>();

        public string ServiceClientClassName => ClientTypeName + "Client";

        /// <summary>
        /// Name of an instance of the List ViewModelClass
        /// </summary>
        public string ListViewModelObjectName => ListViewModelClassName.ToCamelCase();

        #region Member Info - Properties & Methods

        protected abstract IReadOnlyCollection<PropertyViewModel> RawProperties(ClassViewModel effectiveParent);
        protected abstract IReadOnlyCollection<MethodViewModel> RawMethods { get; }
        protected abstract IReadOnlyCollection<TypeViewModel> RawNestedTypes { get; }

        /// <summary>
        /// All properties for the object.
        /// This collection is not filtered to only those properties which should be exposed to the client.
        /// </summary>
        /// <remarks>
        /// This collection is internal to prevent accidental exposing of properties that should not be exposed.
        /// </remarks>
        internal IReadOnlyCollection<PropertyViewModel> Properties
        {
            get
            {
                if (_Properties != null) return _Properties;

                var properties = new List<PropertyViewModel>();
                int count = 1;
                foreach (var prop in RawProperties(this))
                {
                    if (properties.Any(f => f.Name == prop.Name))
                    {
                        // This is a duplicate. Keep the one that isn't virtual
                        if (!prop.IsVirtual)
                        {
                            properties.Remove(properties.First(f => f.Name == prop.Name));
                            prop.ClassFieldOrder = count;
                            properties.Add(prop);
                        }
                    }
                    else
                    {
                        prop.ClassFieldOrder = count;
                        properties.Add(prop);
                    }
                    count++;
                }

                // Don't assign to _Properties until the end in order to avoid threading issues.
                // If _Properties were mutable, we could potentially have two threads attempting to build the same collection at once.
                return _Properties = properties.AsReadOnly();
            }
        }

        /// <summary>
        /// Properties on the class that are permitted to be exposed to the client.
        /// </summary>
        public IEnumerable<PropertyViewModel> ClientProperties => Properties.Where(p => p.IsClientProperty);

        /// <summary>
        /// Properties on the class that are available on the admin page. This is not filtered by IsHidden.
        /// </summary>
        public IEnumerable<PropertyViewModel> AdminPageProperties => Properties.Where(p => p.IsClientProperty || p.IsFile);

        /// <summary>
        /// Properties on the class that are marked with the [File] attribute.
        /// </summary>
        public IEnumerable<PropertyViewModel> FileProperties => Properties.Where(p => p.IsFile);

        public IEnumerable<PropertyViewModel> DataSourceParameters => Properties
            .Where(p =>
                !p.IsInternalUse && p.HasSetter && p.HasAttribute<CoalesceAttribute>()
                // These are the only supported types, for now
                && (p.Type.IsPrimitive || p.Type.IsDate)
            );

        /// <summary>
        /// List of method names that should not be exposed to the client.
        /// </summary>
        private readonly string[] excludedMethodNames = new[] {
            nameof(object.ToString),
            nameof(object.Equals),
            nameof(object.GetHashCode),
            nameof(object.GetType),
        };

        /// <summary>
        /// All the methods for the Class.
        /// This collection is NOT filtered to only client methods.
        /// It IS filtered by common methods that 
        /// </summary>
        /// <remarks>
        /// This collection is internal to prevent accidental exposing of methods that should not be exposed.
        /// </remarks>
        internal IReadOnlyCollection<MethodViewModel> Methods =>
            _Methods ?? (_Methods = RawMethods
                .Where(m => !excludedMethodNames.Contains(m.Name))
                .Where(m => !IsDto || (m.Name != nameof(IClassDto<object>.MapFrom) && m.Name != nameof(IClassDto<object>.MapTo)))
                .ToList().AsReadOnly());

        public IEnumerable<MethodViewModel> ClientMethods =>
            Methods.Where(m => m.IsClientMethod);

        internal IEnumerable<TypeViewModel> ClientNestedTypes =>
            RawNestedTypes.Where(t => !t.IsInternalUse);

        public IEnumerable<ClassViewModel> ClientDataSources(ReflectionRepository repo) => repo
            .DataSources
            .Where(d => d.DeclaredFor.Equals(this))
            .Select(d => d.StrategyClass)
            .OrderBy(d => d.ClientTypeName);

        /// <summary>
        /// Returns a property matching the name if it exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyByName(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            return Properties.FirstOrDefault(f => string.Equals(f.Name, key, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns a client method matching the name if it exists.
        /// Non-client-exposed methods will not be returned.
        /// </summary>
        /// <param name="name">The name of the method to look for. 
        /// Coalesce doesn't support exposing method overloads - 
        /// in the case of overloads, the first matching method will be returned.</param>
        /// <param name="isStatic">Whether to look for a static or instance method. 
        /// If null, the first match will be returned.</param>
        /// <returns></returns>
        public MethodViewModel MethodByName(string name, bool? isStatic = null)
        {
            return ClientMethods.FirstOrDefault(f =>
                string.Equals(f.Name, name, StringComparison.InvariantCultureIgnoreCase)
                && (isStatic == null || f.IsStatic == isStatic));
        }

        /// <summary>
        /// Returns a property matching the name if it exists.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyBySelector<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            PropertyInfo propInfo = propertySelector.GetExpressedProperty();
            if (propInfo == null) throw new ArgumentException("Could not find property");
            return PropertyByName(propInfo.Name);
        }

        #endregion

        #region Searching/Sorting

        /// <summary>
        /// Returns an expression suitable for usage with LINQ Dynamic
        /// that represents the default sort ordering of instances of the class.
        /// </summary>
        public string DefaultOrderByClause(string prependText = "")
        {
            var defaultOrderBy = DefaultOrderBy.ToList();

            if (defaultOrderBy.Count == 0) return null;

            var orderByClauseList = new List<string>();
            foreach (var orderInfo in defaultOrderBy)
            {
                if (orderInfo.OrderByDirection == DefaultOrderByAttribute.OrderByDirections.Ascending)
                {
                    orderByClauseList.Add($"{orderInfo.OrderExpression(prependText)} ASC");
                }
                else
                {
                    orderByClauseList.Add($"{orderInfo.OrderExpression(prependText)} DESC");
                }
            }

            return string.Join(",", orderByClauseList);
        }

        /// <summary>
        /// Gets a sorted list of the default order by attributes for the class.
        /// </summary>
        public ICollection<OrderByInformation> DefaultOrderBy
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

                if (result.Count > 0)
                {
                    return result.OrderBy(f => f.FieldOrder).ToList();
                }

                // Nothing found, order by ListText and then ID.
                var nameProp = PropertyByName("Name");
                if (nameProp?.HasNotMapped == false && nameProp.IsClientProperty)
                {
                    result.Add(new OrderByInformation()
                    {
                        FieldName = "Name",
                        OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending,
                        FieldOrder = 1
                    });
                }
                else if (PrimaryKey != null)
                {
                    result.Add(new OrderByInformation()
                    {
                        FieldName = PrimaryKey.Name,
                        OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending,
                        FieldOrder = 1
                    });
                }
                return result;
            }
        }

        /// <summary>
        /// Gets a list of properties that should be searched for text.
        /// It will first look for any properties that are decorated with the Search attribute.
        /// If it doesn't find those, it will look for a property called Name or {Class}Name.
        /// If none of those are found, it will result in returning the property that is the primary key for the object
        /// </summary>
        public IEnumerable<SearchableProperty> SearchProperties(ClassViewModel rootModel = null, int depth = 0, int maxDepth = 2)
        {
            // Only go down three levels.
            if (depth == 3) yield break;

            var searchProperties = Properties.Where(f => f.IsSearchable(rootModel)).ToList();
            if (searchProperties.Count > 0)
            {
                // Process these items to make sure we have things we can search on.
                foreach (var property in searchProperties)
                {
                    // Get all the child items
                    foreach (var searchProperty in property.SearchProperties(rootModel, depth, maxDepth))
                    {
                        yield return searchProperty;
                    }
                }
                yield break;
            }

            yield return new[]
            {
                PropertyByName("Name"),
                Properties.FirstOrDefault(p => p.Name == $"{p.Parent.Name}Name"),
                PrimaryKey
            }
            .Where(p => p != null && p.IsClientProperty && p.HasNotMapped == false)
            .Select(p => new SearchableValueProperty(p))
            .FirstOrDefault();
        }

        #endregion

        /// <summary>
        /// Returns the property ID field.
        /// </summary>
        public PropertyViewModel PrimaryKey => Properties.FirstOrDefault(f => f.IsPrimaryKey);

        /// <summary>
        /// Use the ListText Attribute first, then Name and then ID.
        /// </summary>
        public PropertyViewModel ListTextProperty =>
            ClientProperties.FirstOrDefault(f => f.IsListText) ??
            ClientProperties.FirstOrDefault(f => f.Name == "Name") ??
            PrimaryKey;

        public bool IsOneToOne => PrimaryKey?.IsForeignKey ?? false;

        /// <summary>
        /// Returns true if this class has a partial typescript file.
        /// </summary>
        public bool HasTypeScriptPartial => HasAttribute<TypeScriptPartialAttribute>();

        public bool WillCreateViewController =>
            this.GetAttributeValue<CreateControllerAttribute, bool>(a => a.WillCreateView) ?? true;

        public bool WillCreateApiController =>
            this.GetAttributeValue<CreateControllerAttribute, bool>(a => a.WillCreateApi) ?? true;

        /// <summary>
        /// Returns a human-readable string that represents the name of this type to the client.
        /// </summary>
        public string DisplayName => ClientTypeName.ToProperCase();

        public bool IsDbMappedType => HasDbSet || (DtoBaseViewModel?.HasDbSet ?? false);

        /// <summary>
        /// Has a DbSet property in the Context.
        /// </summary>
        public bool HasDbSet { get; internal set; }

        private ClassSecurityInfo _securityInfo;

        public ClassSecurityInfo SecurityInfo => _securityInfo ?? (_securityInfo = new ClassSecurityInfo(
            this.GetSecurityPermission<ReadAttribute>(),
            this.GetSecurityPermission<EditAttribute>(),
            this.GetSecurityPermission<DeleteAttribute>(),
            this.GetSecurityPermission<CreateAttribute>()
        ));

        public ExecuteSecurityInfo ExecuteSecurity => new ExecuteSecurityInfo(this.GetSecurityPermission<ExecuteAttribute>());

        public bool IsDefaultDataSource => HasAttribute<DefaultDataSourceAttribute>();

        public object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute =>
            Type.GetAttributeValue<TAttribute>(valueName);

        public bool HasAttribute<TAttribute>() where TAttribute : Attribute =>
            Type.HasAttribute<TAttribute>();

        protected SecurityPermission GetSecurityAttribute<TAttribute>()
            where TAttribute : SecurityAttribute =>
            !HasAttribute<TAttribute>()
            ? new SecurityPermission()
            : new SecurityPermission(
                level: this.GetAttributeValue<TAttribute, SecurityPermissionLevels>(a => a.PermissionLevel) ?? SecurityPermissionLevels.AllowAuthorized,
                roles: this.GetAttributeValue<TAttribute>(a => a.Roles),
                name: typeof(TAttribute).Name.Replace("Attribute", string.Empty)
            );

        public override string ToString() => FullyQualifiedName;

        public override bool Equals(object obj) =>
            Object.ReferenceEquals(this, obj)
            || (obj is ClassViewModel that && this.Type.Equals(that.Type));

        public override int GetHashCode() => this.Type.GetHashCode();
    }
}