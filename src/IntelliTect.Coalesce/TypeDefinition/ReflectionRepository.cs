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

namespace IntelliTect.Coalesce.TypeDefinition
{
    public enum TypeCategory
    {
        Unknown = 0,
        DbContext = 1,
        Entity = 2,
        DataSource = 3,
        Behaviors = 4,
        Dto = 5,
        ExternalType = 6,
    }

    public class ReflectionRepository
    {
        public static readonly ReflectionRepository Global = new ReflectionRepository();

        private object _lock = new object();

        private HashSet<DbContextTypeUsage> _contexts = new HashSet<DbContextTypeUsage>();
        private HashSet<ClassViewModel> _entities = new HashSet<ClassViewModel>();
        private HashSet<CrudStrategyTypeUsage> _behaviors = new HashSet<CrudStrategyTypeUsage>();
        private HashSet<CrudStrategyTypeUsage> _dataSources = new HashSet<CrudStrategyTypeUsage>();
        private HashSet<ClassViewModel> _externalTypes = new HashSet<ClassViewModel>();
        private HashSet<ClassViewModel> _customDtos = new HashSet<ClassViewModel>();

        public ReadOnlyHashSet<DbContextTypeUsage> DbContexts => new ReadOnlyHashSet<DbContextTypeUsage>(_contexts);
        public ReadOnlyHashSet<ClassViewModel> Entities => new ReadOnlyHashSet<ClassViewModel>(_entities);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> Behaviors => new ReadOnlyHashSet<CrudStrategyTypeUsage>(_behaviors);
        public ReadOnlyHashSet<CrudStrategyTypeUsage> DataSources => new ReadOnlyHashSet<CrudStrategyTypeUsage>(_dataSources);
        public ReadOnlyHashSet<ClassViewModel> ExternalTypes => new ReadOnlyHashSet<ClassViewModel>(_externalTypes);
        public ReadOnlyHashSet<ClassViewModel> CustomDtos => new ReadOnlyHashSet<ClassViewModel>(_customDtos);

        public IEnumerable<ClassViewModel> ApiBackedClasses => Entities.Union(CustomDtos);

        public IEnumerable<ClassViewModel> DiscoveredClassViewModels =>
            DbContexts.Select(t => t.ClassViewModel)
            .Union(ApiBackedClasses)
            .Union(ExternalTypes);

        private ConcurrentDictionary<object, ClassViewModel> _allClassViewModels
            = new ConcurrentDictionary<object, ClassViewModel>();

        public ReflectionRepository()
        {
        }

        public void DiscoverCoalescedTypes(IEnumerable<TypeViewModel> types)
        {
            foreach (var type in types)
            {
                if (type.HasAttribute<CoalesceAttribute>())
                {
                    bool AddCrudStrategy(Type iface, HashSet<CrudStrategyTypeUsage> set)
                    {
                        if (!type.IsA(iface)) return false;

                        var servedType = type.GenericArgumentsFor(iface).Single();
                        if (!servedType.HasClassViewModel)
                        {
                            throw new InvalidOperationException($"{servedType} is not a valid type argument for a data source.");
                        }
                        var servedClass = servedType.ClassViewModel;
                        set.Add(new CrudStrategyTypeUsage(Cache(type.ClassViewModel), servedClass, servedClass));
                        return true;
                    }

                    if (type.IsA<DbContext>())
                    {
                        var context = new DbContextTypeUsage(type.ClassViewModel);
                        _contexts.Add(context);
                        _entities.UnionWith(context.Entities.Select(e => e.ClassViewModel));

                        // Force cache these since they have extra bits of info attached now.
                        // TODO: eliminate the need for this.
                        foreach (var e in context.Entities) Cache(e.ClassViewModel, force: true);
                    }
                    else if (AddCrudStrategy(typeof(IDataSource<>), _dataSources))
                    {
                        // Handled by helper
                    }
                    else if (AddCrudStrategy(typeof(IBehaviors<>), _behaviors))
                    {
                        // Handled by helper
                    }
                    else if (type.IsA(typeof(IClassDto<>)))
                    {
                        var classViewModel = type.ClassViewModel;

                        // TODO: this is a lie, and is also terrible.
                        // Just trying to maintain the way that IClassDtos were identified before this overhaul.
                        // This property is primarily used to determine if there is an API controller serving this data.
                        classViewModel.OnContext = true;

                        // Force cache this since it has extra bits of info attached.
                        _customDtos.Add(Cache(classViewModel, force: true));

                        DiscoverNestedCrudStrategiesOn(classViewModel, classViewModel.DtoBaseViewModel);
                    }
                }
            }

            foreach (var entity in Entities)
            {
                DiscoverExternalMethodTypesOn(entity);
                DiscoverExternalPropertyTypesOn(entity);
                DiscoverNestedCrudStrategiesOn(entity);
            }
        }

        /// <summary>
        /// Adds types from the assembly that defines the given type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void AddAssembly<T>() =>
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
            ?? throw new NotImplementedException("Unknown subtype of TypeViewModel");


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
                var returnType = method.ReturnType.PureType;
                if (returnType.HasClassViewModel)
                {
                    // Return type looks like an external type.
                    ConditionallyAddAndDiscoverExternalPropertyTypesOn(returnType.ClassViewModel);
                }

                foreach (var arg in method.Parameters.Where(p => !p.IsDI && p.Type.HasClassViewModel))
                {
                    // Parameter looks like an external type.
                    // TODO: this doesn't actually give us anything,
                    // because the generated typescript doesn't know how to call methods that have non-primitive properties.
                    ConditionallyAddAndDiscoverExternalPropertyTypesOn(arg.Type.ClassViewModel);
                }
            }
        }

        private void DiscoverNestedCrudStrategiesOn(ClassViewModel model, ClassViewModel assertSourceFor = null)
        {
            assertSourceFor = assertSourceFor ?? model;


            bool AddCrudStrategy(TypeViewModel nestedType, Type iface, HashSet<CrudStrategyTypeUsage> set)
            {
                if (!nestedType.IsA(iface)) return false;

                var servedType = nestedType.GenericArgumentsFor(iface).Single();
                if (!servedType.HasClassViewModel)
                {
                    throw new InvalidOperationException($"{servedType} is not a valid type argument for a {iface}.");
                }
                var servedClass = Cache(servedType.ClassViewModel);

                if (!servedClass.Equals(assertSourceFor))
                {
                    throw new InvalidOperationException($"{nestedType} is not a valid {iface} for {model} - " +
                        $"{nestedType} must satisfy {iface} with type parameter <{assertSourceFor}>.");
                }

                set.Add(new CrudStrategyTypeUsage(Cache(nestedType.ClassViewModel), servedClass, model));
                return true;
            }


            foreach (var nestedType in model.ClientNestedTypes)
            {
                AddCrudStrategy(nestedType, typeof(IDataSource<>), _dataSources);
                AddCrudStrategy(nestedType, typeof(IBehaviors<>), _behaviors);
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
