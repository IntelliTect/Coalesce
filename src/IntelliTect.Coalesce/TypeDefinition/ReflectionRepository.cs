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

namespace IntelliTect.Coalesce.TypeDefinition
{
    public static class ReflectionRepository
    {
        private static Dictionary<string, ClassViewModel> _models = new Dictionary<string, ClassViewModel>();
        private static object _lock = new object();


        public static ClassViewModel GetClassViewModel(TypeViewModel classType, string controllerName = null,
            string apiName = null, bool hasDbSet = false, string area = "")
        {
            if (classType.Wrapper is ReflectionTypeWrapper)
            {
                return GetClassViewModel(classType.Wrapper.Info, controllerName, apiName, hasDbSet, area);
            }
            else
            {
                return GetClassViewModel((INamedTypeSymbol)classType.Wrapper.Symbol, controllerName, apiName, hasDbSet, area);
            }
        }

        public static ClassViewModel GetClassViewModel(Type classType, string controllerName = null,
            string apiName = null, bool hasDbSet = false, string area = "")
        {
            // Set defaults
            if (controllerName == null) controllerName = classType.Name;
            if (apiName == null) apiName = classType.Name;

            if (!IsValidViewModelClass(apiName)) return null;

            // Get a unique key
            //TODO: The apiName probably don't need to be specified here, can cause problems where it isn't available.
            string key = GetKey(classType);
            // Create it if is doesn't exist.
            // TODO: This could be better multi-threading code
            lock (_models)
            {
                if (!_models.ContainsKey(key))
                {
                    _models.Add(key, new ClassViewModel(classType, controllerName, apiName, hasDbSet));
                }
            }
            // Return the class requested.
            return _models[key];
        }

        public static ClassViewModel GetClassViewModel(INamedTypeSymbol classType, string controllerName = null,
            string apiName = null, bool hasDbSet = false, string area = "")
        {
            // Set defaults
            if (controllerName == null) controllerName = classType.Name;
            if (apiName == null) apiName = classType.Name;

            if (!IsValidViewModelClass(apiName)) return null;

            // Get a unique key
            string key = GetKey(classType);
            // Create it if is doesn't exist.
            // TODO: This could be better multi-threading code
            lock (_models)
            {
                if (!_models.ContainsKey(key))
                {
                    _models.Add(key, new ClassViewModel(classType, controllerName, apiName, hasDbSet));
                }
            }
            // Return the class requested.
            return _models[key];
        }



        public static ClassViewModel GetClassViewModel(string className)
        {
            return _models.FirstOrDefault(f => f.Value.Name == className).Value;
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

        public static bool IsValidViewModelClass(string name)
        {
            if (name == "Image") return false;
            if (name == "IdentityUserRole") return false;
            if (name == "IdentityRole") return false;
            if (name == "IdentityRoleClaim") return false;
            if (name == "IdentityUserClaim") return false;
            if (name == "IdentityUserLogin") return false;
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
            return string.Format("{0}", $"{classType.ContainingNamespace.Name}.{classType.Name}");
        }

        public static IEnumerable<ClassViewModel> Models
        {
            get
            {
                return _models.Values;
            }
        }

        /// <summary>
        /// Adds a context to the reflection repository. Do this on startup with all the contexts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<ClassViewModel> AddContext<T>(string area = "") where T : DbContext
        {
            var context = new ClassViewModel(typeof(T), null, null, false);
            return AddContext(context, area);
        }

        public static List<ClassViewModel> AddContext(INamedTypeSymbol contextSymbol, string area = "") // where T: AppDbContext
        {
            var context = new ClassViewModel(contextSymbol, null, null, false);
            // Reflect on the AppDbContext
            return AddContext(context, area);
        }

        public static List<ClassViewModel> AddContext(ClassViewModel context, string area = "")
        {
            // Lock so that parallel execution only uses this once at a time.
            lock (_lock)
            {
                var models = new List<ClassViewModel>();
                foreach (var prop in context.Properties)
                {
                    if ((prop.Type.IsCollection || prop.IsDbSet) && IsValidViewModelClass(prop.PureType.Name))
                    {
                        var model = ReflectionRepository.GetClassViewModel(classType: prop.PureType, hasDbSet: prop.IsDbSet);
                        if (model != null)
                        {
                            model.OnContext = true;
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
            foreach (var prop in model.Properties.Where(p => p.IsPOCO && IsValidViewModelClass(p.PureType.Name)))
            {
                var propModel = prop.PureType.ClassViewModel;
                if (propModel != null && !propModel.HasDbSet && !models.Contains(propModel))
                {
                    models.Add(propModel);
                    AddChildModels(models, propModel);
                }
            }
            foreach (var prop in model.Methods.Where(p => !p.IsInternalUse && !p.ReturnType.IsVoid && p.ReturnType.PureType.IsPOCO))
            {
                // Get a unique key
                string key = GetKey(prop.ReturnType.PureType);
                lock (models)
                {
                    if (!models.Any(f => f.Name == prop.ReturnType.PureType.Name))
                    {
                        var methodModel = new ClassViewModel(prop.ReturnType.PureType, "", "", false);
                        models.Add(methodModel);
                        AddChildModels(models, methodModel);
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
                var result = _models.Select(f => f.Value.Namespace).Distinct();
                return result;
            }
        }

    }
}
