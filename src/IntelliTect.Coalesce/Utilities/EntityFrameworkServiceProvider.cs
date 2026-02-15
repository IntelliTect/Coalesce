using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.Utilities;

internal class EntityFrameworkServiceProvider(DbContext db) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        // From Microsoft.EntityFrameworkCore.Infrastructure.Internal.InfrastructureExtensions;
        // Copied directly to avoid having to dynamically create generic overloads at runtime,
        // and also because we don't want to throw for services not found - we want to return null.

        var internalServiceProvider = db.GetInfrastructure();

        // Prefer services ApplicationServiceProvider when possible,
        // since that's where developers will generally expect the things they inject into extensions to come from.
        return internalServiceProvider.GetService<IDbContextOptions>()
                ?.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault()
                ?.ApplicationServiceProvider
                ?.GetService(serviceType)
            ?? internalServiceProvider.GetService(serviceType);
    }
}
