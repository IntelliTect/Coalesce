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

    public abstract partial class MethodViewModel : IAttributeProvider
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

        public abstract string? Comment { get; }

        public abstract string Name { get; }

        public string NameWithoutAsync => Name.EndsWith("Async") && IsAwaitable
            ? Name.Remove(Name.Length - 5)
            : Name;

        public abstract IEnumerable<ParameterViewModel> Parameters { get; }

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

        public MethodReturnViewModel Return => new MethodReturnViewModel(this);

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
        public virtual bool IsInternalUse => this.HasAttribute<InternalUseAttribute>();

        public HttpMethod ApiActionHttpMethod =>
#pragma warning disable CS0618 // Type or member is obsolete
            this.GetAttributeValue<ControllerActionAttribute, HttpMethod>(a => a.Method) ??
#pragma warning restore CS0618 // Type or member is obsolete
            this.GetAttributeValue<ExecuteAttribute, HttpMethod>(a => a.HttpMethod) ??
            HttpMethod.Post;

        public bool HasHttpRequestBody =>
            ApiActionHttpMethod != HttpMethod.Get &&
            ApiActionHttpMethod != HttpMethod.Delete &&
            ApiParameters.Any();

        public PropertyViewModel? VaryByProperty =>
            !IsModelInstanceMethod ? null :
            ApiActionHttpMethod != HttpMethod.Get ? null :
#pragma warning disable CS0618 // Type or member is obsolete
            Parent.PropertyByName(this.GetAttributeValue<ControllerActionAttribute>(a => a.VaryByProperty)) ??
#pragma warning restore CS0618 // Type or member is obsolete
            Parent.PropertyByName(this.GetAttributeValue<ExecuteAttribute>(a => a.VaryByProperty));

        /// <summary>
        /// Return type of the controller action for the method.
        /// If the method is async, this will not include the Task that wraps the result.
        /// </summary>
        public string ApiActionReturnTypeDeclaration => TransportTypeGenericParameter.IsVoid
            ? TransportType.ToString()
            : $"{TransportType}<{TransportTypeGenericParameter.NullableTypeForDto(isInput: false, dtoNamespace: null, dontEmitNullable: true)}>";


        /// <summary>
        /// Convenient accessor for the MethodInfo when in reflection-based contexts.
        /// </summary>
        public virtual MethodInfo MethodInfo => 
            throw new InvalidOperationException("MethodInfo not available in the current context");

        public string JsVariable => NameWithoutAsync.ToCamelCase();

        /// <summary>
        /// List of parameters that are not Dependency Injected (DI)
        /// </summary>
        public IEnumerable<ParameterViewModel> ClientParameters => Parameters
            .Where(f => !f.IsDI);

        /// <summary>
        /// List of parameters that are part of the endpoint's API surface.
        /// Includes implicit parameters that are not defined on the underlying implementation.
        /// </summary>
        public IEnumerable<ParameterViewModel> ApiParameters
        {
            get
            {
                var parameters = ClientParameters;
                if (IsModelInstanceMethod && Parent.PrimaryKey != null)
                {
                    parameters = new[]
                    {
                        new ImplicitParameterViewModel(
                            this,
                            Parent.PrimaryKey,
                            "id", 
                            "Primary Key"
                        )
                    }.Concat(parameters);
                }

                if (VaryByProperty != null && VaryByProperty.IsClientProperty)
                {
                    parameters = parameters.Concat(new[]
                    {
                        new ImplicitParameterViewModel(this, VaryByProperty, "etag")
                    });
                }

                return parameters;
            }
        }

        public bool IsModelInstanceMethod => !IsStatic && !Parent.IsService;

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName =>
            this.GetAttributeValue<DisplayNameAttribute>(a => a.DisplayName) ??
            this.GetAttributeValue<DisplayAttribute>(a => a.Name) ??
            NameWithoutAsync.ToProperCase()!;

        /// <summary>
        /// For the specified area, returns true if the method has a hidden attribute.
        /// </summary>
        public bool IsHidden(HiddenAttribute.Areas area) => HiddenAreas.HasFlag(area);

        public HiddenAttribute.Areas HiddenAreas
        {
            get
            {
                if (IsInternalUse)
                {
                    throw new InvalidOperationException("Cannot evaluate the hidden state of an InternalUse method.");
                }

                if (this.GetAttributeValue<HiddenAttribute, HiddenAttribute.Areas>(a => a.Area) is HiddenAttribute.Areas value)
                {
                    // Take the attribute value first to allow for overrides of the default behavior below
                    return value;
                }

                return HiddenAttribute.Areas.None;
            }
        }

        public MethodSecurityInfo SecurityInfo => new MethodSecurityInfo(this.GetSecurityPermission<ExecuteAttribute>());

        /// <summary>
        /// If true, this is a method that may be called by a client.
        /// </summary>
        public bool IsClientMethod => 
            !IsInternalUse &&
            Parent.WillCreateApiController &&
            !SecurityInfo.Execute.NoAccess && 
            // Services only have instance methods - no static methods.
            (!Parent.IsService || !IsStatic) && 
            // Interface services always expose all their declared methods.
            ((Parent.IsService && Parent.Type.IsInterface) || this.HasAttribute<CoalesceAttribute>());

        public string LoadFromDataSourceName
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var type = 
                    this.GetAttributeValue<LoadFromDataSourceAttribute>(a => a.DataSourceType) ??
#pragma warning restore CS0618 // Type or member is obsolete
                    this.GetAttributeValue<ExecuteAttribute>(a => a.DataSource);

                if (type == null) return Api.DataSources.DataSourceFactory.DefaultSourceName;
                return type.ClientTypeName;
            }
        }

        /// <summary>
        /// If this method is a constructor, returns information about its suitability for usage in IClassDto.MapToNew.
        /// </summary>
        public ConstructorUsage DtoMapToNewConstructorUsage
        {
            get
            {
                var incomingProperties = Parent
                    .ClientProperties
                    .Where(p => p.SecurityInfo.Init.IsAllowed())
                    .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

                var ctorMappableProps = new List<PropertyViewModel>();
                foreach (var parameter in Parameters)
                {
                    if (!incomingProperties.TryGetValue(parameter.Name, out var prop))
                    {
                        // There's no incoming property that corresponds to this parameter name,
                        // so Coalesce cannot provide any value here and therefore has no way 
                        // of knowing how to call this ctor.
                        return new ConstructorUsage
                        {
                            IsAcceptable = false,
                            Reason = $"There is no incoming property named `{parameter.Name}`, so Coalesce cannot provide a value for that constructor parameter."
                        };
                    }
                    else
                    {
                        ctorMappableProps.Add(prop);
                    }
                }

#if NET7_0_OR_GREATER
                if (!this.HasAttribute<System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute>())
                {
                    foreach (var requiredProp in Parent.ClientProperties.Where(p => p.HasRequiredKeyword))
                    {
                        if (!incomingProperties.ContainsKey(requiredProp.Name))
                        {
                            return new ConstructorUsage
                            {
                                IsAcceptable = false,
                                Reason = $"Required property `{requiredProp.Name}` is not accepted as an input, " +
                                    $"so Coalesce cannnot satisfy its `required` constraint. Adding [SetsRequiredMembers] will suppress this."
                            };
                        }
                    }
                }
#endif

                var initParams = incomingProperties.Values.Where(p => p.IsInitOnly || p.HasRequiredKeyword).ToList();
                foreach (var prop in initParams) incomingProperties.Remove(prop.Name);

                return new ConstructorUsage
                {
                    IsAcceptable = true,
                    CtorParams = ctorMappableProps,
                    InitParams = initParams,
                    SetParams = incomingProperties.Values.ToList()
                };
            }
        }

        public abstract IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>() where TAttribute : Attribute;

        public override string ToString()
            => $"{ReturnType} {ToStringWithoutReturn()}";

        public string ToStringWithoutReturn()
            => $"{Name}({string.Join(", ", Parameters)})";


        public class ConstructorUsage
        {
            public bool IsAcceptable { get; set; }
            public string? Reason { get; internal set; }
            public List<PropertyViewModel>? CtorParams { get; internal set; }
            public List<PropertyViewModel>? InitParams { get; internal set; }
            public List<PropertyViewModel>? SetParams { get; internal set; }
        }
    }

}
