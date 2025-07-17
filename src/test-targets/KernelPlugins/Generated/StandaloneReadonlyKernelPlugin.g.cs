using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.SemanticKernel;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyProject.KernelPlugins;
#pragma warning disable CS1998

public class StandaloneReadonlyKernelPlugin(CrudContext context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory, IntelliTect.Coalesce.Tests.TargetClasses.WeatherService WeatherService, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext AppDbContext) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly>(context)
{

    [KernelFunction("get_standalone_readonly")]
    [Description("Gets a Standalone Readonly by its Id value. StandaloneReadonly DefaultSource.")]
    public async Task<string> GetStandaloneReadonly(
        int id)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetStandaloneReadonly, id);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly, IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly>("DefaultSource");
            var dataSourceParams = new DataSourceParameters { DataSource = "DefaultSource" };
            return await dataSource.GetMappedItemAsync<StandaloneReadonlyResponse>(id, dataSourceParams);
        });
    }

    [KernelFunction("list_standalone_readonly")]
    [Description("Lists Standalone Readonly records. StandaloneReadonly DefaultSource.")]
    public async Task<string> ListStandaloneReadonly(
        [Description("Search within properties Name")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: Id,Name,Description")]
        string[] fields)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListStandaloneReadonly, search, page, countOnly, fields);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<StandaloneReadonlyResponse>(errorMessage: "Unauthorized.");

            var dataSource = (IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly.DefaultSource)dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly, IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly>("DefaultSource");
            MappingContext mappingContext = new(context);

            var listParams = new ListParameters { DataSource = "DefaultSource", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 };
            if (countOnly)
            {
                var result = await dataSource.GetCountAsync(listParams);
                return new ListResult<StandaloneReadonlyResponse>(result) { TotalCount = result.Object };
            }
            return await dataSource.GetMappedListAsync<StandaloneReadonlyResponse>(listParams);
        });
    }

    [KernelFunction("instance_method")]
    [Description("StandaloneReadonly InstanceMethod")]
    public async Task<string> InstanceMethod(
        int id)
    {
        if (!_isScoped) return await InvokeScoped<string>(InstanceMethod, id);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("InstanceMethod");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<int>(errorMessage: "Unauthorized");
            var _params = new
            {
                Id = id
            };
            var weather = WeatherService;
            var explicitDbInjection = AppDbContext;
            var implicitDbInjection = AppDbContext;

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly, IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<int>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = await item.InstanceMethod(
                weather,
                explicitDbInjection,
                implicitDbInjection
            );
            var _result = new ItemResult<int>(_methodResult);
            _result.Object = _methodResult.Object;
            return _result;
        });
    }

    [KernelFunction("static_method")]
    [Description("StandaloneReadonly StaticMethod")]
    public async Task<string> StaticMethod()
    {
        if (!_isScoped) return await InvokeScoped<string>(StaticMethod);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("StaticMethod");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<int>(errorMessage: "Unauthorized");
            var weather = WeatherService;
            var explicitDbInjection = AppDbContext;
            var implicitDbInjection = AppDbContext;

            var _methodResult = await IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadonly.StaticMethod(
                weather,
                explicitDbInjection,
                implicitDbInjection
            );
            var _result = new ItemResult<int>(_methodResult);
            _result.Object = _methodResult.Object;
            return _result;
        });
    }
}
