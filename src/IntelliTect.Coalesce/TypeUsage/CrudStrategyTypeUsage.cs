using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.TypeUsage
{
    public class CrudStrategyTypeUsage
    {
        public CrudStrategyTypeUsage(ClassViewModel strategyClass, ClassViewModel servedType, ClassViewModel sourceFor)
        {
            StrategyClass = strategyClass ?? throw new ArgumentNullException(nameof(strategyClass));
            ServedType = servedType ?? throw new ArgumentNullException(nameof(servedType));
            SourceFor = sourceFor ?? throw new ArgumentNullException(nameof(sourceFor));
        }

        /// <summary>
        /// The class that represents the data source. Inherits from IDataSource.
        /// </summary>
        public ClassViewModel StrategyClass { get; }

        /// <summary>
        /// The type for which this is a data source or behavior.
        /// Generated API controllers for this type should offer this data source as an option.
        /// In the case of custom DTOs, this will be an IClassDto, and ServedType will be the type parameter to IClassDto.
        /// </summary>
        public ClassViewModel SourceFor { get; }

        /// <summary>
        /// The type that is served by the data source or behavior.
        /// It is the type that should be used as the type parameter to IDataSource or IBehavior.
        /// </summary>
        public ClassViewModel ServedType { get; }

        public override int GetHashCode() => 
            unchecked(StrategyClass.GetHashCode() + ServedType.GetHashCode() + SourceFor.GetHashCode());

        public override bool Equals(object obj) =>
            obj is CrudStrategyTypeUsage that 
            && StrategyClass.Equals(that.StrategyClass) 
            && ServedType.Equals(that.ServedType)
            && SourceFor.Equals(that.SourceFor);
    }
}
