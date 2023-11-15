using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class ParameterViewModel : ValueViewModel
    {
        private protected ParameterViewModel(MethodViewModel parent, TypeViewModel type) : base(type)
        {
            Parent = parent;
        }

        public MethodViewModel Parent { get; }

        /// <summary>
        /// If set, this parameter's value is automatically populated from the referenced 
        /// property on the parent model's ViewModel instance.
        /// </summary>
        public PropertyViewModel? ParentSourceProp { get; protected set; }

        /// <summary>
        /// True if this is a parameter to the method on the model that is not represented in the controller action signature.
        /// It should instead be passed to the method using a value already available 
        /// either property on the controller or a local variable in the body of the generated action.
        /// </summary>
        public bool IsNonArgumentDI => !ShouldInjectFromServices && (IsAutoInjectedContext || IsAUser || IsAnIncludeTree);

        /// <summary>
        /// True if this is an injected method parameter that should be represented by a controller action argument.
        /// </summary>
        public bool ShouldInjectFromServices => HasInjectAttribute || (!IsAutoInjectedContext && IsDbContext);

        /// <summary>
        /// True if the parameter is marked with <see cref="InjectAttribute"/>
        /// </summary>
        public bool HasInjectAttribute => this.HasAttribute<InjectAttribute>();

        /// <summary>
        /// True if the parameter is NOT provided by the calling client.
        /// </summary>
        public bool IsDI => IsNonArgumentDI || ShouldInjectFromServices || Type.IsA<CancellationToken>();

        /// <summary>
        /// True if the method is a <see cref="DbContext"/> that should be automatically injected, not needing an <see cref="InjectAttribute"/>.
        /// </summary>
        public bool IsAutoInjectedContext => IsDbContext && !HasInjectAttribute && Parent.Parent.DbContext != null;

        public bool IsDbContext => Type.IsA<DbContext>();

        public bool IsAUser => Type.IsA<ClaimsPrincipal>();

        public bool IsAnIncludeTree => Type.IsA<IncludeTree>();

        public string PascalCaseName => Name.ToPascalCase();

        public bool ConvertsFromJsString => Type.IsNumber || Type.IsString || Type.IsGuid || Type.IsDate || Type.IsBool || Type.IsEnum;

        public string CsParameterName => Name.ToCamelCase();

        public abstract bool HasDefaultValue { get; }
        protected abstract object? RawDefaultValue { get; }

        /// <summary>
        /// C# compile-time constant expression representing the default value of the parameter if HasDefaultValue == true.
        /// </summary>
        public virtual string CsDefaultValue
        {
            get
            {
                if (!HasDefaultValue) throw new InvalidOperationException("Parameter has no default value");
                return CSharpUtilities.GetCSharpLiteral(Type, RawDefaultValue);
            }
        }

        /// <summary>
        /// The reference type nullability state for this parameter.
        /// </summary>
        public NullabilityState Nullability { get; set; }

        public override bool IsRequired
        {
            get
            {
                if (base.IsRequired) return true;

                if (Type.IsReferenceType && Nullability == NullabilityState.NotNull) return true;

                return false;
            }
        }

        public override string ToString() => $"{Type} {Name}";
    }
}
