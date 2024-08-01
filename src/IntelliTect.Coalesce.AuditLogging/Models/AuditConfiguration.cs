using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditConfiguration
{
    internal List<Func<PropertyEntry, Func<object, string?>?>> Formatters { get; init; } = new();

    internal List<Func<EntityEntry, bool?>> EntityPredicates { get; init; } = new();

    internal List<Func<PropertyEntry, bool?>> PropertyPredicates { get; init; } = new();

    /// <summary>
    /// If enabled, properties whose value is <see langword="default"/> are excluded from <see cref="AuditEntryState.EntityAdded"/> audit logs.
    /// </summary>
    internal bool KeepAddedDefaultValues { get; set; }

    public AuditConfiguration Clone()
    {
        return new()
        {
            KeepAddedDefaultValues = KeepAddedDefaultValues,
            Formatters = Formatters.ToList(),
            EntityPredicates = EntityPredicates.ToList(),
            PropertyPredicates = PropertyPredicates.ToList(),
        };
    }

    /// <summary>
    /// If enabled, properties whose value is <see langword="default"/> are included in <see cref="AuditEntryState.EntityAdded"/> audit logs.
    /// Defaults to <see langword="false"/> (default values are excluded on add).
    /// </summary>
    public AuditConfiguration IncludeAddedDefaultValues(bool include = true)
    {
        KeepAddedDefaultValues = include;
        return this;
    }

    public AuditConfiguration Include<T>()
    {
        EntityPredicates.Add((x) => (x.Entity is T) ? true : null);
        return this;
    }

    public AuditConfiguration Exclude<T>()
    {
        EntityPredicates.Add((x) => (x.Entity is T) ? false : null);
        return this;
    }

    public AuditConfiguration Include(Func<EntityEntry, bool> predicate)
    {
        EntityPredicates.Add((x) => predicate(x) ? true : null);
        return this;
    }

    public AuditConfiguration Exclude(Func<EntityEntry, bool> predicate)
    {
        EntityPredicates.Add((x) => predicate(x) ? false : null);
        return this;
    }

    public AuditConfiguration ExcludeProperty<T>(Expression<Func<T, object?>> propertySelector)
    {
        var props = propertySelector.GetExpressedProperties<T>().Select(p => p.Name).ToArray();
        return ExcludeProperty<T>(props);
    }

    public AuditConfiguration ExcludeProperty<T>(params string[] propNames)
    {
        var props = propNames.ToHashSet();

        PropertyPredicates.Add((e) => e.EntityEntry.Entity is T && props.Contains(e.Metadata.Name) ? false : null);

        return this;
    }

    public AuditConfiguration Format<T>(Expression<Func<T, object?>> propertySelector, Func<object, string?> formatter) where T : class
    {
        var props = propertySelector.GetExpressedProperties<T>().Select(p => p.Name).ToArray();
        return Format<T>(props, formatter);
    }

    public AuditConfiguration Format<T>(IEnumerable<string> propNames, Func<object, string?> formatter)
    {
        var props = propNames.ToHashSet();

        Formatters.Add((e) => e.EntityEntry.Entity is T && props.Contains(e.Metadata.Name) ? formatter : null);

        return this;
    }

    public AuditConfiguration FormatType<T>(Func<T, string?> formatter)
    {
        var wrappedFormatter = (object v) => formatter((T)v);

        Formatters.Add((e) => e.Metadata.ClrType.IsAssignableTo(typeof(T)) ? wrappedFormatter : null);

        return this;
    }

    internal string? GetFormattedValue(PropertyEntry property, object? currentValue)
    {
        if (currentValue == null || currentValue == DBNull.Value) return null;

        // Reverse iteration makes the last defined rule win.
        for (int i = Formatters.Count - 1; i >= 0; i--)
        {
            var formatter = Formatters[i](property);
            if (formatter != null)
            {
                return formatter(currentValue);
            }
        }

        return currentValue.ToString();
    }

    internal bool IsAuditedEntity(EntityEntry entry)
    {
        // Reverse iteration makes the last defined rule win.
        for (int i = EntityPredicates.Count - 1; i >= 0; i--)
        {
            bool? result = EntityPredicates[i](entry);
            if (result.HasValue)
            {
                return result.Value;
            }
        }

        return true;
    }

    internal bool IsAuditedProperty(PropertyEntry entry)
    {
        // Reverse iteration makes the last defined rule win.
        for (int i = PropertyPredicates.Count - 1; i >= 0; i--)
        {
            bool? flag = PropertyPredicates[i](entry);
            if (flag.HasValue)
            {
                return flag.Value;
            }
        }

        return true;
    }
}