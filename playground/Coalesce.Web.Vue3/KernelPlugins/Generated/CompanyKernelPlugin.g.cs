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

public class CompanyKernelPlugin(CrudContext<Coalesce.Domain.AppDbContext> context, IDataSourceFactory dsFactory, IBehaviorsFactory bhFactory) : KernelPluginBase<Coalesce.Domain.Company>(context)
{
    protected Coalesce.Domain.AppDbContext Db => context.DbContext;

    [KernelFunction("get_company")]
    [Description("Gets a Company by its Id value. .")]
    public async Task<string> GetCompany(int id)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetCompany, id);
        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var dataSource = dsFactory.GetDataSource<Coalesce.Domain.Company, Coalesce.Domain.Company>("DefaultSource");
            var dataSourceParams = new DataSourceParameters { DataSource = "DefaultSource" };
            return await dataSource.GetMappedItemAsync<CompanyResponse>(id, dataSourceParams);
        });
    }

    [KernelFunction("list_company")]
    [Description("Lists Company records. . The search parameter can search on properties Name,LogoUrl. The fields parameter should be used if you only need some of the following fields: Id,Name,Address1,Address2,City,State,ZipCode,Phone,WebsiteUrl,LogoUrl,IsDeleted,Employees,AltName")]
    public async Task<string> ListCompany(
        string search,
        int page,
        bool countOnly,
        string[] fields

    )
    {
        if (!_isScoped) return await InvokeScoped<string>(ListCompany, search, page, countOnly, fields);
        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<CompanyResponse>(errorMessage: "Unauthorized.");

            var dataSource = (Coalesce.Domain.Company.DefaultSource)dsFactory.GetDataSource<Coalesce.Domain.Company, Coalesce.Domain.Company>("DefaultSource");
            MappingContext mappingContext = new(context);

            var listParams = new ListParameters { DataSource = "DefaultSource", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 };
            if (countOnly)
            {
                var result = await dataSource.GetCountAsync(listParams);
                return new ListResult<CompanyResponse>(result) { TotalCount = result.Object };
            }
            return await dataSource.GetMappedListAsync<CompanyResponse>(listParams);
        });
    }

    [KernelFunction("save_company")]
    [Description("Creates a new Company. Updates an existing Company. Only provide value of the fields that need to be changed.")]
    public async Task<string> SaveCompany(CompanyParameter dto)
    {
        if (!_isScoped) return await InvokeScoped<string>(SaveCompany, dto);
        return await Json(async () =>
        {
            var dataSource = dsFactory.GetDefaultDataSource<Coalesce.Domain.Company, Coalesce.Domain.Company>();
            var behaviors = bhFactory.GetBehaviors<Coalesce.Domain.Company>(GeneratedForClassViewModel);
            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;
            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
                return "Creation of Company items not allowed.";
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
                return "Editing of Company items not allowed.";
            return await behaviors.SaveAsync<CompanyParameter, CompanyResponse>(dto, dataSource, new DataSourceParameters());
        });
    }

}
