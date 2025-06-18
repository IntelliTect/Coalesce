using Coalesce.Web.Vue3.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
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

public class PersonKernelPlugin(
    CrudContext<Coalesce.Domain.AppDbContext> context, 
    IDataSourceFactory dsFactory, 
    Coalesce.Domain.Services.IWeatherService IWeatherService
) : KernelPluginBase<Coalesce.Domain.Person>(context)
{
    protected Coalesce.Domain.AppDbContext Db => context.DbContext;

    [KernelFunction("get_person")]
    [Description("Gets a Person by its PersonId value. test description.")]
    public async Task<ItemResult<PersonResponse>> GetPerson(int personId)
    {
        if (!_isScoped) return await InvokeScoped<ItemResult<PersonResponse>>(GetPerson, personId);
        if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(context.User)) return "Unauthorized.";

        var dataSource = dsFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
        var dataSourceParams = new DataSourceParameters { DataSource = "WithoutCases" };
        return await dataSource.GetMappedItemAsync<PersonResponse>(personId, dataSourceParams);
    }

    [KernelFunction("list_person")]
    [Description("Lists Person records. test description. The search parameter can search on properties FirstName,LastName. The fields parameter should be used if you only need some of the following fields: PersonId,Title,FirstName,LastName,Email,Gender,Height,CasesAssigned,CasesReported,BirthDate,LastBath,NextUpgrade,PersonStats,ProfilePic,Name,CompanyId,Company,ArbitraryCollectionOfStrings")]
    public async Task<string> ListPerson(
        string search,
        int page,
        bool countOnly,
        string[] fields
        , PersonCriteriaParameter personCriteria = default
    )
    {
        if (!_isScoped) return await InvokeScoped<string>(ListPerson, search, page, countOnly, fields, personCriteria);
        if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(context.User)) return Json(new ItemResult(errorMessage: "Unauthorized."));

        var dataSource = (Coalesce.Domain.Person.WithoutCases)dsFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
        MappingContext mappingContext = new(context);
        dataSource.PersonCriteria = personCriteria?.MapToNew(mappingContext);

        var listParams = new ListParameters { DataSource = "WithoutCases", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 };
        if (countOnly)
        {
            var result = await dataSource.GetCountAsync(listParams);
            return Json(new ListResult<object>(result) { TotalCount = result.Object });
        }
        return Json(await dataSource.GetMappedListAsync<PersonResponse>(listParams));
    }

    [KernelFunction("ChangeFirstName")]
    [Description("Changes a person's first name, and optionally assigns a title if they don't yet have one.")]
    public async Task<ItemResult<PersonResponse>> ChangeFirstName(
        int id,
        string firstName,
        Coalesce.Domain.Person.Titles? title)
    {
        if (!_isScoped) return await InvokeScoped<ItemResult<PersonResponse>>(ChangeFirstName, id, firstName, title);
        var _method = GeneratedForClassViewModel!.MethodByName("ChangeFirstName");
        if (!_method.SecurityInfo.IsExecuteAllowed(context.User)) return new ItemResult<PersonResponse>(errorMessage: "Unauthorized");
        var _params = new
        {
            Id = id,
            FirstName = firstName,
            Title = title
        };

        var dataSource = dsFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
        var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
        if (!itemResult.WasSuccessful)
        {
            return new ItemResult<PersonResponse>(itemResult);
        }
        var item = itemResult.Object;
        if (Context.Options.ValidateAttributesForMethods)
        {
            var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
            if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
        }

        IncludeTree includeTree = null;
        var _mappingContext = new MappingContext(Context);
        var _methodResult = item.ChangeFirstName(
            Db,
            _params.FirstName,
            _params.Title
        );
        var _result = new ItemResult<PersonResponse>(_methodResult);
        _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult.Object, _mappingContext, includeTree ?? _methodResult.IncludeTree);
        return _result;
    }

    [KernelFunction("SearchPeople")]
    [Description("Finds people whose birthday falls on a given month, and/or people with a specific email domain.")]
    public async Task<ListResult<PersonResponse>> SearchPeople(
        PersonCriteriaParameter criteria,
        int page)
    {
        if (!_isScoped) return await InvokeScoped<ListResult<PersonResponse>>(SearchPeople, criteria, page);
        var _method = GeneratedForClassViewModel!.MethodByName("SearchPeople");
        if (!_method.SecurityInfo.IsExecuteAllowed(context.User)) return new ListResult<PersonResponse>(errorMessage: "Unauthorized");
        var _params = new
        {
            Criteria = criteria,
            Page = page
        };
        var weather = IWeatherService;

        if (Context.Options.ValidateAttributesForMethods)
        {
            var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
            if (!_validationResult.WasSuccessful) return new ListResult<PersonResponse>(_validationResult);
        }

        IncludeTree includeTree = null;
        var _mappingContext = new MappingContext(Context);
        var _methodResult = Coalesce.Domain.Person.SearchPeople(
            Db,
            _params.Criteria?.MapToNew(_mappingContext),
            _params.Page,
            weather
        );
        var _result = new ListResult<PersonResponse>(_methodResult);
        _result.List = _methodResult.List?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(o, _mappingContext, includeTree ?? _methodResult.IncludeTree)).ToList();
        return _result;
    }

}
