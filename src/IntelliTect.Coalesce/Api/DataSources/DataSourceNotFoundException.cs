using IntelliTect.Coalesce.TypeDefinition;
using System;

namespace IntelliTect.Coalesce.Api.DataSources;

#pragma warning disable RCS1194 // Implement exception constructors.
public class DataSourceNotFoundException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
{
    private readonly ClassViewModel servedType;
    private readonly ClassViewModel declaredFor;
    private readonly string dataSourceName;

    public DataSourceNotFoundException(ClassViewModel servedType, ClassViewModel declaredFor, string dataSourceName)
    {
        this.servedType = servedType;
        this.declaredFor = declaredFor;
        this.dataSourceName = dataSourceName;
    }

    public override string Message => $"A DataSource named {dataSourceName} declared for {declaredFor} that serves type {servedType} could not be found";
}
