using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.Api.DataSources
{
    public class DataSourceNotFoundException : Exception
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

        public override string Message => $"A DataSource named {dataSourceName} declared for {declaredFor.Name} that serves type {servedType.Name} could not be found";
    }
}
