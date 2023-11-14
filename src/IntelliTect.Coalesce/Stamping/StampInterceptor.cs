using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Stamping;

internal class StampInterceptor<TStampable> : SaveChangesInterceptor
    where TStampable : class
{
    public Delegate Action { get; }
    public Type[] Services { get; }

    public StampInterceptor(Delegate action, Type[] services)
    {
        Action = action;
        Services = services;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(SavingChanges(eventData, result));
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var db = eventData.Context!;
        var provider = new EntityFrameworkServiceProvider(db);

        var services = Services
            .Select(t => t.IsAssignableFrom(db.GetType())
                ? db
                : provider.GetService(t))
            .ToArray();

        var entities = db.ChangeTracker
            .Entries<TStampable>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entities)
        {
            Action.DynamicInvoke([entry.Entity, .. services]);
        }

        return result;
    }
}

internal sealed class StampInterceptor<TStampable, TService> : StampInterceptor<TStampable>
    where TStampable : class
{
    public StampInterceptor(Action<TStampable, TService?> action)
        : base(action, [typeof(TService)]) { }
}

internal sealed class StampInterceptor<TStampable, TService1, TService2> : StampInterceptor<TStampable>
    where TStampable : class
{
    public StampInterceptor(Action<TStampable, TService1?, TService2?> action)
        : base(action, [typeof(TService1), typeof(TService2)]) { }
}
