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
    [Description("Gets a Product by its ProductId value. .")]
    public async Task<string> GetProduct(
        int productId)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetProduct, productId);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Product, Coalesce.Domain.Product>("DefaultSource");
            var dataSourceParams = new DataSourceParameters { DataSource = "DefaultSource" };
            return await dataSource.GetMappedItemAsync<ProductResponse>(productId, dataSourceParams);
        });
    }

    [KernelFunction("list_product")]
    [Description("Lists Product records. .")]
    public async Task<string> ListProduct(
        [Description("Search within properties Name")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: ProductId,Name,Details,UniqueId,Unknown")]
        string[] fields)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListProduct, search, page, countOnly, fields);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<ProductResponse>(errorMessage: "Unauthorized.");

            var _dataSource = (Coalesce.Domain.Product.DefaultSource)dataSourceFactory.GetDataSource<Coalesce.Domain.Product, Coalesce.Domain.Product>("DefaultSource");
            MappingContext _mappingContext = new(context);

            var listParams = new ListParameters { DataSource = "DefaultSource", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 };
            if (countOnly)
            {
                var result = await _dataSource.GetCountAsync(listParams);
                return new ListResult<ProductResponse>(result) { TotalCount = result.Object };
            }
            return await _dataSource.GetMappedListAsync<ProductResponse>(listParams);
        });
    }
}
