using Microsoft.EntityFrameworkCore;
using System;

namespace IntelliTect.Coalesce;


/// <summary>
/// <para>
/// The targeted class or interface will be exposed as a standalone entity by Coalesce.
/// </para>
/// <para>
/// The class will behave as if it is a standard <see cref="DbContext"/>-mapped entity type, 
/// except it must provide its own definition of a <see cref="IDataSource{T}"/> 
/// and <see cref="IBehaviors{T}"/> (behaviors are optional for read-only types).
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class StandaloneEntityAttribute : Attribute
{
}
