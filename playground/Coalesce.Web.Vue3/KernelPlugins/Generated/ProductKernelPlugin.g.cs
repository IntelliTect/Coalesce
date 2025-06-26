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

namespace Coalesce.Web.Vue3.KernelPlugins;
#pragma warning disable CS1998

public class ProductKernelPlugin(CrudContext<Coalesce.Domain.AppDbContext> context, IDataSourceFactory dsFactory, IBehaviorsFactory bhFactory) : KernelPluginBase<Coalesce.Domain.Product>(context)
{
    protected Coalesce.Domain.AppDbContext Db => context.DbContext;

    [KernelFunction("get_product")]
    [Description("Gets a Product by its ProductId value. .")]
    public async Task<string> GetProduct(int productId)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetProduct, productId);
        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var dataSource = dsFactory.GetDataSource<Coalesce.Domain.Product, Coalesce.Domain.Product>("DefaultSource");
            var dataSourceParams = new DataSourceParameters { DataSource = "DefaultSource" };
            return await dataSource.GetMappedItemAsync<ProductResponse>(productId, dataSourceParams);
        });
    }

    [KernelFunction("list_product")]
    [Description("Lists Product records. . The search parameter can search on properties Name. The fields parameter should be used if you only need some of the following fields: ProductId,Name,Details,UniqueId,Unknown")]
    public async Task<string> ListProduct(
        string search,
        int page,
        bool countOnly,
        string[] fields

    )
    {
        if (!_isScoped) return await InvokeScoped<string>(ListProduct, search, page, countOnly, fields);
        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<ProductResponse>(errorMessage: "Unauthorized.");

            var dataSource = (Coalesce.Domain.Product.DefaultSource)dsFactory.GetDataSource<Coalesce.Domain.Product, Coalesce.Domain.Product>("DefaultSource");
            MappingContext mappingContext = new(context);

            var listParams = new ListParameters { DataSource = "DefaultSource", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 };
            if (countOnly)
            {
                var result = await dataSource.GetCountAsync(listParams);
                return new ListResult<ProductResponse>(result) { TotalCount = result.Object };
            }
            return await dataSource.GetMappedListAsync<ProductResponse>(listParams);
        });
    }

    [KernelFunction("save_product")]
    [Description("Creates a new Product. Updates an existing Product. Only provide value of the fields that need to be changed.")]
    public async Task<string> SaveProduct(ProductParameter dto)
    {
        if (!_isScoped) return await InvokeScoped<string>(SaveProduct, dto);
        return await Json(async () =>
        {
            var dataSource = dsFactory.GetDefaultDataSource<Coalesce.Domain.Product, Coalesce.Domain.Product>();
            var behaviors = bhFactory.GetBehaviors<Coalesce.Domain.Product>(GeneratedForClassViewModel);
            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;
            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
                return "Creation of Product items not allowed.";
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
                return "Editing of Product items not allowed.";
            return await behaviors.SaveAsync<ProductParameter, ProductResponse>(dto, dataSource, new DataSourceParameters());
        });
    }

}
