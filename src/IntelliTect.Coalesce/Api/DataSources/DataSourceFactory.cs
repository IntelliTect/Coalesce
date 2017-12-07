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

        public IDataSource<T> GetDataSource<T>(string dataSourceName)
            where T : class, new()
        {
            return GetDataSource(reflectionRepository.GetClassViewModel<T>(), dataSourceName) as IDataSource<T>;
        }

        public object GetDataSource(Type servedType, string dataSourceName)
        {
            return GetDataSource(reflectionRepository.GetClassViewModel(servedType), dataSourceName);
        }

        public object GetDataSource(ClassViewModel servedType, string dataSourceName)
        {
            var dataSources = servedType.ClientDataSources(reflectionRepository);

            Type dataSourceType = null;
            var dataSourceTypeViewModel = dataSources.FirstOrDefault(t => t.Name == dataSourceName);
            if (dataSourceTypeViewModel == null)
            {
                if (dataSourceName == "" || dataSourceName == "Default" || dataSourceName == null)
                {
                    return GetDefaultDataSource(servedType);
                }
                else
                {
                    // TODO: don't thow an exception. Handle the error through model state errors.
                    throw new Exception($"unknown data source {dataSourceName}");
                }
            }
            else
            {
                dataSourceType = (dataSourceTypeViewModel as ReflectionTypeViewModel).Info;
            }

            return ActivatorUtilities.CreateInstance(serviceProvider, dataSourceType);
        }


        public IDataSource<T> GetDefaultDataSource<T>()
            where T : class, new()
        {
            return GetDefaultDataSource(reflectionRepository.GetClassViewModel<T>()) as IDataSource<T>;
        }

        public object GetDefaultDataSource(Type servedType)
        {
            return GetDefaultDataSource(reflectionRepository.GetClassViewModel(servedType));
        }

        public object GetDefaultDataSource(ClassViewModel servedType)
        {
            var tContext = reflectionRepository.DbContexts.FirstOrDefault(c => c.Entities.Any(e => e.ClassViewModel.Equals(servedType)));
            var dataSourceType = typeof(StandardDataSource<,>).MakeGenericType(
                (servedType.Type as ReflectionTypeViewModel).Info,
                (tContext.ClassViewModel.Type as ReflectionTypeViewModel).Info
            );

            return ActivatorUtilities.CreateInstance(serviceProvider, dataSourceType);
        }
    }

}
