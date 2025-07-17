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

public class PersonKernelPlugin(CrudContext<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Person>(context)
{
    protected IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext Db => context.DbContext;

    [KernelFunction("get_person")]
    [Description("Gets a Person by its PersonId value. ParameterTestsSource.")]
    public async Task<string> GetPerson(
        int personId)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetPerson, personId);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return "Unauthorized.";

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Person, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Person>("ParameterTestsSource");
            var dataSourceParams = new DataSourceParameters { DataSource = "ParameterTestsSource" };
            return await dataSource.GetMappedItemAsync<PersonResponse>(personId, dataSourceParams);
        });
    }

    [KernelFunction("list_person")]
    [Description("Lists Person records. ParameterTestsSource.")]
    public async Task<string> ListPerson(
        [Description("Search within properties FirstName,LastName")]
        string search,
        [Description("Provide values greater than 1 to query subsequent pages of data")]
        int page,
        [Description("Provide true if you only need a count of results.")]
        bool countOnly,
        [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: PersonId,Title,FirstName,LastName,Email,SecretPhrase,Gender,CasesAssigned,CasesReported,BirthDate,Name,CompanyId,Company,SiblingRelationships")]
        string[] fields,
        PersonCriteriaParameter personCriterion = default,
        PersonCriteriaParameter[] personCriteriaArray = default,
        System.Collections.Generic.ICollection<PersonCriteriaParameter> personCriteriaList = default,
        System.Collections.Generic.ICollection<PersonCriteriaParameter> personCriteriaICollection = default,
        int[] intArray = default,
        System.Collections.Generic.ICollection<int> intList = default,
        System.Collections.Generic.ICollection<int> intICollection = default,
        byte[] bytes = default)
    {
        if (!_isScoped) return await InvokeScoped<string>(ListPerson, search, page, countOnly, fields, personCriterion, personCriteriaArray, personCriteriaList, personCriteriaICollection, intArray, intList, intICollection, bytes);

        return await Json(async () =>
        {
            if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new ListResult<PersonResponse>(errorMessage: "Unauthorized.");

            var dataSource = (IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ParameterTestsSource)dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Person, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Person>("ParameterTestsSource");
            MappingContext mappingContext = new(context);
            dataSource.PersonCriterion = personCriterion?.MapToNew(mappingContext);
            dataSource.PersonCriteriaArray = personCriteriaArray?.Select(_m => _m.MapToNew(mappingContext)).ToArray();
            dataSource.PersonCriteriaList = personCriteriaList?.Select(_m => _m.MapToNew(mappingContext)).ToList();
            dataSource.PersonCriteriaICollection = personCriteriaICollection?.Select(_m => _m.MapToNew(mappingContext)).ToList();
            dataSource.IntArray = intArray.ToArray();
            dataSource.IntList = intList.ToList();
            dataSource.IntICollection = intICollection.ToList();
            dataSource.Bytes = bytes;

            var listParams = new ListParameters { DataSource = "ParameterTestsSource", Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 };
            if (countOnly)
            {
                var result = await dataSource.GetCountAsync(listParams);
                return new ListResult<PersonResponse>(result) { TotalCount = result.Object };
            }
            return await dataSource.GetMappedListAsync<PersonResponse>(listParams);
        });
    }
}
