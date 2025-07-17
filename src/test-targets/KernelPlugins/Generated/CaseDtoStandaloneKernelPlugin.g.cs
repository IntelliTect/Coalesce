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

public class CaseDtoStandaloneKernelPlugin(CrudContext<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext> context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>(context)
{
    protected IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext Db => context.DbContext;

    [KernelFunction("save_case_dto_standalone")]
    [Description("Creates a new Case Dto Standalone or Updates an existing Case Dto Standalone.")]
    public async Task<string> SaveCaseDtoStandalone(
        [Description("Provide the value of all fields, even those that are not being changed.")]
        IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone dto)
    {
        if (!_isScoped) return await InvokeScoped<string>(SaveCaseDtoStandalone, dto);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case, IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>();
            var behaviors = behaviorsFactory.GetBehaviors<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case>(GeneratedForClassViewModel);

            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;
            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
                return "Creation of Case Dto Standalone items not allowed.";
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
                return "Editing of Case Dto Standalone items not allowed.";
            return await behaviors.SaveAsync<IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone, IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>(dto, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("delete_case_dto_standalone")]
    [Description("Deletes an existing Case Dto Standalone.")]
    public async Task<string> DeleteCaseDtoStandalone(
        int caseId)
    {
        if (!_isScoped) return await InvokeScoped<string>(DeleteCaseDtoStandalone, caseId);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case, IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>();
            var behaviors = behaviorsFactory.GetBehaviors<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case>(GeneratedForClassViewModel);

            if (!GeneratedForClassViewModel.SecurityInfo.IsDeleteAllowed(User))
                return "Deleting of Case Dto Standalone items not allowed.";
            return await behaviors.DeleteAsync<IntelliTect.Coalesce.Tests.TargetClasses.CaseDtoStandalone>(caseId, dataSource, new DataSourceParameters());
        });
    }
}
