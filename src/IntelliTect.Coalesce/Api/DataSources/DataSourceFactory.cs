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
        var dataSource = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, dataSourceType);

        // If the resolved data source serves a base type rather than the requested type,
        // wrap it in an adapter so that single-item retrieval works for the derived type.
        var servedTypeInfo = servedType.Type.TypeInfo;
        var actualServedType = GetActualServedType(dataSourceType);

        if (actualServedType != null && actualServedType != servedTypeInfo && servedTypeInfo.IsSubclassOf(actualServedType))
        {
            var baseClassViewModel = reflectionRepository.GetClassViewModel(actualServedType)!;
            dataSource = CreateInheritedAdapter(dataSource, actualServedType, servedTypeInfo, baseClassViewModel.SecurityInfo);
        }

        return dataSource;
    }

    /// <summary>
    /// Gets the T from IDataSource&lt;T&gt; that the given data source type implements.
    /// </summary>
    private static Type? GetActualServedType(Type dataSourceType)
    {
        return dataSourceType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDataSource<>))
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault();
    }

    /// <summary>
    /// Creates an <see cref="InheritedDataSourceAdapter{TDerived, TBase}"/> wrapping the given data source.
    /// </summary>
    private object CreateInheritedAdapter(object innerDataSource, Type baseType, Type derivedType, ClassSecurityInfo baseTypeSecurityInfo)
    {
        var adapterType = typeof(InheritedDataSourceAdapter<,>).MakeGenericType(derivedType, baseType);
        var crudContext = serviceProvider.GetRequiredService<CrudContext>();
        Func<System.Security.Claims.ClaimsPrincipal> userAccessor = () => crudContext.User;
        return Activator.CreateInstance(adapterType, innerDataSource, baseTypeSecurityInfo, userAccessor)!;
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
        var dataSource = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, dataSourceType);

        var servedTypeInfo = servedType.Type.TypeInfo;
        var actualServedType = GetActualServedType(dataSourceType);

        if (actualServedType != null && actualServedType != servedTypeInfo && servedTypeInfo.IsSubclassOf(actualServedType))
        {
            var baseClassViewModel = reflectionRepository.GetClassViewModel(actualServedType)!;
            dataSource = CreateInheritedAdapter(dataSource, actualServedType, servedTypeInfo, baseClassViewModel.SecurityInfo);
        }

        return dataSource;
    }
}
