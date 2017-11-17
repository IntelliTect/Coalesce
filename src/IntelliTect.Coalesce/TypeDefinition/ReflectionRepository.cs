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
    public class ReflectionRepository
    {
        public static readonly ReflectionRepository Global = new ReflectionRepository();

        private ConcurrentDictionary<string, ClassViewModel> _models = new ConcurrentDictionary<string, ClassViewModel>();
        private object _lock = new object();
        private string _contextNamespace;


        public ClassViewModel GetClassViewModel(TypeViewModel classType)
        {
            return classType.ClassViewModel;
        }

        public ClassViewModel GetClassViewModel(Type classType)
        {
            return _models.GetOrAdd(GetKey(classType), _ => new ReflectionClassViewModel(classType));
        }

        public ClassViewModel GetClassViewModel(ITypeSymbol classType)
        {
            return _models.GetOrAdd(GetKey(classType), _ => new SymbolClassViewModel(classType));
        }
        

        public ClassViewModel GetClassViewModel(string className)
        {
            return _models.Values.FirstOrDefault(f => f.Name == className);
        }

        public ClassViewModel GetClassViewModel<T>()
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

        public bool IsValidViewModelClass(string name)
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

        public IEnumerable<ClassViewModel> Models => _models.Values;

        /// <summary>
        /// Adds a context to the reflection repository. Do this on startup with all the contexts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<ClassViewModel> AddContext<T>() where T : DbContext => AddContext(typeof(T));
        public List<ClassViewModel> AddContext(Type t) => AddContext(new ReflectionClassViewModel(t));
        public List<ClassViewModel> AddContext(TypeViewModel t) => AddContext(t.ClassViewModel);
        public List<ClassViewModel> AddContext(INamedTypeSymbol contextSymbol) => AddContext(new SymbolClassViewModel(contextSymbol));

        public List<ClassViewModel> AddContext(ClassViewModel context)
        {
            _contextNamespace = context.Namespace;
            // Lock so that parallel execution only uses this once at a time.
            lock (_lock)
            {
                var models = new List<ClassViewModel>();
                foreach (var prop in context.ClientProperties)
                {
                    if ((prop.Type.IsCollection || prop.IsDbSet) && IsValidViewModelClass(prop.PureType.Name))
                    {
                        var model = this.GetClassViewModel(prop.PureType);

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

        private void AddChildModels(List<ClassViewModel> models, ClassViewModel model)
        {
            foreach (var prop in model.ClientProperties.Where(p => p.PureType.IsPOCO && IsValidViewModelClass(p.PureType.Name)))
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

        /// <summary>
        /// All the namespaces in the models.
        /// </summary>
        public IEnumerable<string> Namespaces
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
