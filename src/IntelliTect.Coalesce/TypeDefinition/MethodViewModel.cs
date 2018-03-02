using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using IntelliTect.Coalesce.Models;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class MethodViewModel : IAttributeProvider
    {
        public abstract bool IsStatic { get; }

        public abstract string Comment { get; }

        public abstract string Name { get; }

        public abstract TypeViewModel ReturnType { get; }
        
        public bool ReturnsListResult => ReturnType.IsA(typeof(ListResult<>));

        /// <summary>
        /// The return type of the method, discounting any <see cref="ItemResult{T}"/> wrapped around it.
        /// </summary>
        public TypeViewModel ResultType =>
              ReturnsListResult
            ? ReturnType.ClassViewModel.PropertyByName(nameof(ListResult<object>.List)).Type // Will be a constructed generic IList<T>
            : ReturnType.IsA(typeof(ItemResult<>))
            ? ReturnType.PureType
            : ReturnType.IsA(typeof(ItemResult))
            ? new ReflectionTypeViewModel(typeof(void))
            : ReturnType;

        public abstract IEnumerable<ParameterViewModel> Parameters { get; }

        /// <summary>
        /// Returns true if this method is marked as InternalUse. Not exposed through the API
        /// </summary>
        public virtual bool IsInternalUse => HasAttribute<InternalUseAttribute>();

        public ClassViewModel Parent { get; }

        internal MethodViewModel(ClassViewModel parent)
        {
            Parent = parent;
        }

        public string ApiActionHttpMethodName => ApiActionHttpMethod.ToString().ToUpper();
        public string ApiActionHttpMethodAnnotation => $"Http{ApiActionHttpMethod.ToString()}";
        
        public HttpMethod ApiActionHttpMethod =>
            this.GetAttributeValue<ControllerActionAttribute, HttpMethod>(a => a.Method) ?? HttpMethod.Post;


        /// <summary>
        /// Convenient accessor for the MethodInfo when in reflection-based contexts.
        /// </summary>
        public virtual MethodInfo MethodInfo => throw new InvalidOperationException("MethodInfo not available in the current context");

        public string JsVariable => Name.ToCamelCase();

        /// <summary>
        /// Return type of the controller action for the method.
        /// </summary>
        public string ReturnTypeNameForApi
        {
            get
            {
                var resultType = ResultType;

                if (resultType.IsVoid)
                {
                    return nameof(ItemResult);
                }

                if (ReturnsListResult)
                {
                    return $"{nameof(ListResult<object>)}<{ReturnType.PureType.DtoFullyQualifiedName}>";
                }

                return $"{nameof(ItemResult)}<{resultType.DtoFullyQualifiedName}>";
            }
        }

        /// <summary>
        /// List of parameters that are not Dependency Injected (DI)
        /// </summary>
        public IEnumerable<ParameterViewModel> ClientParameters => Parameters.Where(f => !f.IsDI);

        public bool IsModelInstanceMethod => !IsStatic && !Parent.IsService;

        /// <summary>
        /// Gets the CS parameters for this method call.
        /// </summary>
        public string CsParameters
        {
            get
            {
                var parameters = Parameters.Where(f => !f.IsNonArgumentDI).ToArray();
                var outParameters = new List<string>();

                // For entity instance methods, add an id that specifies the object to work on, and a data source factory.
                if (IsModelInstanceMethod)
                {
                    outParameters.Add("[FromServices] IDataSourceFactory dataSourceFactory");
                    outParameters.Add($"{Parent.PrimaryKey.PureType.FullyQualifiedName} id");
                }
                outParameters.AddRange(parameters.Select(f => f.CsDeclaration));
                return string.Join(", ", outParameters);
            }
        }

        /// <summary>
        /// Gets the CS arguments passed to this method call.
        /// </summary>
        public string CsArguments => string.Join(", ", Parameters.Select(f => f.CsArgument));


        /// <summary>
        /// Gets the js arguments passed to this method call.
        /// </summary>
        public string JsArguments(string obj = "", bool callback = false)
        {
            string result;
            if (obj != "")
            {
                result = string.Join(", ", ClientParameters.Select(f => $"{obj}.{f.JsVariable}()"));
            }
            else
            {
                result = string.Join(", ", ClientParameters.Select(f => obj + f.JsVariable));
            }
            if (callback)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result = result + ", ";
                }
                result = result + "callback";
            }
            return result;
        }

        public string JsPostObject
        {
            get
            {
                var result = "{ ";
                if (IsModelInstanceMethod)
                {
                    result = result + "id: this.parent[this.parent.primaryKeyName]()";
                    if (Parameters.Any()) result = result + ", ";
                }

                string TsConversion(ParameterViewModel param)
                {
                    string argument = param.JsVariable;
                    if (param.Type.HasClassViewModel)
                        return $"{argument} ? {argument}.saveToDto() : null";
                    if (param.Type.IsDate)
                        return $"{argument} ? {argument}.format() : null";
                    return argument;
                }

                result += string.Join(", ", ClientParameters.Select(f => $"{f.JsVariable}: {TsConversion(f)}"));
                result += " }";
                return result;

            }
        }

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName =>
            this.GetAttributeValue<DisplayNameAttribute>(a => a.DisplayName) ??
            this.GetAttributeValue<DisplayAttribute>(a => a.Name) ??
            Regex.Replace(Name, "[A-Z]", " $0").Trim();

        /// <summary>
        /// For the specified area, returns true if the property has a hidden attribute.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool IsHidden(HiddenAttribute.Areas area)
        {
            var hiddenArea = this.GetAttributeValue<HiddenAttribute, HiddenAttribute.Areas>(a => a.Area);
            if (!hiddenArea.HasValue) return false;
            return hiddenArea.Value == HiddenAttribute.Areas.All || hiddenArea.Value == area;
        }

        public ExecuteSecurityInfo SecurityInfo => new ExecuteSecurityInfo(this.GetSecurityPermission<ExecuteAttribute>());

        /// <summary>
        /// If true, this is a method that may be called by a client.
        /// </summary>
        public bool IsClientMethod => !IsInternalUse && !SecurityInfo.Execute.NoAccess && 
            // Services only have instance methods - no static methods.
            (!Parent.IsService || !IsStatic) && 
            // Interface services always expose all their declared methods.
            ((Parent.IsService && Parent.Type.IsInterface) || HasAttribute<CoalesceAttribute>());

        public string LoadFromDataSourceName
        {
            get
            {
                var type = this.GetAttributeValue<LoadFromDataSourceAttribute>(a => a.DataSourceType);
                if (type == null) return Api.DataSources.DataSourceFactory.DefaultSourceName;
                return type.Name;
            }
        }

        public abstract object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;
    }
}
