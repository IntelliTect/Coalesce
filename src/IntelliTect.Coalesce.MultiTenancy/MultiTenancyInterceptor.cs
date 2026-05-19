using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq.Expressions;
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
    private Func<DbContext, object?>? _cachedGetter;

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
        _cachedGetter ??= BuildGetter(db.GetType());

        foreach (var entry in db.ChangeTracker.Entries<TTenanted>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(entityTenantIdPropertyName).CurrentValue = _cachedGetter(db);
            }
            else if (entry.State == EntityState.Modified && entry.Property(entityTenantIdPropertyName).IsModified)
            {
                throw new InvalidOperationException($"Cannot change the {entityTenantIdPropertyName} of an existing entity.");
            }
        }
    }

    private Func<DbContext, object?> BuildGetter(Type dbContextType)
    {
        var propInfo = dbContextType.GetProperty(contextTenantIdPropertyName)
            ?? throw new InvalidOperationException(
                $"Property '{contextTenantIdPropertyName}' not found on {dbContextType.Name}.");

        var param = Expression.Parameter(typeof(DbContext), "db");
        return Expression.Lambda<Func<DbContext, object?>>(
            Expression.Convert(
                Expression.MakeMemberAccess(
                    Expression.Convert(param, dbContextType), propInfo),
                typeof(object)),
            param
        ).Compile();
    }
}
