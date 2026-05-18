using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.MultiTenancy;

internal class MultiTenancyExtension<TTenanted> : IDbContextOptionsExtension
    where TTenanted : class
{
    private readonly string _tenantIdPropertyName;
    private readonly LambdaExpression _tenantIdExpression;
    private readonly string? _providerName;
    private DbContextOptionsExtensionInfo? _info;

    public MultiTenancyExtension(string tenantIdPropertyName, string? providerName, LambdaExpression tenantIdExpression)
    {
        _tenantIdPropertyName = tenantIdPropertyName;
        _providerName = providerName;
        _tenantIdExpression = tenantIdExpression;
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton<IConventionSetPlugin>(
            new ConventionSetPlugin(_tenantIdPropertyName, _providerName, _tenantIdExpression));
    }

    public void Validate(IDbContextOptions options) { }

    private class ConventionSetPlugin(
        string tenantIdPropertyName,
        string? providerName,
        LambdaExpression tenantIdExpression
    ) : IConventionSetPlugin
    {
        public ConventionSet ModifyConventions(ConventionSet conventionSet)
        {
            conventionSet.ModelFinalizingConventions.Add(
                new MultiTenancyConvention<TTenanted>(tenantIdPropertyName, providerName, tenantIdExpression));
            return conventionSet;
        }
    }

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
