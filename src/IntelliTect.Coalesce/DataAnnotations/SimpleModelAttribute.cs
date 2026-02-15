using System;

namespace IntelliTect.Coalesce;

/// <summary>
/// The targeted class will be treated as a Simple Model by Coalesce,
/// generating a corresponding DTO and TypeScript model.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SimpleModelAttribute : Attribute
{
}
