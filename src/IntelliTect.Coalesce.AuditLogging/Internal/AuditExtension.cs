using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal class AuditExtension<TObjectChange> : IDbContextOptionsExtension
    where TObjectChange : class, IObjectChange
{
    private DbContextOptionsExtensionInfo? _info;
    private Type? _operationContextType;

    public AuditExtension(Type? operationContextType = null)
    {
        _operationContextType = operationContextType;
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        if (_operationContextType is not null)
        {
            services.TryAddScoped(typeof(IAuditOperationContext<TObjectChange>), _operationContextType);
        }

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ConventionSetPlugin>();
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
            if (!entityTypeBuilder.Metadata.ClrType.GetInterfaces().Contains(typeof(IObjectChange)))
            {
                return;
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            var e = new EntityTypeBuilder<TObjectChange>((IMutableEntityType)entityTypeBuilder.Metadata);
#pragma warning restore EF1001 // Internal EF Core API usage.

            e.HasIndex(c => new { c.EntityTypeName, c.EntityKeyValue });
            e.HasIndex(c => c.State);

            // An index on EntityTypeName is needed by itself because 
            // the index that includes EntityKeyValue is no good when not looking for a specific key
            // (looking at that index requires an index scan rather than a seek).
            e.HasIndex(c => c.EntityTypeName);

            e.HasMany(c => c.Properties)
                .WithOne()
                .HasPrincipalKey(c => c.ObjectChangeId)
                .HasForeignKey(c => c.ObjectChangeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public void Validate(IDbContextOptions options)
    {
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
