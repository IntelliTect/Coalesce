using Coalesce.Starter.Vue.Web.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Coalesce.Starter.Vue.Web.KernelPlugins;
#pragma warning disable CS1998

public class WidgetKernelPlugin(CrudContext<Coalesce.Starter.Vue.Data.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<Coalesce.Starter.Vue.Data.Models.Widget>(context)
{
    protected Coalesce.Starter.Vue.Data.AppDbContext Db => context.DbContext;

    [KernelFunction("save_widget")]
    [Description("Creates a new Widget or Updates an existing Widget.")]
    public async Task<string> SaveWidget(
        [Description("The values to update. Only provide value of the fields that need to be changed.")]
        WidgetParameter dto)
    {
        if (!_isScoped) return await InvokeScoped<string>(SaveWidget, dto);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Starter.Vue.Data.Models.Widget, Coalesce.Starter.Vue.Data.Models.Widget>();
            var behaviors = behaviorsFactory.GetBehaviors<Coalesce.Starter.Vue.Data.Models.Widget>(GeneratedForClassViewModel);

            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;
            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
                return "Creation of Widget items not allowed.";
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
                return "Editing of Widget items not allowed.";
            return await behaviors.SaveAsync<WidgetParameter, WidgetResponse>(dto, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("delete_widget")]
    [Description("Deletes an existing Widget.")]
    public async Task<string> DeleteWidget(
        int widgetId)
    {
        if (!_isScoped) return await InvokeScoped<string>(DeleteWidget, widgetId);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Starter.Vue.Data.Models.Widget, Coalesce.Starter.Vue.Data.Models.Widget>();
            var behaviors = behaviorsFactory.GetBehaviors<Coalesce.Starter.Vue.Data.Models.Widget>(GeneratedForClassViewModel);

            if (!GeneratedForClassViewModel.SecurityInfo.IsDeleteAllowed(User))
                return "Deleting of Widget items not allowed.";
            return await behaviors.DeleteAsync<WidgetResponse>(widgetId, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("get_widget")]
    [Description("Gets a Widget by its WidgetId value. A Widget represents a whimsical or fantastical invention.")]
    public async Task<string> GetWidget(
        int widgetId)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetWidget, widgetId);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var _dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Starter.Vue.Data.Models.Widget, Coalesce.Starter.Vue.Data.Models.Widget>();
            var _dataSourceParams = new DataSourceParameters { DataSource = "Default" };
            return await _dataSource.GetMappedItemAsync<WidgetResponse>(widgetId, _dataSourceParams);
        });
    }

    [KernelFunction("list_widget")]
    [Description("Lists Widget records. A Widget represents a whimsical or fantastical invention.")]
    public async Task<string> ListWidget(
        [Description("Search within properties Name")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: WidgetId,Name,Category,InventedOn,ModifiedBy,ModifiedById,ModifiedOn,CreatedBy,CreatedById,CreatedOn")]
        string[] fields)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListWidget, search, page, countOnly, fields);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<WidgetResponse>(errorMessage: "Unauthorized.");

            var _dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Starter.Vue.Data.Models.Widget, Coalesce.Starter.Vue.Data.Models.Widget>();
            MappingContext _mappingContext = new(context);

            var _listParams = new ListParameters { DataSource = "Default", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 25 };
            if (countOnly)
            {
                var result = await _dataSource.GetCountAsync(_listParams);
                return new ListResult<WidgetResponse>(result) { TotalCount = result.Object };
            }
            return await _dataSource.GetMappedListAsync<WidgetResponse>(_listParams);
        });
    }
}
