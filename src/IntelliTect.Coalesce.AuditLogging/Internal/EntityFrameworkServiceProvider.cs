using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal class EntityFrameworkServiceProvider(DbContext db) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        // From Microsoft.EntityFrameworkCore.Infrastructure.Internal.InfrastructureExtensions;
        // Copied directly to avoid having to dynamically create generic overloads at runtime,
        // and also because we don't want to throw for services not found - we want to return null.

        var internalServiceProvider = db.GetInfrastructure();

        return internalServiceProvider.GetService(serviceType)
            ?? internalServiceProvider.GetService<IDbContextOptions>()
                ?.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault()
                ?.ApplicationServiceProvider
                ?.GetService(serviceType);
    }
}
