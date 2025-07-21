using System;

namespace IntelliTect.Coalesce;

/// <summary>
/// Used to hint that a IDataSource or IBehaviors instance is declared for the given type. 
/// This is used both when defining such a class for an IClassDto as well as when such an instance needs to be model-bound.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class)]
public class DeclaredForAttribute : Attribute
{
    public DeclaredForAttribute(Type declaredFor)
    {
        DeclaredFor = declaredFor ?? throw new ArgumentNullException(nameof(declaredFor));
    }

    public Type DeclaredFor { get;  }
}