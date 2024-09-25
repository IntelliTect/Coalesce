using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

public class CoalesceAudit
{
    internal CoalesceAudit(AuditConfiguration configuration)
    {
        Configuration = configuration;
    }

    public List<AuditEntry> Entries { get; } = [];

    internal AuditConfiguration Configuration { get; }

    private HashSet<(AuditEntry, INavigationBase)>? HasChanged;

    internal void PreSaveChanges(DbContext context)
    {
        var audit = this;
        var config = audit.Configuration;

        context.ChangeTracker.DetectChanges();
        foreach (EntityEntry item in context.ChangeTracker
            .Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
        )
        {
            if (!config.IsAuditedEntity(item)) continue;

            AuditEntry auditEntry;
            switch (item.State)
            {
                case EntityState.Added:
                    audit.Entries.Add(new()
                    {
                        Parent = audit,
                        Entry = item,
                        State = AuditEntryState.EntityAdded
                    });
                    break;

                case EntityState.Deleted:
                    auditEntry = new()
                    {
                        Parent = audit,
                        Entry = item,
                        State = AuditEntryState.EntityDeleted
                    };
                    audit.Entries.Add(auditEntry);

                    auditEntry.Properties = item.Properties
                        .Where(p => audit.Configuration.IsAuditedProperty(p))
                        .Select(p => new AuditEntryProperty
                        {
                            Parent = auditEntry,
                            PropertyEntry = p,
                            OldValue = p.OriginalValue
                        })
                        .ToList();

                    break;

                case EntityState.Modified:
                    auditEntry = new()
                    {
                        Parent = audit,
                        Entry = item,
                        State = AuditEntryState.EntityModified
                    };
                    audit.Entries.Add(auditEntry);

                    auditEntry.Properties = item.Properties
                        .Where(p => 
                            audit.Configuration.IsAuditedProperty(p) && 
                            !object.Equals(p.CurrentValue, p.OriginalValue)
                        )
                        .Select(p => new AuditEntryProperty
                        {
                            Parent = auditEntry,
                            PropertyEntry = p,
                            OldValue = p.OriginalValue
                        })
                        .ToList();

                    break;
            }
        }
    }

    internal void PostSaveChanges()
    {
        foreach (var auditEntry in Entries)
        {
            switch (auditEntry.State)
            {
                case AuditEntryState.EntityAdded:
                    foreach (var propertyEntry in auditEntry.Entry.Properties)
                    {
                        if (!auditEntry.Parent.Configuration.IsAuditedProperty(propertyEntry))
                        {
                            continue;
                        }

                        object? newValue = propertyEntry.CurrentValue;
                        if (newValue == null) continue;

                        if (!auditEntry.Parent.Configuration.KeepAddedDefaultValues &&
                            // Only applicable to types that can't be null,
                            // i.e. non-nullable value tyepes:
                            propertyEntry.Metadata.ClrType.IsValueType && 
                            Nullable.GetUnderlyingType(propertyEntry.Metadata.ClrType) is null
                        )
                        {
                            object defaultValue = Activator.CreateInstance(propertyEntry.Metadata.ClrType)!;
                            if (defaultValue.Equals(propertyEntry.CurrentValue))
                            {
                                continue;
                            }
                        }

                        AuditEntryProperty auditEntryProperty = new()
                        {
                            Parent = auditEntry,
                            PropertyEntry = propertyEntry,
                            NewValue = newValue
                        };
                        auditEntry.Properties.Add(auditEntryProperty);
                    }

                    break;
                case AuditEntryState.EntityModified:
                    if (auditEntry.Entry.State == EntityState.Detached)
                    {
                        auditEntry.State = AuditEntryState.EntityDeleted;
                        auditEntry.Properties.RemoveAll((x) => !x.PropertyEntry.Metadata.IsKey());
                        break;
                    }

                    foreach (var property in auditEntry.Properties)
                    {
                        property.NewValue = property.PropertyEntry.CurrentValue;
                    }

                    break;
            }
        }
    }

    internal async ValueTask PopulateOldDescriptions(DbContext db, bool async)
    {
        foreach (var entry in Entries)
        {
            if (entry.State == AuditEntryState.EntityAdded) continue;
            foreach (var refNav in entry.Entry.References)
            {
                if (entry.State == AuditEntryState.EntityModified)
                {
                    if (!refNav.IsModified) continue;

                    (HasChanged ??= new()).Add((entry, refNav.Metadata));
                }

                await PopulateDescriptions(db, entry, refNav, isNew: false, async);
            }
        }
    }

    internal async ValueTask PopulateNewDescriptions(DbContext db, bool async)
    {
        foreach (var entry in Entries)
        {
            if (entry.State == AuditEntryState.EntityDeleted) continue;
            foreach (var refNav in entry.Entry.References)
            {
                if (entry.State == AuditEntryState.EntityModified)
                {
                    // We're capturing the new descriptions, but the navigation wasn't modified. Do nothing.
                    // We can't use refNav.IsModified here because we're in the post-save, so it'll always be false.
                    if (HasChanged?.Contains((entry, refNav.Metadata)) != true) continue;
                }

                await PopulateDescriptions(db, entry, refNav, isNew: true, async);
            }
        }
    }

    private static async ValueTask PopulateDescriptions(
        DbContext db,
        AuditEntry entry,
        ReferenceEntry refNav,
        bool isNew,
        bool async)
    {
        var meta = (INavigation)refNav.Metadata;
        var targetEntityType = meta.TargetEntityType;

        if (
            // There's not a great way to look up entries by values other than their PK.
            !meta.ForeignKey.PrincipalKey.IsPrimaryKey() ||
            // The target is an owned type, meaning it belongs to the same table as `entry`,
            // so there's no foreign entry to look up
            targetEntityType.IsOwned()
        )
        {
            return;
        }

        var auditProp = meta.ForeignKey.Properties
            // For composite FKs, take the last part of the FK
            // as it is the part that is most likely to be specific
            // to the principal entity and not reused.
            // The only scenario where we currently would expect a composite FK is 
            // in a multitenant app where TenantId has been made to be the first part of each FK
            // in a way that is invisible to Coalesce, since Coalesce otherwise doesn't support composite keys.
            // FUTURE: We could instead prioritize the FK props that are not [InternalUse],
            // or the props that belong to the fewest number of FKs (TenantId in this example would always belong to at least 2 FKs).
            .Reverse()
            .FirstOrDefault();

        if (auditProp is null) return;

        var auditPropEntry = entry.Properties.FirstOrDefault(p => p.PropertyEntry.Metadata == auditProp);

        if (auditPropEntry is null) return;

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

        var description = await GetListTextValue(db, targetClrType, keyValues, targetListText.PropertyInfo, async);
        if (isNew)
        {
            auditPropEntry.NewValueDescription = description;
        }
        else
        {
            auditPropEntry.OldValueDescription = description;
        }
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
