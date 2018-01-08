using System;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Api.DataSources
{
    public interface IDataSourceFactory
    {
        object GetDataSource(ClassViewModel servedType, ClassViewModel declaredFor, string dataSourceName);

        IDataSource<TServed> GetDataSource<TServed, TDeclaredFor>(string dataSourceName)
             where TServed : class, new()
             where TDeclaredFor : class, new();

        object GetDefaultDataSource(ClassViewModel servedType, ClassViewModel declaredFor);

        IDataSource<TServed> GetDefaultDataSource<TServed, TDeclaredFor>()
            where TServed : class, new()
            where TDeclaredFor : class, new();
    }
}