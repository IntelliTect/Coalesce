using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using System.Collections.Concurrent;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public static class ReflectionRepository
    {
        private static ConcurrentDictionary<string, ClassViewModel> _models = new ConcurrentDictionary<string, ClassViewModel>();
        private static object _lock = new object();
        private static string _contextNamespace;


        public static ClassViewModel GetClassViewModel(TypeViewModel classType)
        {
            if (classType.Wrapper is ReflectionTypeWrapper)
            {
                return GetClassViewModel(classType.Wrapper.Info);
            }
            else
            {
                return GetClassViewModel((INamedTypeSymbol)classType.Wrapper.Symbol);
            }
        }

        public static ClassViewModel GetClassViewModel(Type classType)
        {
            return _models.GetOrAdd(GetKey(classType), _ => new ClassViewModel(classType));
        }

        public static ClassViewModel GetClassViewModel(INamedTypeSymbol classType)
        {
            return _models.GetOrAdd(GetKey(classType), _ => new ClassViewModel(classType));
        }
        

        public static ClassViewModel GetClassViewModel(string className)
        {
            return _models.Values.FirstOrDefault(f => f.Name == className);
        }

        public static ClassViewModel GetClassViewModel<T>()
        {
            return GetClassViewModel(typeof(T));
        }

        /// <summary>
        /// Gets a propertyViewModel based on the property selector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static PropertyViewModel PropertyBySelector<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            var objModel = GetClassViewModel<T>();
            return objModel.PropertyBySelector(propertySelector);
        }

        public static PropertyViewModel PropertyBySelector(LambdaExpression propertySelector)
        {
            var objModel = GetClassViewModel(propertySelector.Parameters.First().Type);
            return objModel.PropertyByName(propertySelector.GetExpressedProperty(objModel.Type).Name);
        }

        public static bool IsValidViewModelClass(string name)
        {
            if (name == "Image") return false;
            if (name == "IdentityUserRole") return false;
            if (name == "IdentityRole") return false;
            if (name == "IdentityRoleClaim") return false;
            if (name == "IdentityUserClaim") return false;
            if (name == "IdentityUserLogin") return false;
            if (name == "IdentityUserToken") return false;
            return true;
        }



        /// <summary>
        /// Gets a unique key for the collection.
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        private static string GetKey(TypeViewModel type)
        {
            if (type.Wrapper is Wrappers.ReflectionTypeWrapper)
            {
                return GetKey(((ReflectionTypeWrapper)(type.Wrapper)).Info);
            }
            else
            {
                return GetKey((INamedTypeSymbol)(((SymbolTypeWrapper)(type.Wrapper)).Symbol));
            }
        }
        /// <summary>
        /// Gets a unique key for the collection.
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        private static string GetKey(Type classType)
        {
            return string.Format("{0}", classType.FullName);
        }

        /// <summary>
        /// Gets a unique key for the collection.
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>

        private static string GetKey(INamedTypeSymbol classType)
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

        public static IEnumerable<ClassViewModel> Models => _models.Values;

        /// <summary>
        /// Adds a context to the reflection repository. Do this on startup with all the contexts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<ClassViewModel> AddContext<T>() where T : DbContext
        {
            return AddContext(typeof(T));
        }


        /// <summary>
        /// Adds a context to the reflection repository. Do this on startup with all the contexts.
        /// </summary>
        /// <typeparam name="t">Type of DbContext to add.</typeparam>
        /// <returns></returns>
        public static List<ClassViewModel> AddContext(Type t)
        {
            var context = new ClassViewModel(t);
            return AddContext(context);
        }

        public static List<ClassViewModel> AddContext(INamedTypeSymbol contextSymbol) // where T: AppDbContext
        {
            var context = new ClassViewModel(contextSymbol);
            // Reflect on the AppDbContext
            return AddContext(context);
        }

        public static List<ClassViewModel> AddContext(ClassViewModel context)
        {
            _contextNamespace = context.Namespace;
            // Lock so that parallel execution only uses this once at a time.
            lock (_lock)
            {
                var models = new List<ClassViewModel>();
                foreach (var prop in context.Properties)
                {
                    if ((prop.Type.IsCollection || prop.IsDbSet)
                        && IsValidViewModelClass(prop.PureType.Name)
                        && !prop.IsInternalUse)
                    {
                        var model = ReflectionRepository.GetClassViewModel(prop.PureType);

                        if (model != null)
                        {
                            model.HasDbSet = prop.IsDbSet;
                            model.ContextPropertyName = prop.Name;
                            model.OnContext = true;
                            model.ContextPropertyName = prop.Name;
                            models.Add(model);
                        }
                    }
                }
                // Check for other associated types that are not dbsets
                foreach (var model in models.ToList())
                {
                    AddChildModels(models, model);
                }

                return models;
            }

        }

        private static void AddChildModels(List<ClassViewModel> models, ClassViewModel model)
        {
            foreach (var prop in model.Properties.Where(p => !p.IsInternalUse && p.PureType.IsPOCO && IsValidViewModelClass(p.PureType.Name)))
            {
                var propModel = prop.PureType.ClassViewModel;
                if (propModel != null && !propModel.HasDbSet && !models.Contains(propModel))
                {
                    models.Add(propModel);
                    AddChildModels(models, propModel);
                }
            }
            foreach (var method in model.Methods.Where(p => !p.IsInternalUse && !p.ReturnType.IsVoid && p.ReturnType.PureType.IsPOCO))
            {
                lock (models)
                {
                    if (!models.Any(f => f.Name == method.ReturnType.PureType.Name))
                    {
                        var methodModel = new ClassViewModel(method.ReturnType.PureType);
                        models.Add(methodModel);
                        AddChildModels(models, methodModel);
                    }
                }
                // Iterate each of the incoming arguments and check them
                foreach (var arg in method.Parameters.Where(p => !p.IsDI && p.Type.IsPOCO))
                {
                    string argKey = GetKey(arg.Type);
                    lock (models)
                    {
                        if (!models.Any(f => f.Name == arg.Type.Name))
                        {
                            var argModel = new ClassViewModel(arg.Type);
                            models.Add(argModel);
                            AddChildModels(models, argModel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// All the namespaces in the models.
        /// </summary>
        public static IEnumerable<string> Namespaces
        {
            get
            {
                var result = _models.Select(f => f.Value.Namespace).Distinct().ToList();
                // Make sure we add the namespace of the context
                if (!result.Contains(_contextNamespace)) result.Add(_contextNamespace);
                return result;
            }
        }

    }

    internal class ClassViewModelComparer : IEqualityComparer<ClassViewModel>
    {
        public bool Equals(ClassViewModel x, ClassViewModel y)
        {
            if (x == y) return true;
            return x?.FullName.Equals(y?.FullName) ?? false;
        }

        public int GetHashCode(ClassViewModel obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}
