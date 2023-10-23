using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal class AuditExtension : IDbContextOptionsExtension
{
    private readonly AuditOptions _options;
    private DbContextOptionsExtensionInfo? _info;

    public AuditExtension(AuditOptions options)
    {
        _options = options;
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        services.AddSingleton(_options);

        // This doesn't really hurt, and will ensure that DefaultAuditOperationContext can always be constructed
        // in contexts that are isolated from AspNetCore, like usages of the DbContext in unit tests.
        services.AddHttpContextAccessor(); 

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ConventionSetPlugin>();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private class ConventionSetPlugin : IConventionSetPlugin
    {
        public ConventionSet ModifyConventions(ConventionSet conventionSet)
        {
            conventionSet.EntityTypeAddedConventions.Add(new AuditConfigConvention());
            return conventionSet;
        }
    }

    private class AuditConfigConvention : IEntityTypeAddedConvention
    {
        public void ProcessEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext<IConventionEntityTypeBuilder> context)
        {
            if (!entityTypeBuilder.Metadata.ClrType.GetInterfaces().Contains(typeof(IAuditLog)))
            {
                return;
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            var e = new EntityTypeBuilder<IAuditLog>((IMutableEntityType)entityTypeBuilder.Metadata);
#pragma warning restore EF1001 // Internal EF Core API usage.

            e.HasIndex(c => new { c.Type, c.KeyValue });
            e.HasIndex(c => c.State);

            // An index on EntityTypeName is needed by itself because 
            // the index that includes EntityKeyValue is no good when not looking for a specific key
            // (looking at that index requires an index scan rather than a seek).
            e.HasIndex(c => c.Type);
        }
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "";

        public override int GetServiceProviderHashCode() => 0;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;
    }
}
