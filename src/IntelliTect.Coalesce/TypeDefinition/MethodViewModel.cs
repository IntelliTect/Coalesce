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
using static IntelliTect.Coalesce.DataAnnotations.ApiActionHttpMethodAttribute;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class MethodViewModel : IAttributeProvider
    {
        public abstract bool IsStatic { get; }

        public abstract string Comment { get; }

        public abstract string Name { get; }

        public abstract TypeViewModel ReturnType { get; }

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
            this.GetAttributeValue<ApiActionHttpMethodAttribute, ApiActionHttpMethodAttribute.HttpMethod>(a => a.Method) ?? HttpMethod.Post;


        /// <summary>
        /// Convenient accessor for the MethodInfo when in reflection-based contexts.
        /// </summary>
        public virtual MethodInfo MethodInfo => throw new InvalidOperationException("MethodInfo not available in the current context");

        public string JsVariable => Name.ToCamelCase();

        /// <summary>
        /// Type of the return. Object if void.
        /// </summary>
        public string ReturnTypeNameForApi
        {
            get
            {
                string result = ReturnType.FullyQualifiedName;
                if (result == "void") return "object";
                result = result.Replace("IQueryable", "IEnumerable");
                if (ReturnType.IsCollection && ReturnType.PureType.HasClassViewModel)
                {
                    // We can just straight replace this since the fully qualified name
                    // that we're replacing should never be a substring of any larger name.
                    // If this were a possibility, then we would run the risk of clobbering other names.
                    result = result.Replace(
                        ReturnType.PureType.ClassViewModel.FullyQualifiedName,
                        ReturnType.PureType.ClassViewModel.DtoName);
                }
                else if (!ReturnType.IsCollection && ReturnType.HasClassViewModel)
                {
                    result = $"{ReturnType.ClassViewModel.DtoName}";
                }
                return result;
            }
        }

        /// <summary>
        /// List of parameters that are not Dependency Injected (DI)
        /// </summary>
        public IEnumerable<ParameterViewModel> ClientParameters => Parameters.Where(f => !f.IsDI);

        /// <summary>
        /// Gets the TypeScript parameters for this method call.
        /// </summary>
        public string TsParameters
        {
            get
            {
                string result = "";
                result = string.Join(", ", ClientParameters.Select(f => f.Type.TsDeclarationPlain(f.Name)));
                if (!string.IsNullOrWhiteSpace(result)) result = result + ", ";
                result = result + $"callback: (result: {ReturnType.TsType}) => void = null, reload: boolean = true";
                return result;
            }
        }

        /// <summary>
        /// Gets the CS parameters for this method call.
        /// </summary>
        public string CsParameters
        {
            get
            {
                var parameters = Parameters.Where(f => !f.IsManualDI).ToArray();
                var outParameters = new List<string>();

                if (!IsStatic)
                    outParameters.Add("[FromServices] IDataSourceFactory dataSourceFactory");

                // When not static add an id that specifies the object to work on.
                if (!IsStatic)
                {
                    outParameters.Add($"{Parent.PrimaryKey.PureType.FullyQualifiedName} id");
                }
                outParameters.AddRange(parameters.Select(f => f.CsDeclaration));
                return string.Join(", ", outParameters);
            }
        }

        /// <summary>
        /// Gets the CS arguments passed to this method call.
        /// </summary>
        public string CsArguments => string.Join(", ", Parameters.Select(f => f.CsArgumentName));


        /// <summary>
        /// Gets the js arguments passed to this method call.
        /// </summary>
        public string JsArguments(string obj = "", bool callback = false)
        {
            string result;
            if (obj != "")
            {
                result = string.Join(", ", ClientParameters.Select(f => $"{obj}.{f.Name.ToCamelCase()}()"));
            }
            else
            {
                result = string.Join(", ", ClientParameters.Select(f => obj + f.Name.ToCamelCase()));
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
                if (!IsStatic)
                {
                    result = result + "id: this.parent[this.parent.primaryKeyName]()";
                    if (Parameters.Any()) result = result + ", ";
                }

                result += string.Join(", ", ClientParameters.Select(f => $"{f.Name}: {f.TsConversion(f.Name)}"));
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

        public SecurityInfoMethod SecurityInfo =>
            new SecurityInfoMethod(HasAttribute<ExecuteAttribute>(), this.GetAttributeValue<ExecuteAttribute>(a => a.Roles) ?? "");

        /// <summary>
        /// If true, this is a method that may be called by a client.
        /// </summary>
        public bool IsClientMethod => !IsInternalUse && HasAttribute<CoalesceAttribute>();

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
