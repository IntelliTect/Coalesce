using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.StringComparison;

namespace IntelliTect.Coalesce.Api.DataSources;

public class DataSourceFactory : IDataSourceFactory
{
    public const string DefaultSourceName = "Default";

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

    public Type GetDataSourceType(ClassViewModel servedType, ClassViewModel declaredFor, string? dataSourceName = null)
    {
        if (string.IsNullOrEmpty(dataSourceName) || dataSourceName.Equals(DefaultSourceName, InvariantCultureIgnoreCase))
        {
            return GetDefaultDataSourceType(servedType, declaredFor);
        }

        var dataSourceClassViewModel = GetDataSourcesForTypeHierarchy(declaredFor)
            .FirstOrDefault(t => t.ClientTypeName.Equals(dataSourceName, InvariantCultureIgnoreCase))
            ?? throw new DataSourceNotFoundException(servedType, declaredFor, dataSourceName);

        return dataSourceClassViewModel.Type.TypeInfo;
    }

    public IDataSource<TServed> GetDataSource<TServed>(ClassViewModel declaredFor, string? dataSourceName)
        where TServed : class
    {
        return (IDataSource<TServed>)GetDataSource(
            reflectionRepository.GetClassViewModel<TServed>()!,
            declaredFor,
            dataSourceName
        );
    }

    public IDataSource<TServed> GetDataSource<TServed, TDeclaredFor>(string? dataSourceName)
        where TServed : class
        where TDeclaredFor : class
    {
        return (IDataSource<TServed>)GetDataSource(
            reflectionRepository.GetClassViewModel<TServed>()!,
            reflectionRepository.GetClassViewModel<TDeclaredFor>()!,
            dataSourceName);
    }

    public object GetDataSource(ClassViewModel servedType, ClassViewModel declaredFor, string? dataSourceName = null)
    {
        var dataSourceType = GetDataSourceType(servedType, declaredFor, dataSourceName);
        return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, dataSourceType);
    }

    protected Type GetDefaultDataSourceType(ClassViewModel servedType, ClassViewModel declaredFor)
    {
        foreach (var (_, dataSources) in GetDataSourcesGroupedByHierarchy(declaredFor))
        {
            var defaultSource = dataSources.SingleOrDefault(s => s.IsDefaultDataSource);
            if (defaultSource != null) return defaultSource.Type.TypeInfo;

            if (declaredFor.IsStandaloneEntity && dataSources.Count == 1)
                return dataSources[0].Type.TypeInfo;
        }

        if (servedType.DbContext is null)
        {
            throw new ArgumentException("Requested type is not served by a DbContext and has no explicitly declared data source.");
        }

        // FUTURE: If other kinds of default data sources are created, add them to the DefaultTypes dictionary above.
        return typeof(IEntityFrameworkDataSource<,>).MakeGenericType(
            servedType.Type.TypeInfo,
            servedType.DbContext.Type.TypeInfo
        );
    }

    /// <summary>
    /// Returns all client data sources for <paramref name="declaredFor"/> and then for each of its
    /// base types in order from most-derived to least-derived.
    /// Data sources on more-derived types take precedence over those on base types.
    /// </summary>
    private IEnumerable<ClassViewModel> GetDataSourcesForTypeHierarchy(ClassViewModel declaredFor)
    {
        foreach (var (_, dataSources) in GetDataSourcesGroupedByHierarchy(declaredFor))
            foreach (var ds in dataSources)
                yield return ds;
    }

    /// <summary>
    /// Returns the data sources for <paramref name="declaredFor"/> and each of its
    /// base types in order from most-derived to least-derived, grouped by declaring type.
    /// </summary>
    private IEnumerable<(ClassViewModel Type, IReadOnlyList<ClassViewModel> DataSources)> GetDataSourcesGroupedByHierarchy(ClassViewModel declaredFor)
    {
        var ownSources = declaredFor.ClientDataSources(reflectionRepository).ToList();
        if (ownSources.Count > 0) yield return (declaredFor, ownSources);

        foreach (var baseType in declaredFor.ClientBaseTypes)
        {
            var baseSources = baseType.ClientDataSources(reflectionRepository).ToList();
            if (baseSources.Count > 0) yield return (baseType, baseSources);
        }
    }

    public IDataSource<TServed> GetDefaultDataSource<TServed, TDeclaredFor>()
        where TServed : class
        where TDeclaredFor : class
    {
        return (IDataSource<TServed>)GetDefaultDataSource(
            reflectionRepository.GetClassViewModel<TServed>()!, 
            reflectionRepository.GetClassViewModel<TDeclaredFor>()!
        );
    }

    public IDataSource<TServed> GetDefaultDataSource<TServed>(ClassViewModel declaredFor)
        where TServed : class
    {
        return (IDataSource<TServed>)GetDefaultDataSource(
            reflectionRepository.GetClassViewModel<TServed>()!,
            declaredFor
        );
    }

    public object GetDefaultDataSource(ClassViewModel servedType, ClassViewModel declaredFor)
    {
        var dataSourceType = GetDefaultDataSourceType(servedType, declaredFor);
        return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, dataSourceType);
    }
}
