using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce
{
    /// <summary>
    /// Indicate that an IDataSource&lt;T&gt; is the default implementation for its type T.
    /// This DataSource will displace the default implementation for its served type.
    /// Any references to "Default", and any requests with no specified DataSource will use this type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DefaultDataSourceAttribute : Attribute
    {
    }
}
