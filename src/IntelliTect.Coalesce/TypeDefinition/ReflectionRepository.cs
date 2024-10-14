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
using IntelliTect.Coalesce.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionRepository
    {
        public static readonly ReflectionRepository Global = new();


        private readonly ConcurrentHashSet<DbContextTypeUsage> _contexts = new();
        private readonly ConcurrentHashSet<ClassViewModel> _entities = new();

        private readonly ConcurrentHashSet<CrudStrategyTypeUsage> _behaviors = new();
        private readonly ConcurrentHashSet<CrudStrategyTypeUsage> _dataSources = new();
        private readonly ConcurrentHashSet<ClassViewModel> _externalTypes = new();
        private readonly ConcurrentHashSet<ClassViewModel> _customDtos = new();
        private readonly ConcurrentHashSet<ClassViewModel> _services = new();
        private readonly ConcurrentHashSet<TypeViewModel> _enums = new();

        private readonly ConcurrentDictionary<ClassViewModel, ClassViewModel> _generatedParamDtos = new();

        private readonly ConcurrentDictionary<object, TypeViewModel> _allTypeViewModels
            = new();

        private readonly object _discoverLock = new();

        public ReadOnlyHashSet<DbContextTypeUsage> DbContexts => new(_contexts);

        private ILookup<ClassViewModel, EntityTypeUsage>? _entityUsages = null;
        public ILookup<ClassViewModel, EntityTypeUsage> EntityUsages => _entityUsages ??= DbContexts
            .SelectMany(contextUsage => contextUsage.Entities)
            .ToLookup(entityUsage => entityUsage.ClassViewModel);

        private ILookup<string, MethodViewModel>? _clientMethods = null;
        public ILookup<string, MethodViewModel> ClientMethodsLookup => _clientMethods ??= ControllerBackedClasses
            .SelectMany(c => c.ClientMethods)
            .ToLookup(m => m.Name);

        private ReadOnlyDictionary<string, ClassViewModel>? _clientTypes;
        public ReadOnlyDictionary<string, ClassViewModel> ClientTypesLookup 
            => _clientTypes ??= new(ClientClasses.Union(Services).ToDictionary(c => c.ClientTypeName, StringComparer.OrdinalIgnoreCase));

        public ReadOnlyHashSet<ClassViewModel> Entities => new(_entities);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> Behaviors => new(_behaviors);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> DataSources => new(_dataSources);
        public ReadOnlyHashSet<ClassViewModel> ExternalTypes => new(_externalTypes);
        public ReadOnlyHashSet<ClassViewModel> CustomDtos => new(_customDtos);
        public ReadOnlyHashSet<ClassViewModel> Services => new(_services);

        /// <summary>
        /// A map from an entity or external type to the DTO that was generated for it.
        /// </summary>
        internal ReadOnlyDictionary<ClassViewModel, ClassViewModel> GeneratedParameterDtos => new(_generatedParamDtos);

        [Obsolete("Replaced by better-named property \"CrudApiBackedClasses\".")]
        public IEnumerable<ClassViewModel> ApiBackedClasses => CrudApiBackedClasses;
        public IEnumerable<ClassViewModel> CrudApiBackedClasses => Entities.Union(CustomDtos);

        /// <summary>
        /// Types that have a generated API controller.
        /// </summary>
        public IEnumerable<ClassViewModel> ControllerBackedClasses => CrudApiBackedClasses.Union(Services);

        /// <summary>
        /// Types that have a generated model class or inteface that includes the type's exposed properties.
        /// Does not include [Service]s.
        /// </summary>
        public IEnumerable<ClassViewModel> ClientClasses => CrudApiBackedClasses.Union(ExternalTypes);

        public ReadOnlyHashSet<TypeViewModel> ClientEnums => new(_enums);

        internal IEnumerable<ClassViewModel> DiscoveredClassViewModels =>
            DbContexts.Select(t => t.ClassViewModel)
            .Union(ClientClasses).Union(Services);

        public ReflectionRepository()
        {
        }

        private HashSet<string>? _rootTypeWhitelist = null;
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
                    .Where(type => 
                        !type.IsInternalUse &&
                        (type.HasAttribute<CoalesceAttribute>() || type.IsA(typeof(GeneratedParameterDto<>)))
                    )
                );
            }
        }

        public void AddTypes(IEnumerable<TypeViewModel> types)
        {
            foreach (var type in types) GetOrAddType(type);
        }

        public SymbolTypeViewModel GetOrAddType(ITypeSymbol symbol) =>
            GetOrAddType(symbol, () => new SymbolTypeViewModel(this, symbol));

        public ReflectionTypeViewModel GetOrAddType(Type type)
        {
            // Hot path during runtime - perform the cache check before entering GetOrAddType
            // which requires allocating a closure.
            if (_allTypeViewModels.TryGetValue(type, out var existing)) return (ReflectionTypeViewModel)existing;

            // Avoid closure on fast path by storing state into scoped locals.
            // Technique from https://github.com/dotnet/runtime/blob/4aa4d28f951babd9b26c2e4cff99a3203c56aee8/src/libraries/Microsoft.Extensions.Options/src/OptionsManager.cs#L48
            var localThis = this;
            var localType = type;

            return GetOrAddType(localType, () => new ReflectionTypeViewModel(localThis, localType));
        }

        public TypeViewModel GetOrAddType(TypeViewModel type) => 
            GetOrAddType(GetCacheKey(type), () => type);

        private T GetOrAddType<T>(object key, Func<T> factory)
            where T : TypeViewModel
        {
            // First, see if we already have the type. Do not invoke the factory for this check.
            if (_allTypeViewModels.TryGetValue(key, out var existing)) return (T)existing;

            var newType = factory();
            if (_allTypeViewModels.TryAdd(key, newType))
            {
                ProcessAddedType(newType);
            }

            // Re-fetch, because this isn't strictly the one that we maybe just added.
            return (T)_allTypeViewModels[key];
        }

        private void ProcessAddedType(TypeViewModel type)
        {
            var generatedDtoEntity = type.GenericArgumentsFor(typeof(GeneratedParameterDto<>))?[0];
            if (generatedDtoEntity?.ClassViewModel is ClassViewModel cvm)
            {
                _generatedParamDtos[cvm] = type.ClassViewModel!;
            }

            if (!type.HasAttribute<CoalesceAttribute>())
            {
                return;
            }

            if (_rootTypeWhitelist != null && !_rootTypeWhitelist.Contains(type.Name))
            {
                return;
            }

            // Null this out so it gets recomputed on next access.
            _clientTypes = null;
            _clientMethods = null;

            if (type.IsA<DbContext>())
            {
                var context = new DbContextTypeUsage(type.ClassViewModel!);

                var entityCvms = context.Entities.Select(e => GetOrAddType(e.TypeViewModel).ClassViewModel!);

                _contexts.Add(context);
                _entities.AddRange(entityCvms);
                // Some of our entities may have gotten discovered as external types already.
                // Remove them from that set now that we know they're entities
                _externalTypes.RemoveRange(entityCvms);

                // Null this out so it gets recomputed on next access.
                _entityUsages = null;

                foreach (var entity in context.Entities)
                {
                    DiscoverOnApiBackedClass(entity.TypeViewModel.ClassViewModel!);
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
                var classViewModel = type.ClassViewModel!;
                _customDtos.Add(classViewModel);
                DiscoverOnApiBackedClass(classViewModel);
            }
            else if (type.ClassViewModel?.IsService ?? false)
            {
                var classViewModel = type.ClassViewModel;
                _services.Add(classViewModel);
                DiscoverExternalMethodTypesOn(classViewModel);
            }
            else if (type.ClassViewModel?.IsStandaloneEntity ?? false)
            {
                var classViewModel = type.ClassViewModel;
                _entities.Add(classViewModel);
                _externalTypes.Remove(classViewModel);
                DiscoverOnApiBackedClass(classViewModel);
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
        public void AddAssembly<T>() => AddAssembly(typeof(T).Assembly);

        /// <summary>
        /// Adds types from the given assembly.
        /// </summary>
        public void AddAssembly(Assembly assembly) =>
            DiscoverCoalescedTypes(assembly.ExportedTypes.Select(t => new ReflectionTypeViewModel(this, t)));


        private object GetCacheKey(TypeViewModel typeViewModel) =>
            (typeViewModel as ReflectionTypeViewModel)?.Info as object
            ?? (typeViewModel as SymbolTypeViewModel)?.Symbol as object
            ?? throw new NotSupportedException("Unknown subtype of TypeViewModel");

        /// <summary>
        /// Attempt to add the given ClassViewModel as an ExternalType if it isn't already known.
        /// If its a newly discovered type, recurse into that type's properties as well.
        /// </summary>
        private void ConditionallyAddAndDiscoverTypesOn(ValueViewModel typeUsage)
        {
            var type = typeUsage.PureType;
            var classViewModel = type.ClassViewModel;
            if (classViewModel != null)
            {
                classViewModel.Usages.Add(typeUsage);

                // Don't dig in if:
                //  - This is a known entity type (its not external)
                //  - This is a known custom DTO type (again, not external)
                //  - This is already a known external type (don't infinitely recurse).
                if (
                    !Entities.Contains(classViewModel)
                    && !CustomDtos.Contains(classViewModel)
                    && !ExternalTypes.Contains(classViewModel)
                    )
                {
                    if (_externalTypes.Add(classViewModel))
                    {
                        DiscoverExternalPropertyTypesOn(classViewModel);
                    }
                }
            }
            else if (type.IsEnum)
            {
                _enums.Add(type.NullableValueUnderlyingType);
            }
        }

        private void DiscoverExternalPropertyTypesOn(ClassViewModel model)
        {
            foreach (var prop in model.ClientProperties)
            {
                ConditionallyAddAndDiscoverTypesOn(prop);
            }
        }

        private void DiscoverExternalMethodTypesOn(ClassViewModel model)
        {
            foreach (var method in model.ClientMethods)
            {
                ConditionallyAddAndDiscoverTypesOn(method.Return);

                foreach (var arg in method.ApiParameters)
                {
                    ConditionallyAddAndDiscoverTypesOn(arg);
                }
            }
        }

        private bool AddCrudStrategy(
            Type iface,
            TypeViewModel strategyType,
            ConcurrentHashSet<CrudStrategyTypeUsage> set,
            ClassViewModel? declaredFor = null
        )
        {
            if (!strategyType.IsA(iface) || strategyType.ClassViewModel == null) return false;

            var servedType = strategyType.GenericArgumentsFor(iface)!.Single();
            var servedClass = servedType.ClassViewModel;
            if (servedClass == null)
            {
                throw new InvalidOperationException($"{servedType} is not a valid type argument for a {iface}.");
            }

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

            if (declaredFor.IsCustomDto && !servedClass.Equals(declaredFor.DtoBaseViewModel))
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
                        _enums.Add(parameter.PureType.NullableValueUnderlyingType);
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

        public ClassViewModel? GetClassViewModel(Type classType) =>
            GetOrAddType(classType).ClassViewModel;

        public ClassViewModel? GetClassViewModel(INamedTypeSymbol classType) =>
            GetOrAddType(classType).ClassViewModel;

        public ClassViewModel? GetClassViewModel<T>() => GetClassViewModel(typeof(T));


        /// <summary>
        /// Gets a propertyViewModel based on the property selector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyBySelector<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            // Nullability note - making a consession here that someone wouldn't do this
            // on something like .PropertyBySelector<string, int>(s => s.Length).
            var objModel = GetClassViewModel<T>() ?? throw new ArgumentException("Provided type T has no ClassViewModel.");
            return objModel.PropertyBySelector(propertySelector);
        }

        public ClassViewModel? GetBehaviorsDeclaredFor(ClassViewModel declaredFor)
        {
            return Behaviors
                .SingleOrDefault(usage => usage.DeclaredFor == declaredFor)
                ?.StrategyClass;
        }
    }
}
