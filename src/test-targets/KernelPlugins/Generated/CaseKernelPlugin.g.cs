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

public class CaseKernelPlugin(CrudContext<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case>(context)
{
    protected IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext Db => context.DbContext;

    [KernelFunction("method_with_js_reserved_param_name")]
    [Description("MethodWithJsReservedParamName")]
    public async Task<string> MethodWithJsReservedParamName(
        int id,
        CaseParameter @case,
        string function,
        int @var,
        bool @async,
        string @await,
        string[] arguments,
        string implements,
        bool delete,
        bool @true)
    {
        if (!_isScoped) return await InvokeScoped<string>(MethodWithJsReservedParamName, id, @case, function, @var, @async, @await, arguments, implements, delete, @true);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("MethodWithJsReservedParamName");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<string>(errorMessage: "Unauthorized");
            var _params = new
            {
                Id = id,
                Case = @case,
                Function = function,
                Var = @var,
                Async = @async,
                Await = @await,
                Arguments = arguments,
                Implements = implements,
                Delete = delete,
                True = @true
            };

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<string>(_validationResult);
            }

            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.MethodWithJsReservedParamName(
                _params.Case?.MapToNew(_mappingContext),
                _params.Function,
                _params.Var,
                _params.Async,
                _params.Await,
                _params.Arguments.ToArray(),
                _params.Implements,
                _params.Delete,
                _params.True
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        });
    }
}
