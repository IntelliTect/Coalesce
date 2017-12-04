using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.TypeUsage
{
    public class DataSourceTypeUsage
    {
        public DataSourceTypeUsage(TypeViewModel typeViewModel, ClassViewModel sourceFor)
        {
            TypeViewModel = typeViewModel ?? throw new ArgumentNullException(nameof(typeViewModel));
            SourceFor = sourceFor ?? throw new ArgumentNullException(nameof(sourceFor));

            var servedType = TypeViewModel.GenericArgumentsFor(typeof(IDataSource<>)).Single();

            if (!servedType.HasClassViewModel)
            {
                throw new InvalidOperationException($"{servedType} is not a valid type argument for a data source.");
            }

            ServedType = servedType.ClassViewModel;
        }

        /// <summary>
        /// The class that represents the data source. Inherits from IDataSource.
        /// </summary>
        public TypeViewModel TypeViewModel { get; }

        /// <summary>
        /// The type for which this is a data source.
        /// Generated API controllers for this type should offer this data source as an option.
        /// This may not be the type that is served in the case of IClassDto.
        /// </summary>
        public ClassViewModel SourceFor { get; }

        /// <summary>
        /// The type that is served by the data source.
        /// This is the unmapped, raw type, not yet mapped to a DTO (generated or custom IClassDto).
        /// This may not be equal to SourceFor in the case of IClassDto.
        /// </summary>
        public ClassViewModel ServedType { get; }
    }
}
