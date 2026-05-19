using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.TypeUsage;

/// <summary>
/// Represents an open generic data source class whose single type parameter
/// is constrained to a specific entity type. The data source is usable for
/// that entity type and all of its derived types.
/// </summary>
/// <param name="StrategyClass">
/// The open generic <see cref="ClassViewModel"/> for the data source strategy class,
/// e.g., the ClassViewModel representing <c>MyDataSource&lt;T&gt;</c>.
/// </param>
/// <param name="ConstraintType">
/// The entity type that the data source's type parameter T is constrained to,
/// e.g., <c>MyBase</c> in <c>where T : MyBase</c>.
/// </param>
public record OpenGenericDataSourceUsage(
    ClassViewModel StrategyClass,
    ClassViewModel ConstraintType
);
