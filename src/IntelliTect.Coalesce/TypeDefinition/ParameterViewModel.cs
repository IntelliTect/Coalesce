using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading;

namespace IntelliTect.Coalesce.TypeDefinition;

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
    public bool ShouldInjectFromServices => 
        HasInjectAttribute ||
        // Only DbContexts that are not the `IsAutoInjectedContext` are injected from services.
        // The auto-injected context is sourced from the controller, not the action parameters.
        (!IsAutoInjectedContext && IsDbContext) ||
        // Interfaces that are not known data types (file, collections) are always injected
        // because Coalesce couldn't otherwise possibly know the implementation type to use.
        (Type.IsInterface && !Type.IsFile && !Type.IsCollection)
        ;

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

    public string CsParameterName => Name.ToCamelCase().GetValidCSharpIdentifier();

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
            // Any parameter with a non-null default value will always ultimately have
            // a value by the time the underlying method is reached, so a required param
            // with a default value doesn't make sense.
            if (HasDefaultValue && RawDefaultValue is not null) return false;

            if (base.IsRequired) return true;

            // The PK param for instance methods is always required.
            if (ParentSourceProp?.IsPrimaryKey == true) return true;

            if (Type.IsReferenceType && Nullability == NullabilityState.NotNull) return true;

            return false;
        }
    }

    public string? FileTypes => this.GetAttributeValue<FileTypeAttribute>(a => a.FileTypes);

    public override string ToString() => $"{Type} {Name}";
}
