using IntelliTect.Coalesce.TypeDefinition;
using System;

namespace IntelliTect.Coalesce.TypeUsage;

public class CrudStrategyTypeUsage
{
    public CrudStrategyTypeUsage(ClassViewModel strategyClass, ClassViewModel servedType, ClassViewModel declaredFor)
    {
        StrategyClass = strategyClass ?? throw new ArgumentNullException(nameof(strategyClass));
        ServedType = servedType ?? throw new ArgumentNullException(nameof(servedType));
        DeclaredFor = declaredFor ?? throw new ArgumentNullException(nameof(declaredFor));
    }

    /// <summary>
    /// The class of the behavior/datasource. Inherits from IDataSource or IBehaviors.
    /// </summary>
    public ClassViewModel StrategyClass { get; }

    /// <summary>
    /// The type for which this data source or behavior was declared.
    /// Generated API controllers for this type should offer this data source as an option.
    /// In the case of custom DTOs, this will be an IClassDto, and ServedType will be the type parameter to IClassDto.
    /// </summary>
    public ClassViewModel DeclaredFor { get; }

    /// <summary>
    /// The type that is served by the data source or behavior.
    /// It is the type that should be used as the type parameter to IDataSource or IBehavior.
    /// </summary>
    public ClassViewModel ServedType { get; }

    public override int GetHashCode() => 
        unchecked(StrategyClass.GetHashCode() + ServedType.GetHashCode() + DeclaredFor.GetHashCode());

    public override bool Equals(object? obj) =>
        obj is CrudStrategyTypeUsage that 
        && StrategyClass.Equals(that.StrategyClass) 
        && ServedType.Equals(that.ServedType)
        && DeclaredFor.Equals(that.DeclaredFor);
}
