using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

using DescriptionStore = System.Collections.Generic.Dictionary<(Z.EntityFramework.Plus.AuditEntry, string), string?>;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal class CoalesceAudit : Audit
{
    internal DescriptionStore? OldValueDescriptions;
    internal DescriptionStore? NewValueDescriptions;
    private HashSet<(AuditEntry, INavigationBase)>? HasChanged;

    internal async ValueTask PopulateOldDescriptions(DbContext db, bool async)
    {
        foreach (var entry in Entries)
        {
            if (entry.State == Z.EntityFramework.Plus.AuditEntryState.EntityAdded) continue;
            foreach (var refNav in entry.Entry.References)
            {
                if (entry.State == Z.EntityFramework.Plus.AuditEntryState.EntityModified)
                {
                    if (!refNav.IsModified) continue;

                    (HasChanged ??= new()).Add((entry, refNav.Metadata));
                }

                OldValueDescriptions ??= [];
                await PopulateDescriptions(db, entry, refNav, OldValueDescriptions, isNew: false, async);
            }
        }
    }

    internal async ValueTask PopulateNewDescriptions(DbContext db, bool async)
    {
        foreach (var entry in Entries)
        {
            if (entry.State == Z.EntityFramework.Plus.AuditEntryState.EntityDeleted) continue;
            foreach (var refNav in entry.Entry.References)
            {
                if (entry.State == Z.EntityFramework.Plus.AuditEntryState.EntityModified)
                {
                    // We're capturing the new descriptions, but the navigation wasn't modified. Do nothing.
                    // We can't use refNav.IsModified here because we're in the post-save, so it'll always be false.
                    if (HasChanged?.Contains((entry, refNav.Metadata)) != true) continue;
                }

                NewValueDescriptions ??= [];
                await PopulateDescriptions(db, entry, refNav, NewValueDescriptions, isNew: true, async);
            }
        }
    }

    private static async ValueTask PopulateDescriptions(
        DbContext db,
        AuditEntry entry,
        ReferenceEntry refNav,
        DescriptionStore descriptionStore,
        bool isNew,
        bool async)
    {
        var meta = (INavigation)refNav.Metadata;

        if (!meta.ForeignKey.PrincipalKey.IsPrimaryKey())
        {
            // There's not a great way to look up entries by values other than their PK.
            return;
        }

        var auditProp = meta.ForeignKey.Properties
            // For composite FKs, take the last part of the FK
            // as it is the part that is most likely to be specific
            // to the principal entity and not reused.
            // The only scenario where we currently would expect a composite FK is 
            // in a multitenant app where TenantId has been made to be the first part of each FK.
            // FUTURE: We could instead prioritize the FK props that are not [InternalUse],
            // or the props that belong to the fewest number of FKs (TenantId in this example would always belong to at least 2 FKs).
            .Reverse()
            .Select(p => p.Name)
            .FirstOrDefault();

        if (auditProp is null)
        {
            return;
        }

        var targetEntityType = meta.TargetEntityType;
        var targetClrType = targetEntityType.ClrType;

        var targetClassVm = ReflectionRepository.Global.GetOrAddType(targetClrType).ClassViewModel;

        // If the list text for the target is the PK,
        // the description won't be useful as it'll just duplicate the value prop.
        if (targetClassVm?.ListTextProperty is not { IsPrimaryKey: false } targetListText) return;

        object[] keyValues = new object[meta.ForeignKey.Properties.Count];
        int i = 0;
        foreach (var prop in meta.ForeignKey.Properties)
        {
            object? propVal = isNew 
                ? entry.Entry.CurrentValues[prop] 
                : entry.Entry.OriginalValues[prop];

            if (propVal is null)
            {
                // Foreign key value is null, so the principal doesn't exist and we can't get ListText from it.
                return;
            }

            keyValues[i++] = propVal;
        }

        descriptionStore[(entry, auditProp)] = await GetListTextValue(db, targetClrType, keyValues, targetListText.PropertyInfo, async);
    }

    private static async ValueTask<string?> GetListTextValue(DbContext db, Type entityType, object[] keys, PropertyInfo prop, bool async)
    {
        // Conundrum: While it is super easy to just call .Find(),
        // if the entity isn't already tracked then it'll be loaded into the context,
        // which in some incredibly rare scenarios might alter application behavior via
        // navigation fixup in a way that a dev might not expect.

        // We could determine if the entity was not previously loaded and became newly tracked
        // by comparing change tracker length before and after, and then use that information
        // to untrack the entity when we're done.

        //var oldCount = set.Local.Count;
        var entity = async
            ? await db.FindAsync(entityType, keys)
            : db.Find(entityType, keys);
        //var newCount = set.Local.Count;
        if (entity is null) return null;
        var entry = db.Entry(entity);

        if (entry is not null)
        {
            var propMeta = entry.Metadata.FindProperty(prop.Name);
            if (propMeta is not null)
            {
                // In the case where we're getting the post-save value,
                // it is still OK to look at OriginalValues since OriginalValues
                // will be the same as CurrentValues after a save.
                return entry.OriginalValues[propMeta]?.ToString();
            }

            // The property is not mapped with EF.
            // It is likely a getter-only C# property.
            // We can't really compute it with `OriginalValues`,
            // so while this will usually be accurate, there's no guarantee.

            // This calls into user code where it is very possible to have NREs,
            // so we must discard any exceptions.
            try
            {
                return prop.GetValue(entity)?.ToString();
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
