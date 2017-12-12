using System;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Api.DataSources
{
    public interface IDataSourceFactory
    {
        object GetDataSource(ClassViewModel servedType, string dataSourceName);
        IDataSource<T> GetDataSource<T>(string dataSourceName) where T : class, new();

        object GetDefaultDataSource(ClassViewModel servedType);
        IDataSource<T> GetDefaultDataSource<T>() where T : class, new();
    }
}