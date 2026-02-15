using System;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Api.DataSources;

public interface IDataSourceFactory
{
    object GetDataSource(ClassViewModel servedType, ClassViewModel declaredFor, string? dataSourceName = null);

    Type GetDataSourceType(ClassViewModel servedType, ClassViewModel declaredFor, string? dataSourceName = null);

    IDataSource<TServed> GetDataSource<TServed>(ClassViewModel declaredFor, string? dataSourceName)
        where TServed : class;

    IDataSource<TServed> GetDataSource<TServed, TDeclaredFor>(string? dataSourceName)
         where TServed : class
         where TDeclaredFor : class;

    object GetDefaultDataSource(ClassViewModel servedType, ClassViewModel declaredFor);

    IDataSource<TServed> GetDefaultDataSource<TServed>(ClassViewModel declaredFor)
        where TServed : class;

    IDataSource<TServed> GetDefaultDataSource<TServed, TDeclaredFor>()
        where TServed : class
        where TDeclaredFor : class;
}
