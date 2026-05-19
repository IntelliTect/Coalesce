using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.MultiTenancy;

internal class MultiTenancyExtension<TTenanted> : IDbContextOptionsExtension
    where TTenanted : class
{
    private readonly string _entityTenantIdPropertyName;
    private readonly string _contextTenantIdPropertyName;
    private DbContextOptionsExtensionInfo? _info;

    public MultiTenancyExtension(string entityTenantIdPropertyName, string contextTenantIdPropertyName)
    {
        _entityTenantIdPropertyName = entityTenantIdPropertyName;
        _contextTenantIdPropertyName = contextTenantIdPropertyName;
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton(new MultiTenancyOptions(_entityTenantIdPropertyName, _contextTenantIdPropertyName));

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ConventionSetPlugin<TTenanted>>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;
        public override string LogFragment => "using MultiTenancy ";
        public override int GetServiceProviderHashCode() => 0;
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["Coalesce:MultiTenancy"] = "1";
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;
    }
}

internal record MultiTenancyOptions(string EntityTenantIdPropertyName, string ContextTenantIdPropertyName);

internal class ConventionSetPlugin<TTenanted>(
    ICurrentDbContext currentDbContext,
    MultiTenancyOptions options
) : IConventionSetPlugin
    where TTenanted : class
{
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        var dbContext = currentDbContext.Context;
        var dbContextType = dbContext.GetType();
        var providerName = dbContext.Database.ProviderName;

        conventionSet.ModelFinalizingConventions.Add(
            new MultiTenancyConvention<TTenanted>(
                options.EntityTenantIdPropertyName,
                providerName,
                dbContextType,
                options.ContextTenantIdPropertyName));

        return conventionSet;
    }
}
