using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.Api.DataSources
{
    public class DataSourceFactory : IDataSourceFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ReflectionRepository reflectionRepository;

        public DataSourceFactory(IServiceProvider serviceProvider, ReflectionRepository reflectionRepository)
        {
            this.serviceProvider = serviceProvider;
            this.reflectionRepository = reflectionRepository;
        }
        
        /// <summary>
        /// Defines all marker interfaces for defaults, along with their default concrete implementation.
        /// </summary>
        internal static readonly Dictionary<Type, Type> DefaultTypes = new Dictionary<Type, Type>
        {
            { typeof(IEntityFrameworkDataSource<,>), typeof(StandardDataSource<,>) }
            // Future: may be other kinds of defaults (non-EF)
        };

        protected Type GetDataSourceType(ClassViewModel servedType, ClassViewModel declaredFor, string dataSourceName)
        {
            var dataSources = declaredFor.ClientDataSources(reflectionRepository);

            var dataSourceClassViewModel = dataSources.FirstOrDefault(t => t.Name == dataSourceName);
            if (dataSourceClassViewModel == null)
            {
                if (dataSourceName == "" || dataSourceName == "Default" || dataSourceName == null)
                {
                    return GetDefaultDataSourceType(servedType, declaredFor);
                }
                else
                {
                    throw new DataSourceNotFoundException(servedType, declaredFor, dataSourceName);
                }
            }
            else
            {
                return dataSourceClassViewModel.Type.TypeInfo;
            }
        }

        public IDataSource<TServed> GetDataSource<TServed, TDeclaredFor>(string dataSourceName)
            where TServed : class, new()
            where TDeclaredFor : class, new()
        {
            return GetDataSource(
                reflectionRepository.GetClassViewModel<TServed>(),
                reflectionRepository.GetClassViewModel<TDeclaredFor>(),
                dataSourceName) as IDataSource<TServed>;
        }

        public object GetDataSource(ClassViewModel servedType, ClassViewModel declaredFor, string dataSourceName)
        {
            var dataSourceType = GetDataSourceType(servedType, declaredFor, dataSourceName);
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, dataSourceType);
        }

        protected Type GetDefaultDataSourceType(ClassViewModel servedType, ClassViewModel declaredFor)
        {
            var dataSources = declaredFor.ClientDataSources(reflectionRepository);
            var defaultSource = dataSources.SingleOrDefault(s => s.IsDefaultDataSource);

            if (defaultSource != null)
            {
                return defaultSource.Type.TypeInfo;
            }

            // FUTURE: If other kinds of default data sources are created, add them to the DefaultTypes dictionary above.
            var tContext = reflectionRepository.DbContexts.FirstOrDefault(c => c.Entities.Any(e => e.ClassViewModel.Equals(servedType)));
            var dataSourceType = typeof(IEntityFrameworkDataSource<,>).MakeGenericType(
                servedType.Type.TypeInfo,
                tContext.ClassViewModel.Type.TypeInfo
            );
            return dataSourceType;
        }

        public IDataSource<TServed> GetDefaultDataSource<TServed, TDeclaredFor>()
            where TServed : class, new()
            where TDeclaredFor : class, new()
        {
            return GetDefaultDataSource(
                reflectionRepository.GetClassViewModel<TServed>(), 
                reflectionRepository.GetClassViewModel<TDeclaredFor>()
            ) as IDataSource<TServed>;
        }

        public object GetDefaultDataSource(ClassViewModel servedType, ClassViewModel declaredFor)
        {
            var dataSourceType = GetDefaultDataSourceType(servedType, declaredFor);
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, dataSourceType);
        }
    }

}
