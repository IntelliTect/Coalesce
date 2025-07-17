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

public class CompanyKernelPlugin(CrudContext<Coalesce.Domain.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<Coalesce.Domain.Company>(context)
{
    protected Coalesce.Domain.AppDbContext Db => context.DbContext;

    [KernelFunction("get_company")]
    [Description("Gets a Company by its Id value. .")]
    public async Task<string> GetCompany(
        int id)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetCompany, id);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Company, Coalesce.Domain.Company>("DefaultSource");
            var dataSourceParams = new DataSourceParameters { DataSource = "DefaultSource" };
            return await dataSource.GetMappedItemAsync<CompanyResponse>(id, dataSourceParams);
        });
    }

    [KernelFunction("list_company")]
    [Description("Lists Company records. .")]
    public async Task<string> ListCompany(
        [Description("Search within properties Name,LogoUrl")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: Id,Name,Address1,Address2,City,State,ZipCode,Phone,WebsiteUrl,LogoUrl,IsDeleted,Employees,AltName")]
        string[] fields)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListCompany, search, page, countOnly, fields);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<CompanyResponse>(errorMessage: "Unauthorized.");

            var _dataSource = (Coalesce.Domain.Company.DefaultSource)dataSourceFactory.GetDataSource<Coalesce.Domain.Company, Coalesce.Domain.Company>("DefaultSource");
            MappingContext _mappingContext = new(context);

            var listParams = new ListParameters { DataSource = "DefaultSource", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 };
            if (countOnly)
            {
                var result = await _dataSource.GetCountAsync(listParams);
                return new ListResult<CompanyResponse>(result) { TotalCount = result.Object };
            }
            return await _dataSource.GetMappedListAsync<CompanyResponse>(listParams);
        });
    }
}
