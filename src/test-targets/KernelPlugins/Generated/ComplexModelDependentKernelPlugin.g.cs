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

public class ComplexModelDependentKernelPlugin(CrudContext<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModelDependent>(context)
{
    protected IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext Db => context.DbContext;

    [KernelFunction("same_method_name_as_method_on_different_type")]
    [Description("SameMethodNameAsMethodOnDifferentType")]
    public async Task<string> SameMethodNameAsMethodOnDifferentType(
        int id,
        IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone input)
    {
        if (!_isScoped) return await InvokeScoped<string>(SameMethodNameAsMethodOnDifferentType, id, input);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("SameMethodNameAsMethodOnDifferentType");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>(errorMessage: "Unauthorized");
            var _params = new
            {
                Id = id,
                Input = input
            };

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModelDependent, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModelDependent>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>(_validationResult);
            }

            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.SameMethodNameAsMethodOnDifferentType(
                _params.Input
            );
            var _result = new ItemResult<IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>();
            _result.Object = _methodResult;
            return _result;
        });
    }
}
