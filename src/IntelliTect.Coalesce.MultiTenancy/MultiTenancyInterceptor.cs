using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Reflection;
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
internal class MultiTenancyInterceptor<TTenanted>(string entityTenantIdPropertyName, string contextTenantIdPropertyName)
    : SaveChangesInterceptor
    where TTenanted : class
{
    private PropertyInfo? _cachedPropInfo;

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
        _cachedPropInfo ??= db.GetType().GetProperty(contextTenantIdPropertyName)
            ?? throw new InvalidOperationException(
                $"Property '{contextTenantIdPropertyName}' not found on {db.GetType().Name}.");

        foreach (var entry in db.ChangeTracker.Entries<TTenanted>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(entityTenantIdPropertyName).CurrentValue = _cachedPropInfo.GetValue(db);
            }
            else if (entry.State == EntityState.Modified && entry.Property(entityTenantIdPropertyName).IsModified)
            {
                throw new InvalidOperationException($"Cannot change the {entityTenantIdPropertyName} of an existing entity.");
            }
        }
    }
}
