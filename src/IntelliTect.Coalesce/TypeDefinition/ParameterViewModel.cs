using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class ParameterViewModel : IAttributeProvider, IValueViewModel
    {
        public ParameterViewModel(MethodViewModel parent)
        {
            Parent = parent;
        }

        public MethodViewModel Parent { get; }

        public abstract string Name { get; }

        public TypeViewModel Type { get; protected set; }

        /// <summary>
        /// Gets the type name without any collection around it.
        /// </summary>
        public TypeViewModel PureType => Type.PureType;

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName =>
            this.GetAttributeValue<DisplayNameAttribute>(a => a.DisplayName) ??
            this.GetAttributeValue<DisplayAttribute>(a => a.Name) ??
            Name.ToProperCase();

        /// <summary>
        /// True if this is a parameter to the method on the model that is not represented in the controller action signature.
        /// It should instead be passed to the method using a value already available 
        /// either property on the controller or a local variable in the body of the generated action.
        /// </summary>
        public bool IsNonArgumentDI => !ShouldInjectFromServices && (IsAutoInjectedContext || IsAUser || IsAnIncludeTree);

        /// <summary>
        /// True if this is an injected method parameter that should be represented by a controller action argument.
        /// </summary>
        public bool ShouldInjectFromServices => HasInjectAttribute || (IsAutoInjectedContext && Parent.Parent.IsService);

        /// <summary>
        /// True if the parameter is marked with <see cref="InjectAttribute"/>
        /// </summary>
        public bool HasInjectAttribute => HasAttribute<InjectAttribute>();

        /// <summary>
        /// True if the parameter is NOT provided by the calling client.
        /// </summary>
        public bool IsDI => IsNonArgumentDI || ShouldInjectFromServices;

        /// <summary>
        /// True if the method is a <see cref="DbContext"/> that should be automatically injected, not needing an <see cref="InjectAttribute"/>.
        /// </summary>
        public bool IsAutoInjectedContext => Type.IsA<DbContext>() && !HasInjectAttribute;

        public bool IsAUser => Type.IsA<ClaimsPrincipal>();

        public bool IsAnIncludeTree => Type.IsA<IncludeTree>();

        /// <summary>
        /// Returns the parameter to pass to the actual method accounting for DI.
        /// </summary>
        public string CsArgument
        {
            get
            {
                if (IsNonArgumentDI)
                {
                    // We expect these to either be present on the controller which we're generating for,
                    // or in the contents of the generated action method.
                    if (IsAutoInjectedContext) return "Db";
                    if (IsAUser) return "User";
                    if (IsAnIncludeTree) return "out includeTree";
                }
                if (!IsDI && Type.HasClassViewModel)
                {
                    return $"{CsParameterName}.{nameof(Mapper.MapToModel)}(new {Type.FullyQualifiedName}(), new {nameof(MappingContext)}(User))";
                }
                return CsParameterName;
            }
        }

        public string PascalCaseName => Name.ToPascalCase();

        public bool ConvertsFromJsString => Type.IsNumber || Type.IsString || Type.IsDate || Type.IsBool || Type.IsEnum;

        public string CsDeclaration
        {
            get
            {
                var typeName = !IsDI && Type.HasClassViewModel 
                    ? Type.ClassViewModel.DtoName 
                    : Type.FullyQualifiedName;
                return $"{(ShouldInjectFromServices ? "[FromServices] " : "")}{typeName} {CsParameterName}{(HasDefaultValue ? " = " + CsDefaultValue : "")}";
            }
        }

        public string JsVariable => Name.ToCamelCase();
        public string CsParameterName => Name.ToCamelCase();

        public abstract bool HasDefaultValue { get; }
        protected abstract object RawDefaultValue { get; }

        /// <summary>
        /// C# compile-time constant expression representing the default value of the parameter if HasDefaultValue == true.
        /// </summary>
        public string CsDefaultValue
        {
            get
            {
                if (!HasDefaultValue) throw new InvalidOperationException("Parameter has no default value");
                return CSharpUtilities.GetCSharpLiteral(Type, RawDefaultValue);
            }
        }

        public override string ToString() => Name;

        public abstract object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;
    }
}
