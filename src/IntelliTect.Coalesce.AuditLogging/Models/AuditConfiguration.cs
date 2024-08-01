using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditConfiguration
{
    public List<Func<PropertyEntry, Func<object, string?>?>> Formatters { get; init; } = new();

    public List<Func<EntityEntry, bool?>> EntityPredicates { get; init; } = new();

    public List<Func<PropertyEntry, bool?>> PropertyPredicates { get; init; } = new();

    /// <summary>
    /// If enabled, properties whose value is <see langword="default"/> are excluded from <see cref="AuditEntryState.EntityAdded"/> audit logs.
    /// </summary>
    public bool IgnoreAddedDefaultValues { get; set; } = true;

    public AuditConfiguration Clone()
    {
        return new()
        {
            IgnoreAddedDefaultValues = IgnoreAddedDefaultValues,
            Formatters = Formatters.ToList(),
            EntityPredicates = EntityPredicates.ToList(),
            PropertyPredicates = PropertyPredicates.ToList(),
        };
    }

    public AuditConfiguration Exclude<T>()
    {
        EntityPredicates.Add((x) => (x.Entity is T) ? false : null);
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

    internal string? GetFormattedValue(PropertyEntry property, object? currentValue)
    {
        if (currentValue == null || currentValue == DBNull.Value)
        {
            return null;
        }

        var propertyName = property.Metadata.Name;

        if (Formatters.Count > 0)
        {
            foreach (var entityValueFormatter in Formatters)
            {
                var formatter = entityValueFormatter(property);
                if (formatter != null)
                {
                    return formatter(currentValue);
                }
            }
        }

        return currentValue.ToString();
    }

    internal bool IsAuditedEntity(EntityEntry entry)
    {
        if (entry.Entity == null || EntityPredicates.Count == 0)
        {
            return true;
        }

        foreach (var predicate in EntityPredicates)
        {
            bool? result = predicate(entry);
            if (result.HasValue)
            {
                return result.Value;
            }
        }

        return true;
    }

    internal bool IsAuditedProperty(PropertyEntry entry)
    {
        if (PropertyPredicates.Count == 0)
        {
            return true;
        }

        foreach (var predicate in PropertyPredicates)
        {
            bool? flag = predicate(entry);
            if (flag.HasValue)
            {
                return flag.Value;
            }
        }

        return true;
    }
}