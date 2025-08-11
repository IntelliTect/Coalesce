using Coalesce.Web.Vue3.Models;
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

namespace Coalesce.Web.Vue3.KernelPlugins;
#pragma warning disable CS1998

public class ProductKernelPlugin(CrudContext<Coalesce.Domain.AppDbContext> context, IDataSourceFactory dataSourceFactory) : KernelPluginBase<Coalesce.Domain.Product>(context)
{
    protected Coalesce.Domain.AppDbContext Db => context.DbContext;

    [KernelFunction("get_product")]
    [Description("Gets a Product by its ProductId value. A product is a piece of software that is supported by a company.")]
    public async Task<string> GetProduct(
        int productId)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetProduct, productId);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var _dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Product, Coalesce.Domain.Product>();
            var _dataSourceParams = new DataSourceParameters { DataSource = "Default" };
            return await _dataSource.GetMappedItemAsync<ProductResponse>(productId, _dataSourceParams);
        });
    }

    [KernelFunction("list_product")]
    [Description("Lists Product records. A product is a piece of software that is supported by a company.")]
    public async Task<string> ListProduct(
        [Description("Search within properties Name")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: ProductId,Name,Details,UniqueId,MilestoneId,Milestone,Unknown")]
        string[] fields)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListProduct, search, page, countOnly, fields);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<ProductResponse>(errorMessage: "Unauthorized.");

            var _dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Product, Coalesce.Domain.Product>();
            MappingContext _mappingContext = new(context);

            var _listParams = new ListParameters { DataSource = "Default", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 25 };
            if (countOnly)
            {
                var result = await _dataSource.GetCountAsync(_listParams);
                return new ListResult<ProductResponse>(result) { TotalCount = result.Object };
            }
            return await _dataSource.GetMappedListAsync<ProductResponse>(_listParams);
        });
    }
}
