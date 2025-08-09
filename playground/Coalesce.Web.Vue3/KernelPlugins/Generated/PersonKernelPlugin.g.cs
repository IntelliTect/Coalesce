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

public class PersonKernelPlugin(CrudContext<Coalesce.Domain.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory, Coalesce.Domain.Services.IWeatherService IWeatherService) : KernelPluginBase<Coalesce.Domain.Person>(context)
{
    protected Coalesce.Domain.AppDbContext Db => context.DbContext;

    [KernelFunction("save_person")]
    [Description("Creates a new Person or Updates an existing Person.")]
    public async Task<string> SavePerson(
        [Description("The values to update. Only provide value of the fields that need to be changed.")]
        PersonParameter dto)
    {
        if (!_isScoped) return await InvokeScoped<string>(SavePerson, dto);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>();
            var behaviors = behaviorsFactory.GetBehaviors<Coalesce.Domain.Person>(GeneratedForClassViewModel);

            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;
            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
                return "Creation of Person items not allowed.";
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
                return "Editing of Person items not allowed.";
            return await behaviors.SaveAsync<PersonParameter, PersonResponse>(dto, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("delete_person")]
    [Description("Deletes an existing Person.")]
    public async Task<string> DeletePerson(
        int personId)
    {
        if (!_isScoped) return await InvokeScoped<string>(DeletePerson, personId);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>();
            var behaviors = behaviorsFactory.GetBehaviors<Coalesce.Domain.Person>(GeneratedForClassViewModel);

            if (!GeneratedForClassViewModel.SecurityInfo.IsDeleteAllowed(User))
                return "Deleting of Person items not allowed.";
            return await behaviors.DeleteAsync<PersonResponse>(personId, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("get_person_names_starting_with_a_with_cases")]
    [Description("Gets a Person by its PersonId value. Retrieves people whose first name start with 'A', and their cases.")]
    public async Task<string> GetPersonNamesStartingWithAWithCases(
        int personId)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetPersonNamesStartingWithAWithCases, personId);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var _dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("NamesStartingWithAWithCases");
            var _dataSourceParams = new DataSourceParameters { DataSource = "NamesStartingWithAWithCases" };
            return await _dataSource.GetMappedItemAsync<PersonResponse>(personId, _dataSourceParams);
        });
    }

    [KernelFunction("list_person_names_starting_with_a_with_cases")]
    [Description("Lists Person records. Retrieves people whose first name start with 'A', and their cases.")]
    public async Task<string> ListPersonNamesStartingWithAWithCases(
        [Description("Search within properties FirstName,LastName")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: PersonId,Title,FirstName,LastName,Email,Gender,Height,CasesAssigned,CasesReported,BirthDate,LastBath,NextUpgrade,PersonStats,ProfilePic,Name,CompanyId,Company,ArbitraryCollectionOfStrings")]
        string[] fields,
        [Description("Filter the cases returned to only those with these statuses.")]
        System.Collections.Generic.ICollection<Coalesce.Domain.Case.Statuses> allowedStatuses = default)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListPersonNamesStartingWithAWithCases, search, page, countOnly, fields, allowedStatuses);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<PersonResponse>(errorMessage: "Unauthorized.");

            var _dataSource = (Coalesce.Domain.NamesStartingWithAWithCases)dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("NamesStartingWithAWithCases");
            MappingContext _mappingContext = new(context);
            _dataSource.AllowedStatuses = allowedStatuses?.ToList();

            if (ItemResult.FromValidation(_dataSource) is { WasSuccessful: false } _validationResult)
                return new ListResult<PersonResponse>(_validationResult);

            var _listParams = new ListParameters { DataSource = "NamesStartingWithAWithCases", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 25 };
            if (countOnly)
            {
                var result = await _dataSource.GetCountAsync(_listParams);
                return new ListResult<PersonResponse>(result) { TotalCount = result.Object };
            }
            return await _dataSource.GetMappedListAsync<PersonResponse>(_listParams);
        });
    }

    [KernelFunction("get_person_without_cases")]
    [Description("Gets a Person by its PersonId value.")]
    public async Task<string> GetPersonWithoutCases(
        int personId)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetPersonWithoutCases, personId);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var _dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
            var _dataSourceParams = new DataSourceParameters { DataSource = "WithoutCases" };
            return await _dataSource.GetMappedItemAsync<PersonResponse>(personId, _dataSourceParams);
        });
    }

    [KernelFunction("list_person_without_cases")]
    [Description("Lists Person records.")]
    public async Task<string> ListPersonWithoutCases(
        [Description("Search within properties FirstName,LastName")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: PersonId,Title,FirstName,LastName,Email,Gender,Height,CasesAssigned,CasesReported,BirthDate,LastBath,NextUpgrade,PersonStats,ProfilePic,Name,CompanyId,Company,ArbitraryCollectionOfStrings")]
        string[] fields,
        [Description("A set of parameters that are ignored entirely.")]
        PersonCriteriaParameter personCriteria = default)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListPersonWithoutCases, search, page, countOnly, fields, personCriteria);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<PersonResponse>(errorMessage: "Unauthorized.");

            var _dataSource = (Coalesce.Domain.Person.WithoutCases)dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
            MappingContext _mappingContext = new(context);
            _dataSource.PersonCriteria = personCriteria?.MapToNew(_mappingContext);

            if (ItemResult.FromValidation(_dataSource) is { WasSuccessful: false } _validationResult)
                return new ListResult<PersonResponse>(_validationResult);

            var _listParams = new ListParameters { DataSource = "WithoutCases", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 25 };
            if (countOnly)
            {
                var result = await _dataSource.GetCountAsync(_listParams);
                return new ListResult<PersonResponse>(result) { TotalCount = result.Object };
            }
            return await _dataSource.GetMappedListAsync<PersonResponse>(_listParams);
        });
    }

    [KernelFunction("change_first_name")]
    [Description("Changes a person's first name, and optionally assigns a title if they don't yet have one.")]
    public async Task<string> ChangeFirstName(
        int id,
        string firstName,
        [Description("A new title for the person. Provide null to leave the title unchanged.")]
        Coalesce.Domain.Person.Titles? title)
    {
        if (!_isScoped) return await InvokeScoped<string>(ChangeFirstName, id, firstName, title);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("ChangeFirstName");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<PersonResponse>(errorMessage: "Unauthorized");
            var _params = new
            {
                Id = id,
                FirstName = firstName,
                Title = title
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
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
        });
    }

    [KernelFunction("search_people")]
    [Description("Finds people whose birthday falls on a given month, and/or people with a specific email domain.")]
    public async Task<string> SearchPeople(
        PersonCriteriaParameter criteria,
        int page)
    {
        if (!_isScoped) return await InvokeScoped<string>(SearchPeople, criteria, page);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("SearchPeople");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ListResult<PersonResponse>(errorMessage: "Unauthorized");
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
        });
    }
}
