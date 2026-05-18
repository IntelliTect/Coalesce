using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.MultiTenancy;

/// <summary>
/// Enforces tenant isolation during SaveChanges:
/// <list type="bullet">
///   <item>Sets <c>TenantId</c> on newly added <typeparamref name="TTenanted"/> entities.</item>
///   <item>Throws if <c>TenantId</c> is modified on an existing entity.</item>
/// </list>
/// </summary>
internal class MultiTenancyInterceptor<TTenanted>(string tenantIdPropertyName, Func<DbContext, object?> getTenantId)
    : SaveChangesInterceptor
    where TTenanted : class
{

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context!);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context!);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    private void Apply(DbContext db)
    {
        foreach (var entry in db.ChangeTracker.Entries<TTenanted>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(tenantIdPropertyName).CurrentValue = getTenantId(db);
            }
            else if (entry.State == EntityState.Modified && entry.Property(tenantIdPropertyName).IsModified)
            {
                throw new InvalidOperationException($"Cannot change the {tenantIdPropertyName} of an existing entity.");
            }
        }
    }
}
