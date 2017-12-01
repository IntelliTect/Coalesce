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
using IntelliTect.Coalesce.Interfaces;
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
        private HashSet<DataSourceTypeUsage> _dataSources = new HashSet<DataSourceTypeUsage>();
        private HashSet<ClassViewModel> _externalTypes = new HashSet<ClassViewModel>();
        private HashSet<ClassViewModel> _customDtos = new HashSet<ClassViewModel>();

        public ReadOnlyHashSet<DbContextTypeUsage> DbContexts => new ReadOnlyHashSet<DbContextTypeUsage>(_contexts);
        public ReadOnlyHashSet<DataSourceTypeUsage> DataSources => new ReadOnlyHashSet<DataSourceTypeUsage>(_dataSources);
        public ReadOnlyHashSet<ClassViewModel> ExternalTypes => new ReadOnlyHashSet<ClassViewModel>(_externalTypes);
        public ReadOnlyHashSet<ClassViewModel> CustomDtos => new ReadOnlyHashSet<ClassViewModel>(_customDtos);

        public IEnumerable<EntityTypeUsage> Entities => DbContexts.SelectMany(c => c.Entities);
        public IEnumerable<ClassViewModel> ApiBackedClasses => Entities.Select(e => e.ClassViewModel).Union(CustomDtos);

        public IEnumerable<ClassViewModel> AllClassViewModels =>
            DbContexts.Select(t => t.ClassViewModel)
            .Union(Entities.Select(c => c.ClassViewModel))
            .Union(ExternalTypes);

        public IEnumerable<TypeViewModel> AllTypeViewModels =>
            AllClassViewModels.Select(c => c.Type);

        public ReflectionRepository()
        {
        }

        public void DiscoverCoalescedTypes(IEnumerable<TypeViewModel> types)
        {
            foreach (var type in types)
            {
                if (type.HasAttribute<CoalesceAttribute>())
                {
                    if (type.IsA<DbContext>())
                    {
                        _contexts.Add(new DbContextTypeUsage(type.ClassViewModel));
                    }
                    else if (type.IsA(typeof(IDataSource<>)))
                    {
                        var genericArgs = type.GenericArgumentsFor(typeof(IDataSource<>));
                        var servedType = genericArgs.Single();
                        if (!servedType.HasClassViewModel)
                        {
                            throw new InvalidOperationException($"{servedType} is not a valid type argument for a data source.");
                        }

                        _dataSources.Add(new DataSourceTypeUsage(type, servedType.ClassViewModel));
                    }
                    else if (type.IsA(typeof(IClassDto<,>)))
                    {
                        var classViewModel = type.ClassViewModel;

                        // TODO: this is a lie, and is also terrible.
                        // Just trying to maintain the way that IClassDtos were identified before this overhaull.
                        classViewModel.OnContext = true;

                        _customDtos.Add(classViewModel);
                    }
                }
            }

            foreach (var entity in Entities)
            {
                DiscoverExternalMethodTypesOn(entity.ClassViewModel);
                DiscoverExternalPropertyTypesOn(entity.ClassViewModel);
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
        /// Attempt to add the given ClassViewModel as an ExternalType if it isn't already known.
        /// If its a newly discovered type, recurse into that type's properties as well.
        /// </summary>
        /// <param name="externalType"></param>
        private void ConditionallyAddAndDiscoverExternalPropertyTypesOn(ClassViewModel externalType)
        {
            if (!AllClassViewModels.Contains(externalType))
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

        // TODO: MAKE THIS O(1) AGAIN INSTEAD OF O(N)

        public ClassViewModel GetClassViewModel(Type classType) =>
            AllClassViewModels.FirstOrDefault(c => (c.Type as ReflectionTypeViewModel)?.Info.Equals(classType) ?? false)
            ?? new ReflectionClassViewModel(classType);

        // TODO: MAKE THIS O(1) AGAIN INSTEAD OF O(N)

        public ClassViewModel GetClassViewModel(ITypeSymbol classType) =>
            AllClassViewModels.FirstOrDefault(c => (c.Type as SymbolTypeViewModel)?.Symbol.Equals(classType) ?? false)
            ?? new SymbolClassViewModel(classType);

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

        public PropertyViewModel PropertyBySelector(LambdaExpression propertySelector)
        {
            var type = propertySelector.Parameters.First().Type;
            var objModel = GetClassViewModel(type);
            return objModel.PropertyByName(propertySelector.GetExpressedProperty(type).Name);
        }
        
        /// <summary>
        /// Gets a unique key for the collection.
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        private string GetKey(TypeViewModel type)
        {
            if (type is ReflectionTypeViewModel reflectedType)
            {
                return GetKey(reflectedType.Info);
            }
            else
            {
                return GetKey(((SymbolTypeViewModel)type).Symbol);
            }
        }

        /// <summary>
        /// Gets a unique key for the collection.
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        private string GetKey(Type classType) => classType.FullName;

        /// <summary>
        /// Gets a unique key for the collection.
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>

        private string GetKey(ITypeSymbol classType)
        {
            List<string> namespaces = new List<string>();
            var curNamespace = classType.ContainingNamespace;
            while (curNamespace.CanBeReferencedByName)
            {
                namespaces.Add(curNamespace.Name);
                curNamespace = curNamespace.ContainingNamespace;
            }
            namespaces.Reverse();

            var fullNamespace = string.Join(".", namespaces);

            return string.Format("{0}", $"{fullNamespace}.{classType.Name}");
        }

        // TODO: remove this pass-through property.
        public IEnumerable<ClassViewModel> Models => AllClassViewModels;

        private void AddChildModels(List<ClassViewModel> models, ClassViewModel model)
        {
            foreach (var prop in model.ClientProperties.Where(p => p.PureType.IsPOCO))
            {
                var propModel = prop.PureType.ClassViewModel;
                if (propModel != null && !propModel.HasDbSet && !models.Contains(propModel))
                {
                    models.Add(propModel);
                    AddChildModels(models, propModel);
                }
            }
            foreach (var method in model.ClientMethods.Where(p => !p.ReturnType.IsVoid && p.ReturnType.PureType.HasClassViewModel))
            {
                lock (models)
                {
                    if (!models.Any(f => f.Name == method.ReturnType.PureType.Name))
                    {
                        var methodModel = method.ReturnType.PureType.ClassViewModel;
                        models.Add(methodModel);
                        AddChildModels(models, methodModel);
                    }
                }
                // Iterate each of the incoming arguments and check them
                foreach (var arg in method.Parameters.Where(p => !p.IsDI && p.Type.HasClassViewModel))
                {
                    string argKey = GetKey(arg.Type);
                    lock (models)
                    {
                        if (!models.Any(f => f.Name == arg.Type.Name))
                        {
                            var argModel = arg.Type.ClassViewModel;
                            models.Add(argModel);
                            AddChildModels(models, argModel);
                        }
                    }
                }
            }
        }
    }
}
