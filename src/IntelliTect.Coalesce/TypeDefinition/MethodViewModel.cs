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
    public enum MethodTransportType
    {
        ItemResult,
        ListResult,
    }

    public abstract class MethodViewModel : IAttributeProvider
    {
        internal MethodViewModel(ClassViewModel parent)
        {
            Parent = parent;
        }

        public ClassViewModel Parent { get; }

        public abstract bool IsStatic { get; }

        /// <summary>
        /// True if the method returns a <see cref="Task"/>
        /// </summary>
        public bool IsAwaitable => ReturnType.IsA<Task>();

        public abstract string Comment { get; }

        public abstract string Name { get; }

        public string NameWithoutAsync => Name.EndsWith("Async") && IsAwaitable
            ? Name.Remove(Name.Length - 5)
            : Name;

        /// <summary>
        /// Provides the raw, unaltered return type of the method.
        /// </summary>
        public abstract TypeViewModel ReturnType { get; }

        /// <summary>
        /// Provides the return type of the method, excluding any <see cref="Task{T}"/> that may wrap it.
        /// </summary>
        public TypeViewModel TaskUnwrappedReturnType
        {
            get
            {
                if (!IsAwaitable) return ReturnType;

                if (ReturnType.IsA(typeof(Task<>))) return ReturnType.FirstTypeArgument!;

                // Return type is a task, but not a generic task. Effective type is void.
                return ReflectionTypeViewModel.GetOrCreate(Parent.ReflectionRepository, typeof(void));
            }
        }

        public abstract IEnumerable<ParameterViewModel> Parameters { get; }

        /// <summary>
        /// True if the method's return type is a <see cref="ListResult{T}"/>
        /// </summary>
        public bool ReturnsListResult => TaskUnwrappedReturnType.IsA(typeof(ListResult<>));

        /// <summary>
        /// The return type of the essential data returned by the method.
        /// This discounts any <see cref="ItemResult"/> or <see cref="ListResult{T}"/> that may be wrapped 
        /// around its return type, as well as any <see cref="Task"/> or <see cref="Task{T}"/>
        /// For <see cref="ListResult{T}"/> returns, this will be <see cref="IList{T}"/>.
        /// For <see cref="ItemResult{T}"/> returns, this will be T.
        /// For any plain return type T, this will be T.
        /// </summary>
        public TypeViewModel ResultType
        {
            get
            {
                var retType = TaskUnwrappedReturnType;

                if (ReturnsListResult)
                {
                    // For ReturnsListResult, the result will be a constructed generic IList<T>
                    return retType.ClassViewModel!.PropertyByName(nameof(ListResult<object>.List))!.Type;
                }

                if (retType.IsA(typeof(ItemResult<>)))
                {
                    return retType.FirstTypeArgument!;
                }

                if (retType.IsA(typeof(ItemResult)))
                {
                    return ReflectionTypeViewModel.GetOrCreate(Parent.ReflectionRepository, typeof(void));
                }

                return retType;
            }
        }

        /// <summary>
        /// The transport object that is returned by the API controller.
        /// </summary>
        public MethodTransportType TransportType => ReturnsListResult
            ? MethodTransportType.ListResult
            : MethodTransportType.ItemResult;

        /// <summary>
        /// The generic parameter to the <see cref="TransportType"/>. Can possibly be void.
        /// </summary>
        public TypeViewModel TransportTypeGenericParameter => ReturnsListResult
            ? ResultType.PureType
            : ResultType;

        /// <summary>
        /// Returns true if this method is marked as InternalUse. Not exposed through the API
        /// </summary>
        public virtual bool IsInternalUse => HasAttribute<InternalUseAttribute>();
        
        
        public HttpMethod ApiActionHttpMethod =>
            this.GetAttributeValue<ControllerActionAttribute, HttpMethod>(a => a.Method) ?? HttpMethod.Post;

        public string ApiActionHttpMethodName => ApiActionHttpMethod.ToString().ToUpper();
        public string ApiActionHttpMethodAnnotation => $"Http{ApiActionHttpMethod.ToString()}";

        /// <summary>
        /// Return type of the controller action for the method.
        /// If the method is async, this will not include the Task that wraps the result.
        /// </summary>
        public string ApiActionReturnTypeDeclaration => TransportTypeGenericParameter.IsVoid
            ? TransportType.ToString()
            : $"{TransportType}<{TransportTypeGenericParameter.DtoFullyQualifiedName}>";


        /// <summary>
        /// Convenient accessor for the MethodInfo when in reflection-based contexts.
        /// </summary>
        public virtual MethodInfo MethodInfo => 
            throw new InvalidOperationException("MethodInfo not available in the current context");

        public string JsVariable => NameWithoutAsync.ToCamelCase();

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
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName =>
            this.GetAttributeValue<DisplayNameAttribute>(a => a.DisplayName) ??
            this.GetAttributeValue<DisplayAttribute>(a => a.Name) ??
            NameWithoutAsync.ToProperCase()!;

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
                return type.ClientTypeName;
            }
        }

        public abstract object? GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;

        public override string ToString()
            => $"{ReturnType} {Name}({string.Join(",", Parameters)})";
    }
}
