using System;

namespace IntelliTect.Coalesce;

/// <summary>
/// Indicate that an IDataSource&lt;T&gt; is the default implementation for its type T.
/// This DataSource will displace the standard implementation for its served type.
/// Any references to "Default", and any requests with no specified DataSource, will use this type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DefaultDataSourceAttribute : Attribute
{
}
