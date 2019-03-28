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

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionRepository
    {
        public static readonly ReflectionRepository Global = new ReflectionRepository();

        private readonly HashSet<DbContextTypeUsage> _contexts = new HashSet<DbContextTypeUsage>();
        private readonly HashSet<ClassViewModel> _entities = new HashSet<ClassViewModel>();
        private readonly HashSet<CrudStrategyTypeUsage> _behaviors = new HashSet<CrudStrategyTypeUsage>();
        private readonly HashSet<CrudStrategyTypeUsage> _dataSources = new HashSet<CrudStrategyTypeUsage>();
        private readonly HashSet<ClassViewModel> _externalTypes = new HashSet<ClassViewModel>();
        private readonly HashSet<ClassViewModel> _customDtos = new HashSet<ClassViewModel>();
        private readonly HashSet<ClassViewModel> _services = new HashSet<ClassViewModel>();

        private readonly ConcurrentDictionary<object, ClassViewModel> _allClassViewModels
            = new ConcurrentDictionary<object, ClassViewModel>();

        public ReadOnlyHashSet<DbContextTypeUsage> DbContexts => new ReadOnlyHashSet<DbContextTypeUsage>(_contexts);
        public ReadOnlyHashSet<ClassViewModel> Entities => new ReadOnlyHashSet<ClassViewModel>(_entities);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> Behaviors => new ReadOnlyHashSet<CrudStrategyTypeUsage>(_behaviors);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> DataSources => new ReadOnlyHashSet<CrudStrategyTypeUsage>(_dataSources);
        public ReadOnlyHashSet<ClassViewModel> ExternalTypes => new ReadOnlyHashSet<ClassViewModel>(_externalTypes);
        public ReadOnlyHashSet<ClassViewModel> CustomDtos => new ReadOnlyHashSet<ClassViewModel>(_customDtos);
        public ReadOnlyHashSet<ClassViewModel> Services => new ReadOnlyHashSet<ClassViewModel>(_services);

        public IEnumerable<ClassViewModel> ApiBackedClasses => Entities.Union(CustomDtos);

        public IEnumerable<ClassViewModel> ClientClasses => ApiBackedClasses.Union(ExternalTypes);

        public IEnumerable<TypeViewModel> ClientEnums => this.ClientClasses
            .SelectMany(c => c.ClientProperties.Select(p => p.Type.NullableUnderlyingType).Where(t => t.IsEnum))
            .Distinct();

        public IEnumerable<ClassViewModel> DiscoveredClassViewModels =>
            DbContexts.Select(t => t.ClassViewModel)
            .Union(ClientClasses);

        public ReflectionRepository()
        {
        }

        internal void DiscoverCoalescedTypes(IEnumerable<TypeViewModel> types)
        {
            foreach (var type in types
                // For some reason, attribute checking can be really slow. We're talking ~350ms to determine that the DbContext type has a [Coalesce] attribute.
                // Really not sure why, but lets parallelize to minimize that impact.
                .AsParallel()
                .Where(type => type.HasAttribute<CoalesceAttribute>())
            )
            {
                if (type.IsA<DbContext>())
                {
                    var context = new DbContextTypeUsage(type.ClassViewModel);
                    _contexts.Add(context);
                    _entities.UnionWith(context.Entities.Select(e => e.ClassViewModel));

                    // Force cache these since they have extra bits of info attached now.
                    // TODO: eliminate the need for this.
                    foreach (var e in context.Entities) Cache(e.ClassViewModel, force: true);

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

                    // Force cache this since it has extra bits of info attached.
                    _customDtos.Add(Cache(classViewModel, force: true));

                    DiscoverNestedCrudStrategiesOn(classViewModel);
                }
                else if (type.ClassViewModel?.IsService ?? false)
                {
                    var classViewModel = type.ClassViewModel;
                    _services.Add(Cache(classViewModel));
                }
            }

            foreach (var entity in Entities)
            {
                DiscoverExternalMethodTypesOn(entity);
                DiscoverExternalPropertyTypesOn(entity);
                DiscoverNestedCrudStrategiesOn(entity);
            }
            foreach (var service in Services)
            {
                DiscoverExternalMethodTypesOn(service);
            }
        }

        /// <summary>
        /// Adds types from the assembly that defines the given type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal void AddAssembly<T>() =>
            DiscoverCoalescedTypes(typeof(T).Assembly.ExportedTypes.Select(t => new ReflectionTypeViewModel(t)));

        /// <summary>
        /// Cache the given model so it can be reused when an instance representing its underlying type is requested.
        /// </summary>
        /// <param name="classViewModel">The ClassViewModel to cache.</param>
        /// <param name="force">
        /// True to override any existing object for the underlying type. 
        /// False to preserve any existing object.
        /// </param>
        /// <returns>The ClassViewModel that was passed in, for convenience.</returns>
        private ClassViewModel Cache(ClassViewModel classViewModel, bool force = false)
        {
            object key = GetCacheKey(classViewModel);

            if (force)
                _allClassViewModels[key] = classViewModel;
            else
                _allClassViewModels.GetOrAdd(key, classViewModel);

            return classViewModel;
        }

        private object GetCacheKey(ClassViewModel classViewModel) => 
            (classViewModel.Type as ReflectionTypeViewModel)?.Info as object
            ?? (classViewModel.Type as SymbolTypeViewModel)?.Symbol as object
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
                if (_externalTypes.Add(Cache(externalType)))
                {
                    DiscoverExternalPropertyTypesOn(externalType);
                }
            }
        }

        private void DiscoverExternalPropertyTypesOn(ClassViewModel model)
        {
            foreach (var type in model
                .ClientProperties
                .Select(p => p.PureType)
                .Where(t => t.HasClassViewModel))
            {
                ConditionallyAddAndDiscoverExternalPropertyTypesOn(type.ClassViewModel);
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

                foreach (var arg in method.Parameters.Where(p => !p.IsDI && p.Type.HasClassViewModel))
                {
                    // Parameter looks like an external type.
                    ConditionallyAddAndDiscoverExternalPropertyTypesOn(arg.Type.ClassViewModel);
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
            var servedClass = Cache(servedType.ClassViewModel);

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

            set.Add(new CrudStrategyTypeUsage(Cache(strategyType.ClassViewModel), servedClass, declaredFor));
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
            _allClassViewModels.GetOrAdd(classType, _ => new ReflectionClassViewModel(classType));

        public ClassViewModel GetClassViewModel(INamedTypeSymbol classType) =>
            _allClassViewModels.GetOrAdd(classType, _ => new SymbolClassViewModel(classType));

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
