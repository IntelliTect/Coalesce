using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using System.Collections.Concurrent;
using IntelliTect.Coalesce.TypeUsage;
using IntelliTect.Coalesce.Api;
using System.Threading;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionRepository
    {
        public static readonly ReflectionRepository Global = new ReflectionRepository();


        private readonly HashSet<DbContextTypeUsage> _contexts = new HashSet<DbContextTypeUsage>();
        private readonly HashSet<ClassViewModel> _entities = new HashSet<ClassViewModel>();
        private ILookup<ClassViewModel, EntityTypeUsage> _entityUsages = null;

        private readonly HashSet<CrudStrategyTypeUsage> _behaviors = new HashSet<CrudStrategyTypeUsage>();
        private readonly HashSet<CrudStrategyTypeUsage> _dataSources = new HashSet<CrudStrategyTypeUsage>();
        private readonly HashSet<ClassViewModel> _externalTypes = new HashSet<ClassViewModel>();
        private readonly HashSet<ClassViewModel> _customDtos = new HashSet<ClassViewModel>();
        private readonly HashSet<ClassViewModel> _services = new HashSet<ClassViewModel>();
        private readonly HashSet<TypeViewModel> _enums = new HashSet<TypeViewModel>();

        private readonly ConcurrentDictionary<object, TypeViewModel> _allTypeViewModels
            = new ConcurrentDictionary<object, TypeViewModel>();

        private readonly object _discoverLock = new object();

        public ReadOnlyHashSet<DbContextTypeUsage> DbContexts => new ReadOnlyHashSet<DbContextTypeUsage>(_contexts);
        public ILookup<ClassViewModel, EntityTypeUsage> EntityUsages => _entityUsages ??= DbContexts
            .SelectMany(contextUsage => contextUsage.Entities)
            .ToLookup(entityUsage => entityUsage.ClassViewModel);

        public ReadOnlyHashSet<ClassViewModel> Entities => new ReadOnlyHashSet<ClassViewModel>(_entities);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> Behaviors => new ReadOnlyHashSet<CrudStrategyTypeUsage>(_behaviors);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> DataSources => new ReadOnlyHashSet<CrudStrategyTypeUsage>(_dataSources);
        public ReadOnlyHashSet<ClassViewModel> ExternalTypes => new ReadOnlyHashSet<ClassViewModel>(_externalTypes);
        public ReadOnlyHashSet<ClassViewModel> CustomDtos => new ReadOnlyHashSet<ClassViewModel>(_customDtos);
        public ReadOnlyHashSet<ClassViewModel> Services => new ReadOnlyHashSet<ClassViewModel>(_services);

        [Obsolete("Replaced by better-named property \"CrudApiBackedClasses\".")]
        public IEnumerable<ClassViewModel> ApiBackedClasses => CrudApiBackedClasses;
        public IEnumerable<ClassViewModel> CrudApiBackedClasses => Entities.Union(CustomDtos);

        public IEnumerable<ClassViewModel> ControllerBackedClasses => CrudApiBackedClasses.Union(Services);

        public IEnumerable<ClassViewModel> ClientClasses => CrudApiBackedClasses.Union(ExternalTypes);

        public ReadOnlyHashSet<TypeViewModel> ClientEnums => new ReadOnlyHashSet<TypeViewModel>(_enums);

        public IEnumerable<ClassViewModel> DiscoveredClassViewModels =>
            DbContexts.Select(t => t.ClassViewModel)
            .Union(ClientClasses);

        public ReflectionRepository()
        {
        }

        private HashSet<string> _rootTypeWhitelist = null;
        internal void SetRootTypeWhitelist(IEnumerable<string> typeNames)
        {
            _rootTypeWhitelist = new HashSet<string>(typeNames);
            if (!_rootTypeWhitelist.Any())
            {
                _rootTypeWhitelist = null;
            }
        }

        internal void DiscoverCoalescedTypes(IEnumerable<TypeViewModel> rootTypes)
        {
            lock (_discoverLock)
            {
                AddTypes(rootTypes
                    // For some reason, attribute checking can be really slow. We're talking ~350ms to determine that the DbContext type has a [Coalesce] attribute.
                    // Really not sure why, but lets parallelize to minimize that impact.
                    .AsParallel()
                    .Where(type => type.HasAttribute<CoalesceAttribute>())
                );
            }
        }

        public void AddTypes(IEnumerable<TypeViewModel> types)
        {
            foreach (var type in types) GetOrAddType(type);
        }

        public SymbolTypeViewModel GetOrAddType(ITypeSymbol symbol) =>
            GetOrAddType(symbol, () => new SymbolTypeViewModel(this, symbol)) as SymbolTypeViewModel;

        public ReflectionTypeViewModel GetOrAddType(Type type) =>
            GetOrAddType(type, () => new ReflectionTypeViewModel(this, type)) as ReflectionTypeViewModel;

        public TypeViewModel GetOrAddType(TypeViewModel type) => 
            GetOrAddType(GetCacheKey(type), () => type);

        private TypeViewModel GetOrAddType(object key, Func<TypeViewModel> factory)
        {
            // First, see if we already have the type. Do not invoke the factory for this check.
            if (_allTypeViewModels.TryGetValue(key, out var existing)) return existing;

            var newType = factory();
            if (_allTypeViewModels.TryAdd(key, newType))
            {
                ProcessAddedType(newType);
            }

            // Re-fetch, because this isn't strictly the one that we maybe just added.
            return _allTypeViewModels[key];
        }

        private void ProcessAddedType(TypeViewModel type)
        {
            if (!type.HasAttribute<CoalesceAttribute>())
            {
                return;
            }

            if (_rootTypeWhitelist != null && !_rootTypeWhitelist.Contains(type.Name))
            {
                return;
            }

            if (type.IsA<DbContext>())
            {
                var context = new DbContextTypeUsage(type.ClassViewModel);

                var entityCvms = context.Entities.Select(e => GetOrAddType(e.TypeViewModel).ClassViewModel);
                lock (_contexts)
                {
                    _contexts.Add(context);
                    _entities.UnionWith(entityCvms);
                    // Some of our entities may have gotten discovered as external types already.
                    // Remove them from that set now that we know they're entities
                    _externalTypes.ExceptWith(entityCvms);
                }

                // Null this out so it gets recomputed on next access.
                _entityUsages = null;

                foreach (var entity in context.Entities)
                {
                    DiscoverOnApiBackedClass(entity.TypeViewModel.ClassViewModel);
                }
            }
            else if (AddCrudStrategy(typeof(IDataSource<>), type, _dataSources))
            {
                // Handled by helper
            }
            else if (AddCrudStrategy(typeof(IBehaviors<>), type, _behaviors))
            {
                // Handled by helper
            }
            else if (type.IsA(typeof(IClassDto<>)))
            {
                var classViewModel = type.ClassViewModel;
                _customDtos.Add(classViewModel);
                DiscoverOnApiBackedClass(classViewModel);
            }
            else if (type.ClassViewModel?.IsService ?? false)
            {
                var classViewModel = type.ClassViewModel;
                _services.Add(classViewModel);
                DiscoverExternalMethodTypesOn(classViewModel);
            }

            void DiscoverOnApiBackedClass(ClassViewModel classViewModel)
            {
                DiscoverExternalMethodTypesOn(classViewModel);
                DiscoverExternalPropertyTypesOn(classViewModel);
                DiscoverNestedCrudStrategiesOn(classViewModel);
            }
        }

        /// <summary>
        /// Adds types from the assembly where type T resides.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddAssembly<T>() =>
            DiscoverCoalescedTypes(typeof(T).Assembly.ExportedTypes.Select(t => new ReflectionTypeViewModel(this, t)));


        private object GetCacheKey(TypeViewModel typeViewModel) =>
            (typeViewModel as ReflectionTypeViewModel)?.Info as object
            ?? (typeViewModel as SymbolTypeViewModel)?.Symbol as object
            ?? throw new NotSupportedException("Unknown subtype of TypeViewModel");


        /// <summary>
        /// Attempt to add the given ClassViewModel as an ExternalType if it isn't already known.
        /// If its a newly discovered type, recurse into that type's properties as well.
        /// </summary>
        /// <param name="externalType"></param>
        private void ConditionallyAddAndDiscoverExternalPropertyTypesOn(ClassViewModel externalType)
        {
            // Don't dig in if:
            //  - This is a known entity type (its not external)
            //  - This is a known custom DTO type (again, not external)
            //  - This is already a known external type (don't infinitely recurse).
            if (
                !Entities.Contains(externalType)
                && !CustomDtos.Contains(externalType)
                && !ExternalTypes.Contains(externalType)
                )
            {
                if (_externalTypes.Add(externalType))
                {
                    DiscoverExternalPropertyTypesOn(externalType);
                }
            }
        }

        private void DiscoverExternalPropertyTypesOn(ClassViewModel model)
        {
            foreach (var type in model
                .ClientProperties
                .Select(p => p.PureType))
            {
                if (type.HasClassViewModel)
                {
                    ConditionallyAddAndDiscoverExternalPropertyTypesOn(type.ClassViewModel);
                }
                else if (type.IsEnum)
                {
                    _enums.Add(type.NullableUnderlyingType);
                }
            }
        }

        private void DiscoverExternalMethodTypesOn(ClassViewModel model)
        {
            foreach (var method in model.ClientMethods)
            {
                var returnType = method.ResultType.PureType;
                if (returnType.HasClassViewModel)
                {
                    // Return type looks like an external type.
                    ConditionallyAddAndDiscoverExternalPropertyTypesOn(returnType.ClassViewModel);
                }
                else if (returnType.IsEnum)
                {
                    _enums.Add(returnType.NullableUnderlyingType);
                }

                foreach (var arg in method.Parameters.Where(p => !p.IsDI))
                {
                    if (arg.PureType.HasClassViewModel)
                    {
                        // Parameter looks like an external type.
                        ConditionallyAddAndDiscoverExternalPropertyTypesOn(arg.PureType.ClassViewModel);
                    }
                    else if (arg.PureType.IsEnum)
                    {
                        _enums.Add(arg.PureType.NullableUnderlyingType);
                    }
                }
            }
        }

        private bool AddCrudStrategy(
            Type iface,
            TypeViewModel strategyType,
            HashSet<CrudStrategyTypeUsage> set,
            ClassViewModel declaredFor = null
        )
        {
            if (!strategyType.IsA(iface)) return false;

            var servedType = strategyType.GenericArgumentsFor(iface).Single();
            if (!servedType.HasClassViewModel)
            {
                throw new InvalidOperationException($"{servedType} is not a valid type argument for a {iface}.");
            }
            var servedClass = servedType.ClassViewModel;

            // See if we were expecting that the strategy be declared for a particular type
            // by virtue of its nesting. If this type has been overridden to something else by an attribute, then that's wrong.
            var explicitlyDeclaredFor = strategyType.GetAttributeValue<DeclaredForAttribute>(a => a.DeclaredFor)?.ClassViewModel;
            if (explicitlyDeclaredFor != null && declaredFor != null && !explicitlyDeclaredFor.Equals(declaredFor))
            {
                throw new InvalidOperationException(
                    $"Expected that {strategyType} is declared for {declaredFor}, but it was explicitly declared for {explicitlyDeclaredFor} instead.");
            }

            // Any explicit declaration is OK. Use that,
            // or the passed in value if no explicit value is given via attribute,
            // or just the type that is served if neither are present.
            declaredFor = explicitlyDeclaredFor ?? declaredFor ?? servedClass;

            if (declaredFor.IsDto && !servedClass.Equals(declaredFor.DtoBaseViewModel))
            {
                throw new InvalidOperationException($"{strategyType} is not a valid {iface} for {declaredFor} - " +
                    $"{strategyType} must satisfy {iface} with type parameter <{declaredFor.DtoBaseViewModel}>.");
            }

            set.Add(new CrudStrategyTypeUsage(strategyType.ClassViewModel, servedClass, declaredFor));

            if (strategyType.IsA(typeof(IDataSource<>)))
            {
                foreach (var parameter in strategyType.ClassViewModel.DataSourceParameters)
                {
                    if (parameter.PureType.IsEnum)
                    {
                        _enums.Add(parameter.PureType.NullableUnderlyingType);
                    }
                }
            }

            return true;
        }

        private void DiscoverNestedCrudStrategiesOn(ClassViewModel model)
        {
            foreach (var nestedType in model.ClientNestedTypes)
            {
                AddCrudStrategy(typeof(IDataSource<>), nestedType, _dataSources, model);
                AddCrudStrategy(typeof(IBehaviors<>), nestedType, _behaviors, model);
            }
        }

        public ClassViewModel GetClassViewModel(Type classType) =>
            GetOrAddType(classType).ClassViewModel;

        public ClassViewModel GetClassViewModel(INamedTypeSymbol classType) =>
            GetOrAddType(classType).ClassViewModel;

        public ClassViewModel GetClassViewModel<T>() => GetClassViewModel(typeof(T));


        /// <summary>
        /// Gets a propertyViewModel based on the property selector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyBySelector<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            var objModel = GetClassViewModel<T>();
            return objModel.PropertyBySelector(propertySelector);
        }
    }
}
