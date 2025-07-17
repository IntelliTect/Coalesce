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

public class StandaloneReadWriteKernelPlugin(CrudContext context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadWrite>(context)
{

    [KernelFunction("save_standalone_read_write")]
    [Description("Creates a new Standalone Read Write or Updates an existing Standalone Read Write.")]
    public async Task<string> SaveStandaloneReadWrite(
        [Description("The values to update. Only provide value of the fields that need to be changed.")]
        StandaloneReadWriteParameter dto)
    {
        if (!_isScoped) return await InvokeScoped<string>(SaveStandaloneReadWrite, dto);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadWrite, IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadWrite>();
            var behaviors = behaviorsFactory.GetBehaviors<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadWrite>(GeneratedForClassViewModel);

            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;
            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
                return "Creation of Standalone Read Write items not allowed.";
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
                return "Editing of Standalone Read Write items not allowed.";
            return await behaviors.SaveAsync<StandaloneReadWriteParameter, StandaloneReadWriteResponse>(dto, dataSource, new DataSourceParameters());
        });
    }

    [KernelFunction("delete_standalone_read_write")]
    [Description("Deletes an existing Standalone Read Write.")]
    public async Task<string> DeleteStandaloneReadWrite(
        int id)
    {
        if (!_isScoped) return await InvokeScoped<string>(DeleteStandaloneReadWrite, id);

        return await Json(async () =>
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadWrite, IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadWrite>();
            var behaviors = behaviorsFactory.GetBehaviors<IntelliTect.Coalesce.Tests.TargetClasses.StandaloneReadWrite>(GeneratedForClassViewModel);

            if (!GeneratedForClassViewModel.SecurityInfo.IsDeleteAllowed(User))
                return "Deleting of Standalone Read Write items not allowed.";
            return await behaviors.DeleteAsync<StandaloneReadWriteResponse>(id, dataSource, new DataSourceParameters());
        });
    }
}
