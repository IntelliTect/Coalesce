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

        protected Type GetDataSourceType(ClassViewModel servedType, string dataSourceName)
        {
            var dataSources = servedType.ClientDataSources(reflectionRepository);

            var dataSourceClassViewModel = dataSources.FirstOrDefault(t => t.Name == dataSourceName);
            if (dataSourceClassViewModel == null)
            {
                if (dataSourceName == "" || dataSourceName == "Default" || dataSourceName == null)
                {
                    return GetDefaultDataSourceType(servedType);
                }
                else
                {
                    throw new DataSourceNotFoundException(servedType, dataSourceName);
                }
            }
            else
            {
                return (dataSourceClassViewModel.Type as ReflectionTypeViewModel).Info;
            }
        }

        public IDataSource<T> GetDataSource<T>(string dataSourceName)
            where T : class, new()
        {
            return GetDataSource(reflectionRepository.GetClassViewModel<T>(), dataSourceName) as IDataSource<T>;
        }

        public object GetDataSource(ClassViewModel servedType, string dataSourceName)
        {
            var dataSourceType = GetDataSourceType(servedType, dataSourceName);
            return ActivatorUtilities.CreateInstance(serviceProvider, dataSourceType);
        }


        protected Type GetDefaultDataSourceType(ClassViewModel servedType)
        {
            var tContext = reflectionRepository.DbContexts.FirstOrDefault(c => c.Entities.Any(e => e.ClassViewModel.Equals(servedType)));
            var dataSourceType = typeof(StandardDataSource<,>).MakeGenericType(
                (servedType.Type as ReflectionTypeViewModel).Info,
                (tContext.ClassViewModel.Type as ReflectionTypeViewModel).Info
            );
            return dataSourceType;
        }

        public IDataSource<T> GetDefaultDataSource<T>()
            where T : class, new()
        {
            return GetDefaultDataSource(reflectionRepository.GetClassViewModel<T>()) as IDataSource<T>;
        }

        public object GetDefaultDataSource(ClassViewModel servedType)
        {
            var dataSourceType = GetDefaultDataSourceType(servedType);
            return ActivatorUtilities.CreateInstance(serviceProvider, dataSourceType);
        }
    }

}
