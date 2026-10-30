using System.Collections.Generic;
using System.Linq;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;

namespace IntelliTect.Coalesce.Api;

public class BulkSaveRequest
{
    public List<BulkSaveRequestItem> Items { get; set; } = new();

    internal IEnumerable<BulkSaveRequestItem> Save => Items.Where(i => i.Action == "save");
    internal IEnumerable<BulkSaveRequestItem> Delete => Items.Where(i => i.Action == "delete");
    internal IEnumerable<BulkSaveRequestItem> None => Items.Where(i => i.Action == "none");

    private Dictionary<int, BulkSaveRequestItem>? _RefsLookup;
    internal Dictionary<int, BulkSaveRequestItem> RefsLookup
    {
        get
        {
            if (_RefsLookup is not null) return _RefsLookup;

            _RefsLookup = new();
            foreach (var item in Items)
            {
                if (item.PrimaryRef is int val)
                {
                    _RefsLookup[val] = item;
                }
            }
            return _RefsLookup;
        }
    }

    /// <summary>
    /// Splits <see cref="Delete"/> into deletes that run before the saves and deletes that run after.
    /// </summary>
    internal (List<BulkSaveRequestItem> Early, List<BulkSaveRequestItem> Late) PartitionDeletes()
    {
        static bool IsPrincipalOf(BulkSaveRequestItem principal, ClassViewModel dependent) => dependent
            .Properties
            .Any(p => p.Role == PropertyRole.ForeignKey
                && p.ForeignKeyPrincipalType is { } principalType
                && principal.ClassViewModel.Type.IsA(principalType.Type));

        var deletes = Delete.ToList();
        var saveTypes = Save.Select(i => i.ClassViewModel).Distinct().ToList();

        var late = deletes
            .Where(d => saveTypes.Any(t => IsPrincipalOf(d, t)))
            .ToHashSet();

        // A delete must not run before a delete of one of its dependents.
        bool changed;
        do
        {
            changed = false;
            foreach (var item in deletes)
            {
                if (!late.Contains(item) && late.Any(l => IsPrincipalOf(item, l.ClassViewModel)))
                {
                    late.Add(item);
                    changed = true;
                }
            }
        } while (changed);

        return (
            deletes.Where(d => !late.Contains(d)).ToList(),
            deletes.Where(late.Contains).ToList()
        );
    }
}
