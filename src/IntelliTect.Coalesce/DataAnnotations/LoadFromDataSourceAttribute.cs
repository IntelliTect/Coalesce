using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Specify that the targeted model instance method should load the instance it is called on
    /// from the specified data source when invoked from an API endpoint.
    /// By default, whatever the default data source for the model's type will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LoadFromDataSourceAttribute : Attribute
    {
        public LoadFromDataSourceAttribute(Type dataSourceType)
        {
            DataSourceType = dataSourceType;
        }

        public Type DataSourceType { get; }
    }
}
