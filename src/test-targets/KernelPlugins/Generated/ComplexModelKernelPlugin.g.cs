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

public class ComplexModelKernelPlugin(CrudContext<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>(context)
{
    protected IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext Db => context.DbContext;

    [KernelFunction("save_complex_model")]
    [Description("Creates a new Complex Model or Updates an existing Complex Model.")]
    public async Task<string> SaveComplexModel(
        [Description("The values to update. Only provide value of the fields that need to be changed.")]
        ComplexModelParameter dto)
    {
        if (!_isScoped) return await InvokeScoped<string>(SaveComplexModel, dto);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>();
            var behaviors = behaviorsFactory.GetBehaviors<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>(GeneratedForClassViewModel);

            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;
            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
                return "Creation of Complex Model items not allowed.";
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
                return "Editing of Complex Model items not allowed.";
            return await behaviors.SaveAsync<ComplexModelParameter, ComplexModelResponse>(dto, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("delete_complex_model")]
    [Description("Deletes an existing Complex Model.")]
    public async Task<string> DeleteComplexModel(
        int complexModelId)
    {
        if (!_isScoped) return await InvokeScoped<string>(DeleteComplexModel, complexModelId);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>();
            var behaviors = behaviorsFactory.GetBehaviors<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>(GeneratedForClassViewModel);

            if (!GeneratedForClassViewModel.SecurityInfo.IsDeleteAllowed(User))
                return "Deleting of Complex Model items not allowed.";
            return await behaviors.DeleteAsync<ComplexModelResponse>(complexModelId, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("method_with_optional_params")]
    [Description("ComplexModel Many Params")]
    public async Task<string> MethodWithOptionalParams(
        int id,
        int requiredInt,
        int plainInt,
        int? nullableInt,
        int intWithDefault = 42,
        IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case.Statuses enumWithDefault = IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case.Statuses.ClosedNoSolution,
        string stringWithDefault = "foo",
        TestParameter optionalObject = default,
        TestParameter[] optionalObjectCollection = default)
    {
        if (!_isScoped) return await InvokeScoped<string>(MethodWithOptionalParams, id, requiredInt, plainInt, nullableInt, intWithDefault, enumWithDefault, stringWithDefault, optionalObject, optionalObjectCollection);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("MethodWithOptionalParams");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<string>(errorMessage: "Unauthorized");
            var _params = new
            {
                Id = id,
                RequiredInt = requiredInt,
                PlainInt = plainInt,
                NullableInt = nullableInt,
                IntWithDefault = intWithDefault,
                EnumWithDefault = enumWithDefault,
                StringWithDefault = stringWithDefault,
                OptionalObject = optionalObject,
                OptionalObjectCollection = optionalObjectCollection
            };

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>("Default");
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
            var _methodResult = item.MethodWithOptionalParams(
                _params.RequiredInt,
                _params.PlainInt,
                _params.NullableInt,
                _params.IntWithDefault,
                _params.EnumWithDefault,
                _params.StringWithDefault,
                _params.OptionalObject?.MapToNew(_mappingContext),
                _params.OptionalObjectCollection?.Select(_m => _m.MapToNew(_mappingContext)).ToArray()
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        });
    }

    [KernelFunction("method_with_string_array_parameter_and_return")]
    [Description("ComplexModel Static")]
    public async Task<string> MethodWithStringArrayParameterAndReturn(
        string[] strings)
    {
        if (!_isScoped) return await InvokeScoped<string>(MethodWithStringArrayParameterAndReturn, strings);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("MethodWithStringArrayParameterAndReturn");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<string[]>(errorMessage: "Unauthorized");
            var _params = new
            {
                Strings = strings
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<string[]>(_validationResult);
            }

            var _methodResult = IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel.MethodWithStringArrayParameterAndReturn(
                _params.Strings.ToArray()
            );
            var _result = new ItemResult<string[]>();
            _result.Object = _methodResult?.ToArray();
            return _result;
        });
    }

    [KernelFunction("method_with_optional_cancellation_token")]
    [Description("MethodWithOptionalCancellationToken")]
    public async Task<string> MethodWithOptionalCancellationToken(
        int id,
        string q,
        System.Threading.CancellationToken cancellationToken = default)
    {
        if (!_isScoped) return await InvokeScoped<string>(MethodWithOptionalCancellationToken, id, q, cancellationToken);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("MethodWithOptionalCancellationToken");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult(errorMessage: "Unauthorized");
            var _params = new
            {
                Id = id,
                Q = q
            };

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            await item.MethodWithOptionalCancellationToken(
                _params.Q,
                cancellationToken
            );
            var _result = new ItemResult();
            return _result;
        });
    }

    [KernelFunction("post_with_implicit_di_parameters")]
    [Description("PostWithImplicitDiParameters")]
    public async Task<string> PostWithImplicitDiParameters(
        System.Threading.CancellationToken cancellationToken,
        int id,
        ExternalTypeWithDtoPropParameter input)
    {
        if (!_isScoped) return await InvokeScoped<string>(PostWithImplicitDiParameters, cancellationToken, id, input);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("PostWithImplicitDiParameters");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult(errorMessage: "Unauthorized");
            var _params = new
            {
                Id = id,
                Input = input
            };

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _mappingContext = new MappingContext(Context);
            await item.PostWithImplicitDiParameters(
                _params.Input?.MapToNew(_mappingContext),
                cancellationToken,
                User,
                Db
            );
            var _result = new ItemResult();
            return _result;
        });
    }

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

            var dataSource = dataSourceFactory.GetDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel>("Default");
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

    [KernelFunction("has_top_level_param_with_same_name_as_object_prop")]
    [Description("HasTopLevelParamWithSameNameAsObjectProp")]
    public async Task<string> HasTopLevelParamWithSameNameAsObjectProp(
        int complexModelId,
        ComplexModelParameter model)
    {
        if (!_isScoped) return await InvokeScoped<string>(HasTopLevelParamWithSameNameAsObjectProp, complexModelId, model);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("HasTopLevelParamWithSameNameAsObjectProp");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult(errorMessage: "Unauthorized");
            var _params = new
            {
                ComplexModelId = complexModelId,
                Model = model
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _mappingContext = new MappingContext(Context);
            var _methodResult = IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ComplexModel.HasTopLevelParamWithSameNameAsObjectProp(
                _params.ComplexModelId,
                _params.Model?.MapToNew(_mappingContext)
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        });
    }
}
